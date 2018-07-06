// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HoloToolkit.Unity
{
    public abstract class InspectorBase : Editor
    {
        // Colors
        protected readonly static Color defaultColor = new Color(1f, 1f, 1f);
        protected readonly static Color disabledColor = new Color(0.6f, 0.6f, 0.6f);
        protected readonly static Color warningColor = new Color(1f, 0.85f, 0.6f);
        protected readonly static Color errorColor = new Color(1f, 0.55f, 0.5f);
        protected readonly static Color successColor = new Color(0.5f, 1f, 0.5f);
        protected readonly static Color titleColor = new Color(0.5f, 0.5f, 0.5f);

        protected readonly static int titleFontSize = 14;

        protected static int indentOnSectionStart = 0;

        public const float DottedLineScreenSpace = 4.65f;

        protected int dataCount = 0;

        protected delegate void ListButtonEvent(int index);

        public struct ListSettings
        {
            public bool ShowChars;
            public Vector2 Scroll;
        }

        protected List<ListSettings> listSettings;
        
        protected void AdjustListSettings(int count)
        {
            int diff = count - listSettings.Count;
            if (diff > 0)
            {
                for (int i = 0; i < diff; i++)
                {
                    listSettings.Add(new ListSettings() { ShowChars = false, Scroll = new Vector2() });
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
        
        protected virtual void RemoveButton(string label, int index, ListButtonEvent callback)
        {
            // delete button
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);

            GUIContent buttonLabel = new GUIContent(label);
            float buttonWidth = GUI.skin.button.CalcSize(buttonLabel).x;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(buttonLabel, buttonStyle, GUILayout.Width(buttonWidth)))
            {
                callback(index);
            }

            EditorGUILayout.EndHorizontal();
        }

        protected virtual void AddButton(string label, float padding, int index, ListButtonEvent callback)
        {
            GUIStyle addStyle = new GUIStyle(GUI.skin.button);
            addStyle.fixedHeight = 25;
            GUIContent addLabel = new GUIContent(label);
            float addButtonWidth = GUI.skin.button.CalcSize(addLabel).x * padding; //EditorGUIUtility.currentViewWidth * 0.65f;
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(addLabel, addStyle, GUILayout.Width(addButtonWidth)))
            {
                callback(index);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        protected bool IsFontValid(Font font)
        {
            if (font != null)
            {
                return true;
            }

            return false;
        }

        protected bool IsMaterialValid(Material material)
        {
            if (material != null)
            {
                return true;
            }

            return false;
        }

        public static void DrawTitle(string title)
        {
            GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
            labelStyle.fontSize = titleFontSize;
            labelStyle.fixedHeight = titleFontSize * 2;
            labelStyle.normal.textColor = titleColor;
            EditorGUILayout.LabelField(new GUIContent(title), labelStyle);
            GUILayout.Space(titleFontSize * 0.5f);

        }

        public static void DrawWarning(string warning)
        {
            Color prevColor = GUI.color;

            GUI.color = warningColor;
            EditorGUILayout.BeginVertical(EditorStyles.textArea);
            EditorGUILayout.LabelField(warning, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            GUI.color = prevColor;
        }

        public static void DrawNotice(string notice)
        {
            Color prevColor = GUI.color;

            GUI.color = defaultColor;
            EditorGUILayout.BeginVertical(EditorStyles.textArea);
            EditorGUILayout.LabelField(notice, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            GUI.color = prevColor;
        }

        public static void DrawSuccess(string notice)
        {
            Color prevColor = GUI.color;

            GUI.color = successColor;
            EditorGUILayout.BeginVertical(EditorStyles.textArea);
            EditorGUILayout.LabelField(notice, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            GUI.color = prevColor;
        }

        public static void DrawError(string error)
        {
            Color prevColor = GUI.color;

            GUI.color = errorColor;
            EditorGUILayout.BeginVertical(EditorStyles.textArea);
            EditorGUILayout.LabelField(error, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            GUI.color = prevColor;
        }

        public static void DrawDivider()
        {
            GUIStyle styleHR = new GUIStyle(GUI.skin.box);
            styleHR.stretchWidth = true;
            styleHR.fixedHeight = 2;
            GUILayout.Box("", styleHR);
        }
    }
}
