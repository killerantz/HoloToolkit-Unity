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

            SetupStateOptions();
        }


        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            serializedObject.Update();

            string minus = "\u2212";
            GUIStyle smallButton = new GUIStyle(EditorStyles.miniButton);
            float minusButtonWidth = GUI.skin.button.CalcSize(new GUIContent(minus)).x;

            for (int i = 0; i < stateList.arraySize; i++)
            {
                SerializedProperty stateItem = stateList.GetArrayElementAtIndex(i);

                // index is i
                // set the bit
                // set the name

                // add
                // remove
            }

            serializedObject.ApplyModifiedProperties();
        }

        private State[] GetStates()
        {
            InteractableStates states = new InteractableStates(InteractableStates.Default);
            return states.GetStates();
        }

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

        private static string[] GetOptions(SerializedProperty options)
        {
            List<string> list = new List<string>();
            for (int i = 0; i < options.arraySize; i++)
            {
                list.Add(options.GetArrayElementAtIndex(i).stringValue);
            }

            return list.ToArray();
        }

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

        private void SetupStateOptions()
        {
            List<Type> stateTypes = new List<Type>();
            List<string> names = new List<string>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsSubclassOf(typeof(StateModel)))
                    {
                        stateTypes.Add(type);
                        names.Add(type.Name);
                    }
                }
            }

            stateOptions = names.ToArray();
            stateType = stateTypes.ToArray();
        }
        
        // redundant method, put in a utils with static methods!!!
        private static int ReverseLookup(string option, string[] options)
        {
            for (int i = 0; i < options.Length; i++)
            {
                if (options[i] == option)
                {
                    return i;
                }
            }

            return 0;
        }

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
