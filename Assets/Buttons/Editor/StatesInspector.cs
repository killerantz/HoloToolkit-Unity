using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace HoloToolkit.Unity
{
    [CustomEditor(typeof(States))]
    public class StatesInspector : InspectorBase
    {
        private States instance;
        private SerializedProperty stateList;
        private List<InteractableInspector.ListSettings> listSettings;

        private Type[] stateType;
        private string[] stateOptions;
        
        private void OnEnable()
        {
            instance = (States)target;

            listSettings = new List<InteractableInspector.ListSettings>();

            stateList = serializedObject.FindProperty("StateList");
            AdjustListSettings(stateList.arraySize);
            instance.SetupStateOptions();
            
        }


        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            serializedObject.Update();

            string minus = "\u2212";
            GUIStyle smallButton = new GUIStyle(EditorStyles.miniButton);
            float minusButtonWidth = GUI.skin.button.CalcSize(new GUIContent(minus)).x;

            DrawTitle("States");
            DrawNotice("Manage a state configuration to drive Interactables or Tansitions");

            // get the list of options
            SerializedProperty options = serializedObject.FindProperty("StateOptions");
            stateOptions = SerializedPropertyToOptions(options);

            SerializedProperty stateLogicName = serializedObject.FindProperty("StateLogicName");
            int option = States.ReverseLookup(stateLogicName.stringValue, stateOptions);

            int newLogic = EditorGUILayout.Popup("State Model", option, stateOptions);
            if (option != newLogic)
            {
                stateLogicName.stringValue = stateOptions[newLogic];
            }

            for (int i = 0; i < stateList.arraySize; i++)
            {
                EditorGUILayout.BeginVertical("Box");
                SerializedProperty stateItem = stateList.GetArrayElementAtIndex(i);

                SerializedProperty name = stateItem.FindPropertyRelative("Name");
                SerializedProperty index = stateItem.FindPropertyRelative("Index");
                SerializedProperty bit = stateItem.FindPropertyRelative("Bit");

                index.intValue = i;

                EditorGUILayout.BeginHorizontal();
                name.stringValue = EditorGUILayout.TextField(new GUIContent("Name", "Display name for the state"), name.stringValue);
                if (GUILayout.Button(new GUIContent(minus, "Remove State"), smallButton, GUILayout.Width(minusButtonWidth)))
                {
                    RemoveState(i);
                }
                EditorGUILayout.EndHorizontal();

                bit.intValue = EditorGUILayout.IntField(new GUIContent("Bit", "Bitwise value of the state, used for comparing state combinations"), bit.intValue);
                
                EditorGUILayout.EndVertical();
            }

            RemoveButton(new GUIContent("+", "Add Theme Property"), 0, AddState);

            serializedObject.ApplyModifiedProperties();
        }

        private void AddState(int index)
        {
            stateList.InsertArrayElementAtIndex(stateList.arraySize);
        }

        private void RemoveState(int index)
        {
            stateList.DeleteArrayElementAtIndex(index);
        }

        // remove - shouldn't be needed!
        private State[] GetStates()
        {
            InteractableStates states = new InteractableStates(InteractableStates.Default);
            return states.GetStates();
        }

        // move this to a base class and target the local settings
        private void AdjustListSettings(int count)
        {
            int diff = count - listSettings.Count;
            if (diff > 0)
            {
                for (int i = 0; i < diff; i++)
                {
                    listSettings.Add(new InteractableInspector.ListSettings() { Show = false, Scroll = new Vector2() });
                }
            }
            else if (diff < 0)
            {
                int removeCnt = 0;
                for (int i = listSettings.Count - 1; i > -1; i--)
                {
                    if (removeCnt > diff)
                    {
                        listSettings.RemoveAt(i);
                        removeCnt--;
                    }
                }
            }
        }

        // move to partent class
        private static int GetOptionsIndex(SerializedProperty options, string selection)
        {
            for (int i = 0; i < options.arraySize; i++)
            {
                if (options.GetArrayElementAtIndex(i).stringValue == selection)
                {
                    return i;
                }
            }

            return 0;
        }

        // move to partent class
        public static string[] SerializedPropertyToOptions(SerializedProperty arr)
        {
            List<string> list = new List<string>();
            for (int i = 0; i < arr.arraySize; i++)
            {
                list.Add(arr.GetArrayElementAtIndex(i).stringValue);
            }
            return list.ToArray();
        }
    }
}
