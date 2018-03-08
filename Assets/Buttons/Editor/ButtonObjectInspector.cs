using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Buttons
{
    [CustomEditor(typeof(ButtonObject))]
    public class ButtonObjectInspector : Editor
    {

        protected ButtonObject inspector;
        protected SerializedObject inspectorRef;
        protected SerializedProperty dataList;

        private void OnEnable()
        {
            // define the inspector from the host class?
            inspector = (ButtonObject)target;
            inspectorRef = new SerializedObject(inspector);
            dataList = inspectorRef.FindProperty("Settings");

            // how to format inspectors
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();
            inspectorRef.Update();

            // find all the values and create the right controls to edit them

            SerializedProperty clickEvent = inspectorRef.FindProperty("MainEvent");
            SerializedProperty clickTest = serializedObject.FindProperty("MainEvent");
            EditorGUILayout.PropertyField(clickEvent, new GUIContent("OnClick"));
            
            serializedObject.ApplyModifiedProperties();
            inspectorRef.ApplyModifiedProperties();


        }
    }
}
