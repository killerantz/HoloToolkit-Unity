// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HoloToolkit.Unity.UX
{
    [CustomEditor(typeof(FontSet))]
    public class FontSetEditor : InspectorBase
    {
        protected FontSet inspector;
        protected SerializedObject inspectorRef;
        protected SerializedProperty dataList;
        protected SerializedProperty isEnabled;

        protected virtual void OnEnable()
        {
            inspector = (FontSet)target;
            inspectorRef = new SerializedObject(inspector);
            
            dataList = inspectorRef.FindProperty("Data");
            isEnabled = inspectorRef.FindProperty("Enabled");
            
            dataCount = dataList.arraySize;

            listSettings = new List<ListSettings>();
            AdjustListSettings(dataCount);
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            DrawTitle("FontSet");

            serializedObject.Update();
            inspectorRef.Update();

            DrawNotice("To add this set to the Fonts Menu, make sure it is enabled and choose the Menu:Fonts/Generate Fonts Menu.");

            if (target.name == FontUtils.DefaultFontSetName)
            {
                DrawWarning("This is the Default Font Set, errors may occur if it is renamed or deleted. Disable this set to remove it from the Fonts Menu, then choose Menu:Fonts/Generate Fonts Menu.");
            }
            
            isEnabled.boolValue = EditorGUILayout.Toggle(new GUIContent("Enabled"), isEnabled.boolValue);


            EditorGUILayout.BeginVertical();

            indentOnSectionStart = 0;
            EditorGUI.indentLevel = indentOnSectionStart;

            for (int i = 0; i < dataList.arraySize; i++)
            {
                ListSettings settings = listSettings[i];

                EditorGUILayout.BeginVertical("Box");
                GUILayout.Space(2);

                SerializedProperty sItem = dataList.GetArrayElementAtIndex(i);

                SerializedProperty font = sItem.FindPropertyRelative("Font");
                EditorGUILayout.PropertyField(font, new GUIContent("Font"));

                SerializedProperty name = sItem.FindPropertyRelative("Name");
                name.stringValue = EditorGUILayout.TextField(new GUIContent("Display Name"), name.stringValue);

                SerializedProperty material = sItem.FindPropertyRelative("Material");

                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(material, new GUIContent("Material"));

                // only show the create button if the Material is empty
                if (material.objectReferenceValue == null && name.stringValue.Length > 3)
                { 
                    if (GUILayout.Button(new GUIContent("Create")))
                    {
                        CreateAndAddMaterial(i);
                    }
                }

                GUILayout.EndHorizontal();
                
                Font fontRef = (Font)font.objectReferenceValue;
                Material materialRef = (Material)material.objectReferenceValue;

                SerializedProperty enabled = sItem.FindPropertyRelative("Enabled");

                if (IsFontValid(fontRef) && IsMaterialValid(materialRef) && (!string.IsNullOrEmpty(name.stringValue) && name.stringValue.Length > 2))
                {
                    enabled.boolValue = true;
                    DrawSuccess("Good to go!");
                }
                else
                {
                    enabled.boolValue = false;
                    DrawWarning("Disabled! - Required Fields: Font, Material, and valid Display Name.");
                }

                GUILayout.Space(5);
                
                listSettings[i] = settings;

                RemoveButton("Remove Font", i, RemoveItem);

                EditorGUILayout.EndVertical();
            }

            AddButton("Add Font", 1.5f, 0, AddItem);

            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel = indentOnSectionStart;

            inspectorRef.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();
        }

        private void RemoveItem(int index)
        {
            dataList.DeleteArrayElementAtIndex(index);
            listSettings.RemoveAt(index);
        }

        private void AddItem(int index)
        {
            dataList.InsertArrayElementAtIndex(dataList.arraySize);
            SerializedProperty newItem = dataList.GetArrayElementAtIndex(dataList.arraySize - 1);
            // TODO: should we clean up the new item so it doesn't copy values from previous item in the list?
            SerializedProperty isEnabled = newItem.FindPropertyRelative("Enabled");
            isEnabled.boolValue = true;

            listSettings.Add(new ListSettings() { ShowChars = false, Scroll = new Vector2() });
        }

        private void CreateAndAddMaterial(int index)
        {
            Font font = inspector.Data[index].Font;
            string name = inspector.Data[index].Name;
            Material material = FontUtils.CreateMaterial(Selection.activeObject, font, name, "_FontMat");
            
            SerializedProperty sData = dataList.GetArrayElementAtIndex(index);
            SerializedProperty sMaterial = sData.FindPropertyRelative("Material");
            sMaterial.objectReferenceValue = material;
        }
    }
}
