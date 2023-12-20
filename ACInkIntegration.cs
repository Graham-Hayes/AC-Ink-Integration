using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Ink.Runtime;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

/* 
    Helps integrate Ink with AC's save system and exposes GlobalVariables to Ink with getValue
    It's assumed that you have one monolithic story that includes all other files.

    This script is attached to the PersistantEngine and your JSON file is attached to main story.
    
    To enable saving create a string global variable, call it whatever you want, take note of its ID number and set varID to that
    in the inspector.
 */

public class ACInkIntegration : MonoBehaviour
{
    [HideInInspector] public static Story inkStory;
    public TextAsset mainStory;
    public int varID = -1;
    public Canvas inkChoiceMenu;
    [HideInInspector] public static int choiceID = -1;

    private void OnEnable()
    {
        inkStory = new Story(mainStory.text);
        BindFunctions();
        EventManager.OnBeforeSaving += BeforeSave;
        EventManager.OnFinishLoading += AfterLoad;
        //EventManager.OnClickConversation += SetChoiceID;
        Debug.Log(KickStarter.speechManager.lines.Count);
    }

    private void OnDisable()
    {
        EventManager.OnBeforeSaving -= BeforeSave;
        EventManager.OnFinishLoading -= AfterLoad;
        //EventManager.OnClickConversation -= SetChoiceID;
    }

    private void BeforeSave(int saveID)
    {
        GlobalVariables.SetStringValue(varID, inkStory.state.ToJson());
    }

    private void AfterLoad(int saveID)
    {
        inkStory.state.LoadJson(GlobalVariables.GetStringValue(varID));
    }

    private void BindFunctions()
    {
        inkStory.BindExternalFunction("getValue", (int a) =>
        {
            GVar var = GlobalVariables.GetVariable(a);

            if (var == null) return "no var";

            switch (var.type)
            {
                case VariableType.Boolean:
                    return var.BooleanValue;
                case VariableType.Integer:
                    return var.IntegerValue;
                case VariableType.String:
                    return var.TextValue;
                case VariableType.Float:
                    return var.FloatValue;
            }
            return "no var";
        });

        inkStory.BindExternalFunction("getLocalValue", (int a) =>
        {
            GVar var = LocalVariables.GetVariable(a);

            if (var == null) return "no var";

            switch (var.type)
            {
                case VariableType.Boolean:
                    return var.BooleanValue;
                case VariableType.Integer:
                    return var.IntegerValue;
                case VariableType.String:
                    return var.TextValue;
                case VariableType.Float:
                    return var.FloatValue;
            }
            return "no var";
        });

        inkStory.BindExternalFunction("inventoryContains", (string item) =>
        {
            foreach(InvItem invItem in KickStarter.runtimeInventory.localItems){
                if(invItem.GetLabel(0).Trim().ToLower() == item.Trim().ToLower())
                {
                    return true;
                }     
            }
            return false;
        });
    }

     void SetChoiceID(Conversation conversation, int optionID)
        {
            Debug.Log("Choose " + optionID);
            choiceID = optionID;

        }

    /*
        GatherSpeech goes through your Ink files that are in the Assets/Story/ directory and adds a lineID tag
        It then creates a SpeechLine in the SpeechManager.
        Probably a good idea to backup your Ink files before using.
     */
 /*     [MenuItem("Adventure Creator/Third Party/Ink/Gather Speech (DANGER: EXPERIMENTAL)")]
    private static void GatherSpeech()
    {

        string[] inkFiles = Directory.GetFiles(Application.dataPath + "/AdvJam21/Story/", "*.csv");
        
        foreach (string file in inkFiles)
        {
            string line;

            StreamReader r = new StreamReader(file);
           // string finalFile = string.Empty;
           // bool inCommentBlock = false;

            SpeechManager speechManager = KickStarter.speechManager;
            int index = 0;
            using (r)
            {
                do
                {
                    line = r.ReadLine();
                    if (line != null)
                    {

                        if(index == 0){
                            index++;
                            continue;
                        }
                    
                        string[] components = line.Split('\t');
                        int id = int.Parse(components[0]);
                        string owner = components[1];
                        bool isPlayer = false;
                        if(owner == "Peregrine"){
                            isPlayer = true;
                        }
                        string text = components[2];
                        string audioFile = components[3];

                        string path = "Speech/" + components[3].Trim();
                        AudioClip currentSpeechClip = Resources.Load(path) as AudioClip;
                        Debug.Log(path + " " + currentSpeechClip);
                        SpeechLine newLine = new SpeechLine(id, "", owner, text, 0, AC_TextType.Speech, isPlayer);
                        newLine.customAudioClip = currentSpeechClip;
                        speechManager.lines.Add(newLine);
                        
                        
        
                    }
                }
                while (line != null);
                r.Close();
            }

        }
    }*/

    
}
