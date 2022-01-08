using UnityEngine;
using System;
using System.IO;
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
        public override ActionCategory Category{get { return ActionCategory.ThirdParty;}}
        public override string Title {get {return "Run Ink Script";}}
        public override string Description { get { return "Runs an Ink Script, from the start or designated knot";}}
       
        public bool newStory;
        public string knot;    
        public List<Char> actors = new List<Char>();
        public List<Marker> markers = new List<Marker>();
        public List<AudioClip> sounds = new List<AudioClip>();
        public List<_Camera> cameras = new List<_Camera>();
        public List<GameObject> objects = new List<GameObject>();

        public ActionList actionList;
        public GameObject gameObject;

        protected int maxChoices = 3;
		
		protected Dictionary<string, string[]> speakerColours = new Dictionary<string, string[]>();

        /*
        These static variables define the default behaviour for an action called from an Ink file via a tag.
        If you have a standard way of running these actions then change the defaults to that so you will
        require less optional tagging in your Ink files.
         */
        static AnimOptions defaultAnimOptions = new AnimOptions(0, 0, false, true);
        static FaceOptions defaultFaceOptions = new FaceOptions(true, false);
        static MoveOptions defaultMoveOptions = new MoveOptions(false, true, true, false);
        static MusicOptions defaultMusicOptions = new MusicOptions(MusicAction.Play, false, false, false, false, 0.0f);
        static SoundOptions defaultSoundOptions = new SoundOptions(false);
        static SpeechOptions defaultSpeechOptions = new SpeechOptions(false, false, false);
        static SwitchCameraOptions defaultCameraOptions = new SwitchCameraOptions(0.0f, MoveMethod.Smooth, false, true);
        static FadeOptions defaultFadeOptions = new FadeOptions(false, true, 1.0f);

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

        protected struct FadeOptions
        {
            public bool isInstant;
            public bool willWait;
            public float time;

            public FadeOptions(bool _isInstant = false, bool _willWait = true, float _time = 1.0f)
            {
                isInstant = _isInstant;
                willWait = _willWait;
                time = _time;
            }
        }

        protected struct MoveOptions
        {
            public bool run;
            public bool pathFind;
            public bool waitFinish;
            public bool face;

            public MoveOptions(bool _run, bool _pathFind, bool _waitFinish, bool _face)
            {
                run = _run;
                pathFind = _pathFind;
                waitFinish = _waitFinish;
                face = _face;
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
                    isRunning = true;
                    ACInkIntegration.choiceID = -1;
                    SetScript();
                    gameObject = new GameObject("Action Holder");
                    actionList = CreateActionList();
                    BuildActionList();
                    actionList.Interact(0, false);

                    return defaultPauseTime;
                }
                else
                {           
                    if(ACInkIntegration.choiceID >= 0)
                    {
                        ACInkIntegration.inkStory.ChooseChoiceIndex(ACInkIntegration.choiceID);
                        BuildActionList();
                        ACInkIntegration.choiceID = -1;
                        actionList.Interact(0, false);

                    }

                    if (KickStarter.actionListManager.IsListRunning (actionList))
					{
						return defaultPauseTime;
					}
					    else
					{
						isRunning = false;
					}
                    
                    isRunning = false;
                    Destroy(gameObject);

                    return 0f;
                }
            }
            return 0f;
        }
       
        void BuildActionList()
        {
           actionList.actions = new List<Action>();

            while(ACInkIntegration.inkStory.canContinue)
            {
                String currentLine = ACInkIntegration.inkStory.Continue();

                 foreach(string tag in ACInkIntegration.inkStory.currentTags)
                 {
                     Action action = EvaluateTag(tag);

                     if(action != null)
                     {
                         actionList.actions.Add(action);
                     }
                 }
  
                ActionSpeech actionSpeech = RunScript(currentLine);
                if (actionSpeech != null) actionList.actions.Add(actionSpeech);
                
            }

            if (ACInkIntegration.inkStory.currentChoices.Count > 0)
            {
                actionList.actions.Add(ActionInkChoice.CreateNew(GetChoices(maxChoices)));              
            }
        }

        protected List<string> GetChoices(int maxChoices){

            List<string> choices = new List<string>(); 
            string state = ACInkIntegration.inkStory.state.ToJson();

            for (int i = 0; i < ACInkIntegration.inkStory.currentChoices.Count; ++i)
            {
                Choice choice = ACInkIntegration.inkStory.currentChoices[i];
                ACInkIntegration.inkStory.ChooseChoiceIndex(i);
                ACInkIntegration.inkStory.Continue();
                var (_, dialogueText) = ExtractSpeakerAndDialogue(choice.text, true);
                choices.Add(dialogueText);
                ACInkIntegration.inkStory.state.LoadJson(state);
                if(i == maxChoices-1){
                    return choices;
                }
            }
            return choices;

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
        public int numberOfObjects = 0;

        override public void ShowGUI()
        {
            newStory = EditorGUILayout.Toggle("New Story?", newStory);
            knot = EditorGUILayout.TextField("Knot/Stitch:", knot);
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

            numberOfObjects = EditorGUILayout.DelayedIntField(new GUIContent("Number of objects:"), numberOfObjects);

            if (objects != null)
            {
                while (numberOfObjects < objects.Count)
                {
                    objects.RemoveAt(objects.Count - 1);
                }
                while (numberOfObjects > objects.Count)
                {
                    objects.Add(null);
                }
                for (int i = 0; i < objects.Count; i++)
                {
                    objects[i] = (GameObject)EditorGUILayout.ObjectField(new GUIContent(string.Format("Object {0}", 1 + i)), objects[i], typeof(GameObject), true);
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
        protected ActionSpeech RunScript(string text)
        {
            var (currentSpeaker, dialogueLine) = ExtractSpeakerAndDialogue(text);

            if (dialogueLine.Length > 0)
            {
                int lineID = GetLineID(GetTagStartsWith("lineid"));

                SpeechOptions options = GetSpeechOptions();
                
                return ActionSpeech.CreateNew(currentSpeaker,dialogueLine,lineID,!options.isBackground);
            }
            return null;
        }

        private Tuple<Char, string> ExtractSpeakerAndDialogue(string text, bool isConversation = false)
        {
            Char currentSpeaker = null;
            string localisedText = text;
            char[] delimiter = { ':' };
            string[] components = localisedText.Split(delimiter, 2);
            string dialogueLine = "";

            if (components.Length > 1)
            {
                string speakerName = components[0].Trim();
                currentSpeaker = GetActorFrom(speakerName);
                dialogueLine = components[1].Trim();
            }
            else
            {
                dialogueLine = localisedText;
            }

            return Tuple.Create(currentSpeaker, dialogueLine.Trim());
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

        protected Action EvaluateTag(string tag)
        {
            char[] delimiter = { ' ' };
            string[] components = tag.Split(delimiter, 2);
           
            if (components.Length == 2)
            {
                switch (components[0].ToLower())
                {
                    case "move":
                        return MoveTo(components[1]);
                    case "wait":
                        return Wait(components[1]);
                    case "animate":
                        return Animate(components[1]);
                    case "set":
                        return SetVariable(components[1]);
                    case "face":
                       return Face(components[1]);
                    case "music":
                        return Music(components[1]);
                    case "sound":
                        return Sound(components[1]);
                    case "camera":
                        return SwitchCamera(components[1]);
                    case "inventory":
                        return InventoryAction(components[1]);
                    case "toscene":
                        return ToScene(components[1]);
                    case "visible":
                        return Visible(components[1]);
                    case "teleport":
                        return Teleport(components[1]);
                    case "fade":
                        return Fade(components[1]);
                    case "setstandard":
                        return SetStandard(components[1]);
                    case "resettoidle":
                        return ResetToIdle(components[1]);
                }
            }
            return null;
        }

        protected Char GetActorFrom(string actorName)
        {
            if (actorName == KickStarter.player.name)
            {
                return KickStarter.player;
            }

            foreach (Char speaker in actors)
            {
                if (speaker.name == actorName.Trim())
                {
                    return speaker;
                }
            }
            
            return null;
        }

        protected AnimOptions SetAnimOptions(string[] components)
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
            return options;
        }

        protected Action Animate(string text)
        {
             string[] components = text.Split(',');

            if (components.Length < 2) return null;

            Char actionActor = GetActorFrom(components[0]);
            string clip = components[1].Trim();
            AnimOptions animOptions = SetAnimOptions(components);

            if (actionActor == null){ 

                GameObject animObject = null;

                foreach(GameObject obj in objects){
                    if(obj.name.ToLower().Trim() == components[0].ToLower().Trim())
                    {
                        animObject = obj;
                        break;
                    }
                }

                if(animObject == null) return null;
             
                return ActionAnim.CreateNew_SpritesUnity_PlayCustom(animObject.GetComponent<Animator>(),clip,0,0,animOptions.waitFinish);                 
            }
            else
            {
                return ActionCharAnim.CreateNew_SpritesUnity_PlayCustom(actionActor,clip,false,0,0,animOptions.waitFinish, animOptions.resetToIdle );
            }         
        }

        protected ActionCharFaceDirection Face(string text)
        {     
            string[] components = text.Split(',');

            if (components.Length < 2) return null;

            Char actionActor = GetActorFrom(components[0]);

            if (actionActor == null) return null;

            CharDirection dir = GetLookVector(components[1].Trim().ToLower());
            FaceOptions faceOptions = defaultFaceOptions;

            if (components.Length > 2) faceOptions = GetFaceOptions(components);

            return ActionCharFaceDirection.CreateNew(actionActor, dir, ActionCharFaceDirection.RelativeTo.Camera, faceOptions.isInstant, faceOptions.willWait);         
        }

        protected CharDirection GetLookVector(string direction)
        {
            switch(direction)
            {
                case "down":
                    return CharDirection.Down;
                case "left":
                    return CharDirection.Left;
                case "right":
                    return CharDirection.Right;
                case "up":
                    return CharDirection.Up;
                case "upleft":
                    return CharDirection.UpLeft;
                case "upright":
                    return CharDirection.UpRight;
                case "downright":
                    return CharDirection.DownRight;
                case "downleft":
                    return CharDirection.DownLeft;
            }

            return CharDirection.Up;         
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

        protected ActionCharPathFind MoveTo(string text)
        {
            string[] components = text.Split(',');

            if (components.Length < 2)
            {
  
                return null;
            }
            Char actionActor = GetActorFrom(components[0]);

            if (actionActor == null) return null;

            Marker marker = null;

            foreach (Marker mark in markers)
            {
                if (mark.name == components[1].Trim())
                {
                    marker = mark;
                    break;
                }
            }

            if (marker == null) return null;

            MoveOptions options = defaultMoveOptions;

            if (components.Length > 2)
            {
                options = GetMoveOptions(components);
            }
            
            PathSpeed speed = PathSpeed.Walk;
            if(options.run) speed = PathSpeed.Run;

            return ActionCharPathFind.CreateNew(actionActor, marker, speed, options.pathFind, options.waitFinish, options.face);
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
                else if (option.EndsWith("face")) options.face = TagState(option);
            }
            return options;
        }

        protected ActionMusic Music(string text)
        {
            Music music = KickStarter.stateHandler.GetMusicEngine();

            if (music == null) return null;

            string[] components = text.Split(',');
            string trackName = components[0].Trim().ToLower();
            MusicOptions options = defaultMusicOptions;

            if (components.Length > 1) options = GetMusicOptions(components);
            
            if (trackName == "stop") return ActionMusic.CreateNew_Stop(options.transitionTime, options.wait);
            else if (trackName == "resume") return ActionMusic.CreateNew_ResumeLastTrack(options.transitionTime);

            int index = 0;

            for(int i=0; i<KickStarter.settingsManager.musicStorages.Count; i++){
               if(KickStarter.settingsManager.musicStorages[i].audioClip.name.ToLower() == trackName){
                   index = i;
                   break;
               }
            }

            if (options.method == MusicAction.Play) return ActionMusic.CreateNew_Play(index,options.loop,options.queue,options.transitionTime,options.transitionTime > 0f,options.wait);    
        
            return null;
        }

        protected MusicOptions GetMusicOptions(string[] components)
        {
            MusicOptions options = defaultMusicOptions;

            for (int i = 1; i < components.Length; i++)
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
            }
            return options;
        }

        protected ActionSoundShot Sound(string text)
        {
            string[] components = text.Split(',');
            SoundOptions options = defaultSoundOptions;

            if (components.Length > 1)
            {
                string option = components[1].ToLower().Trim();
                if (option.EndsWith("wait")) options.waitFinish = TagState(option);
            }
         
            foreach (AudioClip audioClip in sounds)
            {
                if (audioClip.name == components[0].Trim())
                {
                    return ActionSoundShot.CreateNew(audioClip,null,options.waitFinish);
                }
            }
            
            return null;          
        }

        protected ActionVarSet SetVariable(string text)
        {
            string[] components = text.Split('=');

            if (components.Length == 2)
            {
                string varName = components[0].Trim();
                string value = components[1].Trim().ToLower();

                if (KickStarter.runtimeVariables)
                {
                    List<GVar> vars = GlobalVariables.GetAllVars();

                    for (int i = 0 ; i < vars.Count ; i++)
                    {
                        GVar v = vars[i];

                        if (varName == v.label)
                        {
                            switch (v.type)
                            {
                                case VariableType.Boolean:
                                    return ActionVarSet.CreateNew_Global(i, SetBoolean(value));
                                case VariableType.Integer:
                                    return ActionVarSet.CreateNew_Global(i, SetInt(value));
                                case VariableType.Float:
                                    return ActionVarSet.CreateNew_Global(i, SetFloat(value));
                                case VariableType.String:
                                    return ActionVarSet.CreateNew_Global(i, value);
                            }
                        }
                    }
                }
            }
            return null;
        }

        protected ActionTeleport Teleport(string text){

            string[] components = text.Split(',');

            Marker mark = null;
       
            foreach(Marker m in markers){
                if(components[1].Trim().ToLower() == m.name.Trim().ToLower()){
                    mark = m;
                    break;
                }
            }

            if (mark == null) return null;
            
            Char c = GetActorFrom(components[0]);

            if(c != null){
                return ActionTeleport.CreateNew(c.gameObject, mark);
            }
            
            foreach(GameObject g in objects){
                if(components[0].Trim().ToLower() == g.name.Trim().ToLower()){
                    return ActionTeleport.CreateNew(g, mark);
                }
            }

            return null;
        }

        protected ActionPause Wait(string text)
        {
            return ActionPause.CreateNew(SetFloat(text));
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

        protected ActionCamera SwitchCamera(string text)
        {
            SwitchCameraOptions options = defaultCameraOptions;
            string[] components = text.Split(',');
            string cameraName = components[0];

            if(components.Length > 1) options = GetSwitchCameraOptions(components);
            
            foreach (_Camera camera in cameras)
            {
                if (camera.name == cameraName)
                {
                   return ActionCamera.CreateNew(camera, options.time, options.waitFinish, options.method); 
                }
            }

            return null;
        }

        protected ActionInventorySet InventoryAction(string text){

            string[] components = text.Split(',');
            string verb = components[0].ToLower().Trim();

            switch(verb){
                case "add":
                    for(int i = 0; i < KickStarter.inventoryManager.items.Count ; i++)
                    {
                        if(KickStarter.inventoryManager.items[i].GetLabel(0).ToLower().Trim() == components[1].ToLower().Trim())
                        {             
                            return ActionInventorySet.CreateNew_Add(i);       
                        } 
                    }
                    break;
                case "remove":
                    for(int i = 0; i < KickStarter.inventoryManager.items.Count ; i++)
                    {
                        if(KickStarter.runtimeInventory.localItems[i].GetLabel(0).ToLower().Trim() == components[1].ToLower().Trim())
                        {
                            return ActionInventorySet.CreateNew_Remove(i);         
                        } 
                    }
                    break;
            }
            return null;
        }

        protected ActionScene ToScene(string text)
        {
            return ActionScene.CreateNew_Switch(text.Trim(), false, false);           
        }

        protected ActionFade Fade(string text){
            
            string[] components = text.Split(',');
            FadeType fadeType = components[0].ToLower().Trim() == "out" ? FadeType.fadeOut : FadeType.fadeIn;
            FadeOptions options = defaultFadeOptions;

            if (components.Length > 1){
                for(int i = 1 ; i<components.Length ; i++){
                    string param = components[i].ToLower().Trim();
                    if(param.EndsWith("wait")){
                        options.willWait = TagState(param);
                        continue;
                    }
                    if(param.EndsWith("instant")){
                        options.willWait = TagState(param);
                        continue;
                    }
                    float.TryParse(param, out options.time);

                }
            }

            return ActionFade.CreateNew(fadeType, options.time, options.willWait);
        }

        protected ActionVisible Visible(string text){

            String[] components = text.Split(',');
            VisState state = VisState.Visible;
            if(components[1].Trim().ToLower() == "false") state = VisState.Invisible;
            
            foreach(GameObject obj in objects){
                if(obj.name.ToLower() == components[0].Trim().ToLower()){
                    
                    return ActionVisible.CreateNew(obj, state, true); 
                }
            }

            Char actor = GetActorFrom(components[0]);
            if (actor != null){
                if(actor.name.ToLower() == components[0].Trim().ToLower()){
                    return ActionVisible.CreateNew(actor.gameObject, state, true);            
                }
            }
            return null;
        }

        protected ActionCharAnim SetStandard(string str)
        {
            string[] components = str.Split(',');
  
            if(components.Length < 3) return null;
            
            Char actor = GetActorFrom(components[0]);

            if(actor == null) return null;

            string movement = components[1].Trim().ToLower();
            string animation = components[2].Trim();
            AnimStandard standard = AnimStandard.Idle;

            switch(movement){
                case "idle":
                    standard = AnimStandard.Idle;
                    break;
                case "walk":
                    standard = AnimStandard.Walk;
                    break;
                case "talk":
                    standard = AnimStandard.Talk;
                    break;
                case "run":
                    standard = AnimStandard.Run;
                    break;
            }

            return ActionCharAnim.CreateNew_SpritesUnity_SetStandard(actor,standard,animation);
        }

        protected ActionCharAnim ResetToIdle(string str)
        {   
            Char actor = GetActorFrom(str.Trim());
            if (actor != null){
                return ActionCharAnim.CreateNew_SpritesUnity_ResetToIdle(actor);
            }  
            return null;        
        }
		
        protected bool SetBoolean(string str)
        {
            if (str == "true")
            {
                return true;
            }
            else
            {
                return false;
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

        private ActionList CreateActionList ()
		{
			ActionList currentActionList = gameObject.GetComponent <ActionList>();
			if (currentActionList != null)
			{
				return currentActionList;
			}
			return gameObject.AddComponent <ActionList>();
		}      
    }  
	}