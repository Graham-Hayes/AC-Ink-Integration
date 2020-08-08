using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    private void OnEnable()
    {
        inkStory = new Story(mainStory.text);
        BindFunctions();
        EventManager.OnBeforeSaving += BeforeSave;
        EventManager.OnFinishLoading += AfterLoad;
    }

    private void OnDisable()
    {
        EventManager.OnBeforeSaving -= BeforeSave;
        EventManager.OnFinishLoading -= AfterLoad;
    }

    private void BeforeSave(int saveID)
    {
        GlobalVariables.SetStringValue(varID, inkStory.state.ToJson());
    }

    private void AfterLoad()
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

    /*
        GatherSpeech goes through your Ink files that are in the Assets/Story/ directory and adds a lineID tag
        It then creates a SpeechLine in the SpeechManager.
        Probably a good idea to backup your Ink files before using.
     */
    //   [MenuItem("Adventure Creator/Third Party/Ink/Gather Speech (DANGER: EXPERIMENTAL)")]
    // private static void GatherSpeech()
    // {

    //     string[] inkFiles = Directory.GetFiles(Application.dataPath + "/Story/", "*.ink");
        
    //     foreach (string file in inkFiles)
    //     {
    //         string line;

    //         StreamReader r = new StreamReader(file);
    //         string finalFile = string.Empty;
    //         bool inCommentBlock = false;

    //         SpeechManager speechManager = KickStarter.speechManager;

    //         using (r)
    //         {
    //             do
    //             {
    //                 line = r.ReadLine();
    //                 if (line != null)
    //                 {
    //                     finalFile += line + "\n";

    //                     if (inCommentBlock) continue;
    //                     else if (line.Trim().StartsWith("#")) continue;
    //                     else if (line.Trim().StartsWith("->")) continue;
    //                     else if (line.Trim().StartsWith("=")) continue;
    //                     else if (line.Trim() == string.Empty) continue;
    //                     else if (line.Trim().StartsWith("EXTERNAL ")) continue;
    //                     else if (line.Trim().StartsWith("//")) continue;
    //                     else if (inCommentBlock && line.Trim().StartsWith("*/"))
    //                     {
    //                         inCommentBlock = false;
    //                         continue;
    //                     }
    //                     else if (line.Trim().StartsWith("/*"))
    //                     {
    //                         inCommentBlock = true;
    //                         continue;
    //                     }
    //                     else if (line.Trim().StartsWith("TODO:")) continue;
    //                     else if (line.Trim().StartsWith("INCLUDE ")) continue;
    //                     else if (line.Trim().StartsWith("VAR ")) continue;
    //                     else if (line.Trim().StartsWith("~")) continue;
    //                     else if (line.Contains("#audio = ")) continue;
    //                     else if (line.Contains("#lineID =")) continue;
                        

    //                     int id = 0;

    //                     while (true)
    //                     {
    //                         id++;
    //                         if (speechManager.GetLine(id) == null)
    //                         {
    //                             break;
    //                         }
    //                     }

    //                     SpeechLine newLine = new SpeechLine(id, "", "", line, 0, AC_TextType.Speech, false);
    //                     speechManager.lines.Add(newLine);
    //                     finalFile = finalFile.Insert(finalFile.LastIndexOf("\n"), string.Format(" #lineID = {0}", id));
    //                 }
    //             }
    //             while (line != null);
    //             r.Close();
    //         }

    //         StreamWriter w = new StreamWriter(file);
    //         w.Write(finalFile);
    //         w.Close();
    //         Debug.Log(finalFile);
    //     }
    // }

    
}
