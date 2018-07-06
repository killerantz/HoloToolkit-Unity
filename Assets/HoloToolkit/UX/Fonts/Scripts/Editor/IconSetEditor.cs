// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HoloToolkit.Unity.UX
{
    [CustomEditor(typeof(IconSet))]
    public class IconSetEditor : InspectorBase
    {
        protected IconSet inspector;
        protected SerializedObject inspectorRef;
        protected SerializedProperty dataList;
        
        protected virtual void OnEnable()
        {
            inspector = (IconSet)target;
            inspectorRef = new SerializedObject(inspector);
            dataList = inspectorRef.FindProperty("Data");

            dataCount = dataList.arraySize;

            listSettings = new List<ListSettings>();
            AdjustListSettings(dataCount);
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            
            DrawTitle("IconSet");

            serializedObject.Update();
            inspectorRef.Update();

            DrawNotice("To add this set to the IconViewer, make sure each Icon is enabled and choose the Menu:Fonts/Generate Fonts Menu.");

            if (target.name == FontUtils.DefaultIconSetName)
            {
                DrawWarning("This is the Default Icon Set, errors may occur if it is renamed or deleted. Disable the icons to remove them from the IconViewer, then choose Menu:Fonts/Generate Fonts Menu.");
            }

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

                SerializedProperty charCodes = sItem.FindPropertyRelative("CharCodes");

                if (IsFontValid(fontRef) && IsMaterialValid(materialRef) && (!string.IsNullOrEmpty(name.stringValue) && name.stringValue.Length > 2))
                {
                    SerializedProperty enabled = sItem.FindPropertyRelative("Enabled");
                    enabled.boolValue = EditorGUILayout.Toggle(new GUIContent("Enabled"), enabled.boolValue);

                    EditorGUI.indentLevel = indentOnSectionStart + 1;
                    
                    if (charCodes.arraySize > 0) {
                        GUILayout.Space(3);
                        
                        GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
                        foldoutStyle.fontStyle = FontStyle.Bold;
                        settings.ShowChars = EditorGUILayout.Foldout(settings.ShowChars, new GUIContent("Char Codes (" + charCodes.arraySize + ")"), true, foldoutStyle);
                    }

                    if (charCodes.arraySize > 0 && settings.ShowChars)
                    {
                        //display chars
                        IconSetData item = inspector.Data[i];
                        IconData[] icons = item.CharCodes.ToArray();

                        IconScrollView window = IconScrollView.Create(new Vector2(20, 25), 1, settings.Scroll, fontRef, 14, (Material)material.objectReferenceValue, icons, 9, true, ButtonCallback, i);
                        settings.Scroll = window.ScrollPosition;
                    }

                    if (charCodes.arraySize < 1)
                    {
                        if (GUILayout.Button(new GUIContent("Import Glyphs")))
                        {
                            // create codes
                            charCodes = IconScrollView.GetSerializedCodesFromFont(fontRef, charCodes);
                        }
                    }
                    
                    EditorGUI.indentLevel = indentOnSectionStart;

                    if(charCodes.arraySize < 1)
                    {
                        DrawWarning("Click the Import Glyphs button to import the icon gyphs.");
                    }
                    else
                    {
                        DrawSuccess("Good to go!");
                    }
                    
                }
                else
                {
                    // clear codes
                    if (charCodes.arraySize > 0)
                    {
                        List<IconData> codes = new List<IconData>();
                        charCodes.arraySize = codes.Count;
                        IconSetData item = inspector.Data[i];
                        item.SetCharCodes(codes);
                    }

                    DrawWarning("Required Fields: Font, Material, and valid Display Name.");
                }

                GUILayout.Space(5);


                listSettings[i] = settings;

                RemoveButton("Remove Icon Font", i, RemoveItem);

                EditorGUILayout.EndVertical();
            }

            AddButton("Add Icon Font", 1.5f, 0, AddItem);

            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel = indentOnSectionStart;

            serializedObject.ApplyModifiedProperties();
            inspectorRef.ApplyModifiedProperties();

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
            SerializedProperty isEnabled = newItem.FindPropertyRelative("Enabled");
            isEnabled.boolValue = true;

            listSettings.Add(new ListSettings() { ShowChars = false, Scroll = new Vector2() });
        }

        private void CreateAndAddMaterial(int index)
        {
            Font font = inspector.Data[index].Font;
            string name = inspector.Data[index].Name;
            Material material = FontUtils.CreateMaterial(Selection.activeObject, font, name, "_IconMat");

            SerializedProperty sData = dataList.GetArrayElementAtIndex(index);
            SerializedProperty sMaterial = sData.FindPropertyRelative("Material");
            sMaterial.objectReferenceValue = material;
        }

        protected void ButtonCallback(int index, string label, IconButtonSettings settings)
        {
            bool isEnabled = inspector.Data[settings.hostIndex].getCharCodeEnabled(index);
            inspector.Data[settings.hostIndex].SetCharCodeEnabled(index, !isEnabled);
        }
    }
}
