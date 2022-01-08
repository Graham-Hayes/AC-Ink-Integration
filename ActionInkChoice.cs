using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionInkChoice : Action
	{		
		public override ActionCategory Category { get { return ActionCategory.Custom; }}
		public override string Title { get { return "Action Ink Choice"; }}
		public override string Description { get { return "Displays a menu for an Ink choice"; }}

		protected Canvas localCanvas = null;
        protected List<string> choices;
        protected bool choiceSelected = false;
		
		public override float Run ()
		{	
			if (!isRunning)
			{
				isRunning = true;
                GameObject pe = GameObject.Find("PersistentEngine");
                Canvas canvas = pe.GetComponent<ACInkIntegration>().inkChoiceMenu;
	
                if (canvas)
				{
					localCanvas = (Canvas) Instantiate (canvas);
					localCanvas.gameObject.name = canvas.name;
					DontDestroyOnLoad (localCanvas.gameObject);
				}
                
                UnityEngine.UI.Button[] buttons = localCanvas.GetComponentsInChildren<UnityEngine.UI.Button>();

                for(int i = 0 ; i < buttons.Length ; i++){
                    UnityEngine.UI.Button b = buttons[i];
                    if(i< choices.Count){
                        b.onClick.AddListener(HandleChoice);
                        UnityEngine.UI.Text t =  b.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>();
                        t.text = choices[i];
                    }
                    else {
                        b.gameObject.SetActive(false);
                    }
                }
				
				return defaultPauseTime;    
			}
			else
			{
                if(choiceSelected){
                    isRunning = false;
                    return 0f;
                }
				return defaultPauseTime;;
			}
		}

        protected void HandleChoice()
        {
            string buttonName = EventSystem.current.currentSelectedGameObject.name;
            char choice = buttonName[buttonName.Length - 1];
            ACInkIntegration.choiceID = int.Parse(choice.ToString()) - 1;
            Debug.Log(ACInkIntegration.choiceID);
            choiceSelected = true;
			Destroy(localCanvas.gameObject);
        }

        

		public override void Skip ()
		{
			/*
			 * This function is called when the Action is skipped, as a
			 * result of the player invoking the "EndCutscene" input.
			 * 
			 * It should perform the instructions of the Action instantly -
			 * regardless of whether or not the Action itself has been run
			 * normally yet.  If this method is left blank, then skipping
			 * the Action will have no effect.  If this method is removed,
			 * or if the Run() method call is left below, then skipping the
			 * Action will cause it to run itself as normal.
			 */

			 Run ();
		}

		
		#if UNITY_EDITOR

		public override void ShowGUI ()
		{
			// Action-specific Inspector GUI code here
		}
		

		public override string SetLabel ()
		{
			// (Optional) Return a string used to describe the specific action's job.
			
			return string.Empty;
		}

		#endif

        public static ActionInkChoice CreateNew (List<string> inkChoices)
		{
			ActionInkChoice newAction = CreateNew<ActionInkChoice> ();
            newAction.choices = inkChoices;
			return newAction;
		}
		
	}

}