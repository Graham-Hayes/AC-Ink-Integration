using UnityEngine;
using System;
using System.Collections.Generic;
using Ink.Runtime;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
    [System.Serializable]
    public class ActionInkRunScript : Action
    {
        public bool newStory;
        public string knot;
        public List<Char> actors = new List<Char>();
        public List<Marker> markers = new List<Marker>();
        public List<AudioClip> sounds = new List<AudioClip>();
        public List<_Camera> cameras = new List<_Camera>();

        protected Speech speech;
        protected AudioClip currentSpeechClip;
        protected string currentLine = string.Empty;
        public Conversation conversation;
        protected int choiceID = -1;
        protected int tagIndex = -1;
        protected bool evaluatingTags = false;
        protected bool tagActionRunning = false;
        protected Char actionActor = null;
        protected float actionComplete = -999f;

        /*
        These static variables define the default behaviour for an action called from an Ink file via a tag.
        If you have a standard way of running these actions then change the defaults to that so you will
        require less optional tagging in your Ink files.
         */
        static AnimOptions defaultAnimOptions = new AnimOptions(0, 0, false, true);
        static FaceOptions defaultFaceOptions = new FaceOptions(true, false);
        static MoveOptions defaultMoveOptions = new MoveOptions(false, true, true);
        static MusicOptions defaultMusicOptions = new MusicOptions(MusicAction.Play, false, false, false, false, 0.0f);
        static SoundOptions defaultSoundOptions = new SoundOptions(false);
        static SpeechOptions defaultSpeechOptions = new SpeechOptions(false, true, false);
        static SwitchCameraOptions defaultCameraOptions = new SwitchCameraOptions(0.0f, MoveMethod.Linear, false, false);
        static string defaultSpeechPath = "Speech/";
        protected AnimOptions animOptions;

        protected struct AnimOptions
        {
            public int layer;
            public float fadeTime;
            public bool resetToIdle;
            public bool waitFinish;

            public AnimOptions(int _layer = 0, float _fadeTime = 0, bool _resetToIdle = false, bool _waitFinish = true)
            {
                layer = _layer;
                fadeTime = _fadeTime;
                resetToIdle = _resetToIdle;
                waitFinish = _waitFinish;
            }
        }

        protected struct FaceOptions
        {
            public bool isInstant;
            public bool willWait;

            public FaceOptions(bool _isInstant = true, bool _willWait = false)
            {
                isInstant = _isInstant;
                willWait = _willWait;
            }
        }

        protected struct MoveOptions
        {
            public bool run;
            public bool pathFind;
            public bool waitFinish;

            public MoveOptions(bool _run, bool _pathFind, bool _waitFinish)
            {
                run = _run;
                pathFind = _pathFind;
                waitFinish = _waitFinish;
            }
        }

        protected struct MusicOptions
        {
            public MusicAction method;
            public bool loop;
            public bool queue;
            public bool resume;
            public bool wait;
            public float transitionTime;

            public MusicOptions(MusicAction _method, bool _loop, bool _queue, bool _resume, bool _wait, float _transistionTime)
            {
                method = _method;
                loop = _loop;
                queue = _queue;
                resume = _resume;
                wait = _wait;
                transitionTime = _transistionTime;
            }
        }

        protected struct SoundOptions
        {
            public bool waitFinish;

            public SoundOptions(bool _waitFinish)
            {
                waitFinish = _waitFinish;
            }
        }

        protected struct SpeechOptions
        {
            public bool isBackground;
            public bool noAnimation;
            public bool noSkip;

            public SpeechOptions(bool _isBackground, bool _noAnimation, bool _noSkip)
            {
                isBackground = _isBackground;
                noAnimation = _noAnimation;
                noSkip = _noSkip;
            }

        }

        protected struct SwitchCameraOptions
        {
            public float time;
            public MoveMethod method;
            public bool smooth;
            public bool waitFinish;

            public SwitchCameraOptions(float _time, MoveMethod _method, bool _smooth, bool _waitFinish)
            {
                time = _time;
                method = _method;
                smooth = _smooth;
                waitFinish = _waitFinish;
            }
        }

        public ActionInkRunScript()
        {
            this.isDisplayed = true;
            category = ActionCategory.ThirdParty;
            title = "Ink Script";
            description = "Runs an Ink Script, from the start or designated knot";
        }

        override public float Run()
        {
            if (KickStarter.speechManager == null)
            {
                ACDebug.Log("No Speech Manager present");
                return 0f;
            }

            if (KickStarter.dialog && KickStarter.stateHandler)
            {
                if (!isRunning)
                {
                    EventManager.OnClickConversation += GetChoiceID;
                    EventManager.OnStartSpeech += SpeechStart;
                    EventManager.OnStopSpeech += SpeechStop;
                    isRunning = true;
                    tagIndex = -1;
                    choiceID = -1;
                    SetScript();
                    return defaultPauseTime;
                }
                else
                {
                    if (evaluatingTags)
                    {
                        float t = EvaluateTags();

                        if (t > actionComplete)
                        {
                            return t;
                        }
                        evaluatingTags = false;
                        return defaultPauseTime;
                    }

                    CheckSpeechAudioEnded();

                    if (speech != null)
                    {
                        if (speech.isAlive && !speech.isBackground) return defaultPauseTime;
                    }


                    if (ACInkIntegration.inkStory.canContinue && currentLine == string.Empty)
                    {

                        if (tagIndex == -1)
                        {
                            currentLine = ACInkIntegration.inkStory.Continue();
                            tagIndex = 0;
                            if (ACInkIntegration.inkStory.currentTags.Count > 0 && !evaluatingTags)
                            {
                                evaluatingTags = true;
                                return defaultPauseTime;
                            }
                        }
                    }

                    if (currentLine != string.Empty)
                    {
                        RunScript(currentLine);
                        tagIndex = -1;
                        currentLine = string.Empty;
                        return defaultPauseTime;
                    }

                    if (conversation != null)
                    {
                        if (!conversation.IsActive(false) && choiceID >= 0)
                        {
                            ACInkIntegration.inkStory.ChooseChoiceIndex(choiceID);
                            choiceID = -1;
                            tagIndex = -1;
                            return defaultPauseTime;
                        }

                        if (ACInkIntegration.inkStory.currentChoices.Count > 0 && !conversation.IsActive(false))
                        {
                            GetChoices();
                            if(conversation.options.Count == 1)
                            {
                                choiceID = 0;
                                return defaultPauseTime;
                            } else 
                            {
                                conversation.Interact();
                            }
                        }

                        if (conversation != null && conversation.IsActive(false))
                        {
                            return defaultPauseTime;
                        }
                    }
                    isRunning = false;
                    EventManager.OnClickConversation -= GetChoiceID;
                    EventManager.OnStartSpeech -= SpeechStart;
                    EventManager.OnStopSpeech -= SpeechStop;
                    return 0f;
                }
            }
            return 0f;
        }
        void GetChoiceID(Conversation conversation, int optionID)
        {
            choiceID = optionID;
        }

        void CheckSpeechAudioEnded()
        {
            if (speech == null) return;

            if (speech.speaker != null)
            {
                if (currentSpeechClip != null && !speech.speaker.speechAudioSource.isPlaying)
                {
                    KickStarter.dialog.EndSpeechByCharacter(speech.speaker);
                }
            }
        }

        void SpeechStart(AC.Char speakingCharacter, string speechText, int lineID)
        {
            if (currentSpeechClip != null && speakingCharacter != null)
            {
                speakingCharacter.speechAudioSource.clip = currentSpeechClip;
                speakingCharacter.speechAudioSource.loop = false;
                speakingCharacter.speechAudioSource.Play();
            }
        }

        void SpeechStop(AC.Char speakingCharacter)
        {
            if (speakingCharacter != null)
            {
                speakingCharacter.speechAudioSource.Stop();
            }

            currentSpeechClip = null;
        }

        override public void Skip()
        {
            Run();
        }


#if UNITY_EDITOR

        public int numberOfActors = 0;
        public int numberOfMarkers = 0;
        public int numberOfSounds = 0;
        public int numberOfCameras = 0;

        override public void ShowGUI()
        {
            newStory = EditorGUILayout.Toggle("New Story?", newStory);
            knot = EditorGUILayout.TextField("Knot/Stitch:", knot);
            conversation = (Conversation)EditorGUILayout.ObjectField(new GUIContent("Conversation:"), conversation, typeof(Conversation), true);
            numberOfActors = EditorGUILayout.DelayedIntField(new GUIContent("Number of speakers:"), numberOfActors);

            if (actors != null)
            {
                while (numberOfActors < actors.Count)
                {
                    actors.RemoveAt(actors.Count - 1);
                }
                while (numberOfActors > actors.Count)
                {
                    actors.Add(null);
                }
                for (int i = 0; i < actors.Count; i++)
                {
                    actors[i] = (Char)EditorGUILayout.ObjectField(new GUIContent(string.Format("Speaker {0}", i + 1)), actors[i], typeof(Char), true);
                }
            }
            numberOfMarkers = EditorGUILayout.DelayedIntField(new GUIContent("Number of markers:"), numberOfMarkers);

            if (markers != null)
            {
                while (numberOfMarkers < markers.Count)
                {
                    markers.RemoveAt(markers.Count - 1);
                }
                while (numberOfMarkers > markers.Count)
                {
                    markers.Add(null);
                }
                for (int i = 0; i < markers.Count; i++)
                {
                    markers[i] = (Marker)EditorGUILayout.ObjectField(new GUIContent(string.Format("Marker {0}", i + 1)), markers[i], typeof(Marker), true);
                }
            }
            numberOfSounds = EditorGUILayout.DelayedIntField(new GUIContent("Number of sounds:"), numberOfSounds);

            if (sounds != null)
            {
                while (numberOfSounds < sounds.Count)
                {
                    sounds.RemoveAt(sounds.Count - 1);
                }
                while (numberOfSounds > sounds.Count)
                {
                    sounds.Add(null);
                }
                for (int i = 0; i < sounds.Count; i++)
                {
                    sounds[i] = (AudioClip)EditorGUILayout.ObjectField(new GUIContent(string.Format("Sound {0}", 1 + i)), sounds[i], typeof(AudioClip), true);
                }
            }

            numberOfCameras = EditorGUILayout.DelayedIntField(new GUIContent("Number of cameras:"), numberOfCameras);

            if (cameras != null)
            {
                while (numberOfCameras < cameras.Count)
                {
                    cameras.RemoveAt(cameras.Count - 1);
                }
                while (numberOfCameras > cameras.Count)
                {
                    cameras.Add(null);
                }
                for (int i = 0; i < cameras.Count; i++)
                {
                    cameras[i] = (_Camera)EditorGUILayout.ObjectField(new GUIContent(string.Format("Camera {0}", 1 + i)), cameras[i], typeof(_Camera), true);
                }
            }
            AfterRunningOption();
        }

        public override string SetLabel()
        {
            // (Optional) Return a string used to describe the specific action's job.
            return string.Empty;
        }

#endif

        protected void SetScript()
        {
            if (newStory)
            {
                ACInkIntegration.inkStory.ResetState();
                // TODO: Evaluate global tags
            }

            if (knot.Trim() != "")
            {
                ACInkIntegration.inkStory.ChoosePathString(knot);
                //TODO: Evaluate knot tags
            }
        }
        protected void RunScript(string text)
        {
            var (currentSpeaker, dialogueLine) = ExtractSpeakerAndDialogue(text);

            if (dialogueLine != string.Empty)
            {
                int lineID = GetLineID(GetTagStartsWith("lineid"));

                if (lineID == -1)
                {
                    GetSpeechAudio(GetTagStartsWith("audio"));
                }
                SpeechOptions options = GetSpeechOptions();
                speech = KickStarter.dialog.StartDialog(currentSpeaker, dialogueLine, options.isBackground, lineID, options.noAnimation, options.noSkip);
            }
        }

        private Tuple<Char, string> ExtractSpeakerAndDialogue(string text)
        {
            Char currentSpeaker = null;
            char[] delimiter = { ':' };
            string[] components = text.Split(delimiter, 2);
            string dialogueLine = "";

            if (components.Length > 1)
            {
                string speakerName = components[0].Trim();
                currentSpeaker = GetActorFrom(speakerName);
                dialogueLine = components[1].Trim();
            }
            else
            {
                dialogueLine = text;
            }

            return Tuple.Create(currentSpeaker, dialogueLine);
        }

        protected int GetLineID(string tag)
        {
            string[] components = tag.Split('=');

            if (components.Length == 2)
            {
                return Convert.ToInt32(components[1].Trim());
            }
            return -1;
        }

        protected void GetSpeechAudio(string tag)
        {

            string[] components = tag.Split('=');

            if (components.Length == 2)
            {
                string path = defaultSpeechPath + components[1].Trim();
                currentSpeechClip = Resources.Load(path) as AudioClip;
            }
        }

        protected SpeechOptions GetSpeechOptions()
        {
            SpeechOptions options = defaultSpeechOptions;

            foreach (string item in ACInkIntegration.inkStory.currentTags)
            {
                string option = item.ToLower().Trim();

                if (option.EndsWith("bg")) options.isBackground = TagState(option);
                else if (option.EndsWith("anim")) options.noAnimation = !TagState(option);
                else if (option.EndsWith("skip")) options.noSkip = !TagState(option);
            }
            return options;
        }

        protected string GetTagStartsWith(string tag)
        {
            foreach (string item in ACInkIntegration.inkStory.currentTags)
            {
                if (item.ToLower().StartsWith(tag))
                {
                    return item;
                }
            }
            return string.Empty;
        }

        protected float EvaluateTags()
        {
            if (tagIndex >= ACInkIntegration.inkStory.currentTags.Count)
            {
                return actionComplete;
            }
            string tag = ACInkIntegration.inkStory.currentTags[tagIndex];
            char[] delimiter = { ' ' };
            string[] components = tag.Split(delimiter, 2);
            float time = actionComplete;

            if (components.Length == 2)
            {
                switch (components[0].ToLower())
                {
                    case "move":
                        time = MoveTo(components[1]);
                        break;

                    case "wait":
                        time = Wait(components[1]);
                        tagIndex += 1;
                        break;

                    case "animate":
                        time = PlayAnimation(components[1]);
                        break;

                    case "set":
                        time = actionComplete;
                        SetVariable(components[1]);
                        break;

                    case "face":
                        time = Face(components[1]);
                        break;

                    case "music":
                        time = actionComplete;
                        Music(components[1]);
                        break;

                    case "sound":
                        time = Sound(components[1]);
                        break;

                    case "camera":
                        time = SwitchCamera(components[1]);
                        break;

                    case "inventory":
                        InventoryAction(components[1]);
                        time = actionComplete;
                        break;

                    case "toscene":
                        ToScene(components[1]);
                        time = actionComplete;
                        break;
                }
            }
            if (time == actionComplete)
            {
                tagIndex += 1;
                time = defaultPauseTime;
            }
            return time;
        }

        protected Char GetActorFrom(string actorName)
        {
            foreach (Char speaker in actors)
            {
                if (speaker.name == actorName.Trim())
                {
                    return speaker;
                }
            }
            if (actorName == KickStarter.player.name)
            {
                return KickStarter.player;
            }
            return null;
        }

        protected void SetAnimOptions(string[] components)
        {
            AnimOptions options = defaultAnimOptions;

            if (components.Length > 2)
            {
                for (int i = 2; i < components.Length; i++)
                {
                    string option = components[i].Trim().ToLower();

                    if (option.StartsWith("layer"))
                    {
                        string[] lc = components[i].Split('=');
                        if (lc.Length == 2)
                        {
                            options.layer = Convert.ToInt32(lc[1].Trim());
                        }
                    }
                    else if (option.StartsWith("fadetime"))
                    {
                        string[] lc = components[i].Split('=');
                        if (lc.Length == 2)
                        {
                            options.fadeTime = (float)Convert.ToDouble(lc[1].Trim());
                        }
                    }
                    else if (option.EndsWith("idle")) options.resetToIdle = TagState(option);
                    else if (option.EndsWith("wait")) options.waitFinish = TagState(option);
                }
            }
            animOptions = options;
        }

        protected float PlayAnimation(string text)
        {
            if (!tagActionRunning)
            {
                string[] components = text.Split(',');

                if (components.Length < 2)
                {
                    tagActionRunning = false;
                    return actionComplete;
                }

                actionActor = GetActorFrom(components[0]);

                if (actionActor == null) return actionComplete;

                SetAnimOptions(components);

                tagActionRunning = true;
                actionActor.charState = CharState.Custom;
                actionActor.GetAnimator().CrossFade(components[1].Trim(), 0, 0);

                if (!animOptions.waitFinish)
                {
                    tagActionRunning = false;
                    return actionComplete;
                }
                return defaultPauseTime;
            }
            else
            {
                float totalLength = actionActor.GetAnimator().GetCurrentAnimatorStateInfo(animOptions.layer).length;
                float timeLeft = (1f - actionActor.GetAnimator().GetCurrentAnimatorStateInfo(animOptions.layer).normalizedTime) * totalLength;
                timeLeft -= 0.1f;

                if (timeLeft > 0f)
                {
                    return (timeLeft);
                }

                if (animOptions.resetToIdle)
                {
                    actionActor.charState = CharState.Idle;
                }
            }

            tagActionRunning = false;
            actionActor = null;
            return actionComplete;
        }

        protected float Face(string text)
        {
            if (!tagActionRunning)
            {
                string[] components = text.Split(',');

                if (components.Length < 2) return actionComplete;

                actionActor = GetActorFrom(components[0]);

                if (actionActor == null) return actionComplete;

                FaceOptions faceOptions = defaultFaceOptions;

                if (components.Length > 2)
                {
                    faceOptions = GetFaceOptions(components);
                }

                if (!faceOptions.isInstant && actionActor.IsMovingAlongPath())
                {
                    actionActor.EndPath();
                }

                actionActor.SetLookDirection(GetLookVector(components[1].Trim().ToLower()), faceOptions.isInstant);

                if (!faceOptions.isInstant)
                {
                    if (faceOptions.willWait)
                    {
                        tagActionRunning = true;
                        return defaultPauseTime;
                    }
                }
                return actionComplete;
            }
            else
            {
                if (actionActor.IsTurning())
                {
                    return defaultPauseTime;
                }
                else
                {
                    tagActionRunning = false;
                    actionActor = null;
                    return actionComplete;
                }
            }
        }

        protected Vector3 GetLookVector(string direction)
        {
            Vector3 lookVector = Vector3.zero;
            Vector3 upVector = Camera.main.transform.up;
            Vector3 rightVector = Camera.main.transform.right - new Vector3(0f, 0.01f); // Angle slightly so that left->right rotations face camera

            if (SceneSettings.IsTopDown())
            {
                upVector = -Camera.main.transform.forward;
            }

            if (direction == "down")
            {
                lookVector = -upVector;
            }
            else if (direction == "left")
            {
                lookVector = -rightVector;
            }
            else if (direction == "right")
            {
                lookVector = rightVector;
            }
            else if (direction == "up")
            {
                lookVector = upVector;
            }
            else if (direction == "downleft")
            {
                lookVector = (-upVector - rightVector).normalized;
            }
            else if (direction == "downright")
            {
                lookVector = (-upVector + rightVector).normalized;
            }
            else if (direction == "upleft")
            {
                lookVector = (upVector - rightVector).normalized;
            }
            else if (direction == "upright")
            {
                lookVector = (upVector + rightVector).normalized;
            }
            else
            {
                //TODO: face object in here
            }

            lookVector = new Vector3(lookVector.x, 0f, lookVector.y);
            return lookVector;
        }

        protected FaceOptions GetFaceOptions(string[] components)
        {
            FaceOptions options = defaultFaceOptions;

            for (int i = 2; i < components.Length; i++)
            {
                string option = components[i].Trim().ToLower();

                if (option.EndsWith("instant")) options.isInstant = TagState(option);
                else if (option.EndsWith("wait")) options.willWait = TagState(option);
            }
            return options;
        }
        protected float MoveTo(string text)
        {
            if (!tagActionRunning)
            {
                string[] components = text.Split(',');

                if (components.Length < 2)
                {
                    tagActionRunning = false;
                    return actionComplete;
                }
                actionActor = GetActorFrom(components[0]);

                if (actionActor == null) return actionComplete;

                Marker marker = null;

                foreach (Marker mark in markers)
                {
                    if (mark.name == components[1].Trim())
                    {
                        marker = mark;
                        break;
                    }
                }

                if (marker == null) return actionComplete;

                MoveOptions options = defaultMoveOptions;

                if (components.Length > 2)
                {
                    options = GetMoveOptions(components);
                }

                actionActor.MoveToPoint(marker.transform.position, options.run, options.pathFind);

                if (!options.waitFinish)
                {
                    return actionComplete;
                }
                tagActionRunning = true;
                return defaultPauseTime;
            }

            if (actionActor == null)
            {
                tagActionRunning = false;
                return actionComplete;
            }
            else if (actionActor.IsMovingAlongPath())
            {
                return defaultPauseTime;
            }
            else
            {
                tagActionRunning = false;
                actionActor = null;
                return actionComplete;
            }
        }

        protected MoveOptions GetMoveOptions(string[] components)
        {
            MoveOptions options = defaultMoveOptions;

            for (int i = 2; i < components.Length; i++)
            {
                string option = components[i].ToLower().Trim();

                if (option.EndsWith("run")) options.run = TagState(option);
                else if (option.EndsWith("path")) options.pathFind = TagState(option);
                else if (option.EndsWith("wait")) options.waitFinish = TagState(option);
            }
            return options;
        }

        protected void Music(string text)
        {
            Music music = KickStarter.stateHandler.GetMusicEngine();

            if (music == null) return;

            string[] components = text.Split(',');
            string trackName = components[0].Trim().ToLower();
            MusicOptions options = defaultMusicOptions;

            if (components.Length > 2)
            {
                options = GetMusicOptions(components);
            }
            else
            {
                if (trackName == "stop") options.method = MusicAction.Stop;
                else if (trackName == "resume") options.method = MusicAction.ResumeLastStopped;
            }
            if (options.method == MusicAction.Play)
            {
                foreach (MusicStorage track in KickStarter.settingsManager.musicStorages)
                {
                    if (track.audioClip.name.ToLower() == trackName)
                    {
                        music.Play(track.ID, options.loop, options.queue, options.transitionTime, options.resume);
                    }
                }
            }
            else if (options.method == MusicAction.Crossfade)
            {
                foreach (MusicStorage track in KickStarter.settingsManager.musicStorages)
                {
                    if (track.audioClip.name.ToLower() == trackName)
                    {
                        music.Crossfade(track.ID, options.loop, options.queue, options.transitionTime, options.resume);
                    }
                }
            }
            else if (options.method == MusicAction.Stop)
            {
                music.StopAll(options.transitionTime);
            }
            else if (options.method == MusicAction.ResumeLastStopped)
            {
                music.ResumeLastQueue(options.transitionTime, options.resume);
            }
        }

        protected MusicOptions GetMusicOptions(string[] components)
        {
            MusicOptions options = defaultMusicOptions;

            for (int i = 2; i < components.Length; i++)
            {
                string option = components[i].Trim().ToLower();

                if (option.EndsWith("loop")) options.loop = TagState(option);
                else if (option.EndsWith("queue")) options.queue = TagState(option);
                else if (option.EndsWith("resume")) options.resume = TagState(option);
                else if (option.EndsWith("wait")) options.wait = TagState(option);
                else if (option.StartsWith("fade"))
                {
                    string[] fadeComponents = option.Split('=');

                    if (fadeComponents.Length == 2)
                    {
                        options.transitionTime = (float)Convert.ToDouble(fadeComponents[1]);
                    }
                }
                else if (option == "play") options.method = MusicAction.Play;
                else if (option == "crossfade") options.method = MusicAction.Crossfade;
            }
            return options;
        }

        protected float Sound(string text)
        {
            string[] components = text.Split(',');
            SoundOptions options = defaultSoundOptions;

            if (components.Length > 1)
            {
                string option = components[1].ToLower().Trim();
                if (option.EndsWith("wait")) options.waitFinish = TagState(option);
            }

            if (!tagActionRunning)
            {
                foreach (AudioClip audioClip in sounds)
                {
                    if (audioClip.name == components[0].Trim())
                    {
                        Vector3 originPos = Camera.main.transform.position;
                        float volume = Options.GetSFXVolume();
                        AudioSource.PlayClipAtPoint(audioClip, originPos, volume);

                        if (options.waitFinish)
                        {
                            tagActionRunning = true;
                            return audioClip.length;
                        }
                        break;
                    }
                }
            }
            tagActionRunning = false;
            return actionComplete;
        }

        protected void SetVariable(string text)
        {
            string[] components = text.Split('=');

            if (components.Length == 2)
            {
                string varName = components[0].Trim();
                string value = components[1].Trim().ToLower();

                if (KickStarter.runtimeVariables)
                {
                    List<GVar> vars = GlobalVariables.GetAllVars();

                    foreach (GVar v in vars)
                    {
                        if (v.label == varName)
                        {
                            switch (v.type)
                            {
                                case VariableType.Boolean:
                                    v.val = SetBoolean(value);
                                    break;

                                case VariableType.Integer:
                                    v.val = SetInt(value);
                                    break;

                                case VariableType.Float:
                                    v.floatVal = SetFloat(value);
                                    break;

                                case VariableType.String:
                                    v.textVal = value;
                                    break;
                            }
                            return;
                        }
                    }
                }
            }
        }

        protected float Wait(string text)
        {
            return (float)Convert.ToDouble(text.Trim());
        }

        protected SwitchCameraOptions GetSwitchCameraOptions(string[] components)
        {
            SwitchCameraOptions options = defaultCameraOptions;

            for(int i = 1; i < components.Length; i++)
            {
                string option = components[i].ToLower().Trim();
                if(option.StartsWith("time"))
                {
                    string[] t = option.Split('=');
                    if(t.Length == 2){
                        options.time = (float) Convert.ToDouble(t[1].Trim());
                    }
                    continue;
                }
                if(option.Equals("linear")) options.method = MoveMethod.Linear;
                if(option.Equals("smooth")) options.method = MoveMethod.Smooth;
                if(option.Equals("curved")) options.method = MoveMethod.Curved;
                if(option.Equals("easein")) options.method = MoveMethod.EaseIn;
                if(option.Equals("easeout")) options.method = MoveMethod.EaseOut;
                if(option.EndsWith("retainSpeed")) options.smooth = TagState(option);
                if(option.EndsWith("waitfinish")) options.waitFinish = TagState(option);        
            }
            return options;
        }

        protected float SwitchCamera(string text)
        {
            string cameraName = text.ToLower().Trim();
            SwitchCameraOptions options = defaultCameraOptions;

            string[] components = text.Split(',');
            if(components.Length > 1){
                options = GetSwitchCameraOptions(components);
            }

            foreach (_Camera camera in cameras)
            {
                if (camera.name.ToLower().Trim() == cameraName)
                {
                    KickStarter.mainCamera.SetGameCamera(camera, options.time, options.method, null, options.smooth);
                }
            }
            if(options.waitFinish && options.time > 0.0f) return options.time;
            
            return actionComplete;
        }

        protected void InventoryAction(string text){

            string[] components = text.Split(',');
            string verb = components[0].ToLower().Trim();

            switch(verb){
                case "add":
                    for(int i = 0; i < KickStarter.inventoryManager.items.Count ; i++)
                    {
                        if(KickStarter.inventoryManager.items[i].GetLabel(0).ToLower().Trim() == components[1].ToLower().Trim())
                        {
                            KickStarter.runtimeInventory.Add(i);
                            break;          
                        } 
                    }
                    break;
                case "remove":
                    for(int i = 0; i < KickStarter.inventoryManager.items.Count ; i++)
                    {
                        if(KickStarter.runtimeInventory.localItems[i].GetLabel(0).ToLower().Trim() == components[1].ToLower().Trim())
                        {
                            KickStarter.runtimeInventory.Remove(i);
                            break;          
                        } 
                    }
                    break;
            }
        }

        protected void ToScene(string text)
        {
            KickStarter.sceneChanger.PrepareSceneForExit();
            KickStarter.sceneChanger.ChangeScene(new SceneInfo(text.Trim()), true );
        }

        protected int SetBoolean(string str)
        {
            if (str == "true")
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        protected int SetInt(string str)
        {
            int num = 0;
            try
            {
                num = Convert.ToInt32(str);
            }
            catch (FormatException)
            {
                Console.WriteLine("Number Format Exception from Ink file, value used: {0}", str);
            }
            return num;
        }

        protected float SetFloat(string str)
        {
            float num = 0f;

            try
            {
                num = (float)Convert.ToDouble(str);
            }
            catch (FormatException)
            {
                Console.WriteLine("Number Format Exception from Ink file, value used: {0}", str);
            }
            return num;
        }

        protected bool TagState(string str)
        {
            return !str.StartsWith("no");
        }

        protected void GetChoices()
        {
            conversation.options.Clear();

            for (int i = 0; i < ACInkIntegration.inkStory.currentChoices.Count; ++i)
            {
                Choice choice = ACInkIntegration.inkStory.currentChoices[i];
                var (_, dialogueText) = ExtractSpeakerAndDialogue(choice.text);
                ButtonDialog button = new ButtonDialog(choice.index, dialogueText, true);
                conversation.options.Add(button);
            }
        }
    }
}
