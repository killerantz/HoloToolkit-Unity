// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HoloToolkit.Unity.UX
{
    [CustomEditor(typeof(FontSetManager))]
    public class FontSetManagerEditor : InspectorBase
    {
        private SerializedProperty fontDataFonts;
        private SerializedProperty fontDataIcons;
        
        private void OnEnable()
        {
            fontDataFonts = serializedObject.FindProperty("FontList");
            fontDataIcons = serializedObject.FindProperty("IconList");
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            serializedObject.Update();
            
            GUILayout.BeginVertical();
            DrawWarning("Materials and configuration files will be created outside of this package, please DO NOT edit or remove this file.");

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField(new GUIContent("Font List"));
            
            for (int i = 0; i < fontDataFonts.arraySize; i++)
            {
                GUILayout.BeginVertical("Box");

                SerializedProperty sItem = fontDataFonts.GetArrayElementAtIndex(i);

                SerializedProperty font = sItem.FindPropertyRelative("Font");
                EditorGUILayout.PropertyField(font, new GUIContent("Font"));

                SerializedProperty name = sItem.FindPropertyRelative("Name");
                name.stringValue = EditorGUILayout.TextField(new GUIContent("Display Name"), name.stringValue);

                RemoveButton("Remove Font", i, RemoveFont);

                GUILayout.EndVertical();
            }

            AddButton("Add Font", 1.5f, 0, AddFont);
            
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField(new GUIContent("Icon List"));

            for (int i = 0; i < fontDataIcons.arraySize; i++)
            {
                GUILayout.BeginVertical("Box");

                SerializedProperty sItem = fontDataIcons.GetArrayElementAtIndex(i);

                SerializedProperty icon = sItem.FindPropertyRelative("Font");
                EditorGUILayout.PropertyField(icon, new GUIContent("Font"));

                SerializedProperty name = sItem.FindPropertyRelative("Name");
                name.stringValue = EditorGUILayout.TextField(new GUIContent("Display Name"), name.stringValue);

                SerializedProperty chars = sItem.FindPropertyRelative("IconData");

                if (icon.objectReferenceValue == null)
                {
                    chars.arraySize = 0;
                }

                if (chars.arraySize < 1)
                {
                    if (GUILayout.Button(new GUIContent("Import Glyphs")))
                    {
                        Font fontRef = (Font)icon.objectReferenceValue;

                        // create codes
                        SerializedProperty charCodes = sItem.FindPropertyRelative("IconData");
                        charCodes = IconScrollView.GetSerializedCodesFromFont(fontRef, charCodes);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField(new GUIContent("Icon CharCodes: " + chars.arraySize));
                }

                RemoveButton("Remove Icon", i, RemoveIcon);

                GUILayout.EndVertical();
            }

            AddButton("Add Icon", 1.5f, 0, AddIcon);

            GUILayout.EndVertical();

            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        private void AddFont(int index)
        {
            fontDataFonts.InsertArrayElementAtIndex(fontDataFonts.arraySize);
        }

        private void RemoveFont(int index)
        {
            fontDataFonts.DeleteArrayElementAtIndex(index);
        }

        private void AddIcon(int index)
        {
            fontDataIcons.InsertArrayElementAtIndex(fontDataIcons.arraySize);
        }

        private void RemoveIcon(int index)
        {
            fontDataIcons.DeleteArrayElementAtIndex(index);
        }
    }
}
