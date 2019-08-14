using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
    [System.Serializable]

    public class ActionInkCheckVisitCount : ActionCheck
    {
        public string knotName;
        public IntCondition condition;
        public int compareNumber;

        public ActionInkCheckVisitCount()
        {
            this.isDisplayed = true;
            category = ActionCategory.ThirdParty;
            title = "Ink: Check Visit Count";
            description = "Checks the visit count of a specific knot/stitch.";
        }

        public override bool CheckCondition(){

            int visitCount;

            try{
                visitCount = ACInkIntegration.inkStory.state.VisitCountAtPathString(knotName);
            }
            catch(Exception e){
                Debug.Log(e.Message);
                return false;
            }

            switch(condition)
            {
                case IntCondition.EqualTo:
                    if(compareNumber == visitCount) return true;
                    break;
                case IntCondition.LessThan:
                    if(visitCount < compareNumber) return true;
                    break;
                case IntCondition.MoreThan:
                    if(visitCount > compareNumber) return true;
                    break;
                case IntCondition.NotEqualTo:
                    if(visitCount != compareNumber) return true;
                    break;
            }
            return false;
        } 
        
        #if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
            knotName = EditorGUILayout.TextField("Knot/Stitch:", knotName);
            condition = (IntCondition) EditorGUILayout.EnumPopup("Condition:", condition);
            compareNumber = EditorGUILayout.IntField("Number:", compareNumber);
        }
        #endif
    }
}