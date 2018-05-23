using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HoloToolkit.Unity
{
    public abstract class InspectorBase : Editor
    {
        // Colors
        protected readonly static Color defaultColor = new Color(1f, 1f, 1f);
        protected readonly static Color baseColor = new Color(0.75f, 0.75f, 0.75f);
        protected readonly static Color disabledColor = new Color(0.6f, 0.6f, 0.6f);
        protected readonly static Color warningColor = new Color(1f, 0.85f, 0.6f);
        protected readonly static Color errorColor = new Color(1f, 0.55f, 0.5f);
        protected readonly static Color successColor = new Color(0.5f, 1f, 0.5f);
        protected readonly static Color titleColor = new Color(0.5f, 0.5f, 0.5f);
        protected readonly static Color sectionColor = new Color(0.85f, 0.9f, 1f); //

        protected const int titleFontSize = 14;
        protected const int defaultFontSize = 10;

        protected static int indentOnSectionStart = 0;

        public const float DottedLineScreenSpace = 4.65f;

        protected delegate void ListButtonEvent(int index);
        protected delegate void MultiListButtonEvent(int[] arr);

        protected virtual bool RemoveButton(GUIContent label, int index, ListButtonEvent callback)
        {
            // delete button
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            
            float buttonWidth = GUI.skin.button.CalcSize(label).x;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            bool triggered = false;
            if (GUILayout.Button(label, buttonStyle, GUILayout.Width(buttonWidth)))
            {
                callback(index);
                triggered = true;
            }

            EditorGUILayout.EndHorizontal();
            return triggered;
        }

        protected virtual bool RemoveButton(GUIContent label, int[] arr, MultiListButtonEvent callback)
        {
            // delete button
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            
            float buttonWidth = GUI.skin.button.CalcSize(label).x;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            bool triggered = false;
            if (GUILayout.Button(label, buttonStyle, GUILayout.Width(buttonWidth)))
            {
                callback(arr);
                triggered = true;
            }

            EditorGUILayout.EndHorizontal();
            return triggered;
        }

        protected virtual bool AddButton(GUIContent label, float padding, int index, ListButtonEvent callback)
        {
            GUIStyle addStyle = new GUIStyle(GUI.skin.button);
            addStyle.fixedHeight = 25;
            float addButtonWidth = GUI.skin.button.CalcSize(label).x * padding;
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            bool triggered = false;
            if (GUILayout.Button(label, addStyle, GUILayout.Width(addButtonWidth)))
            {
                callback(index);
                triggered = true;
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            return triggered;
        }

        protected virtual bool AddButton(GUIContent label, float padding, int[] arr, MultiListButtonEvent callback)
        {
            GUIStyle addStyle = new GUIStyle(GUI.skin.button);
            addStyle.fixedHeight = 25;
            float addButtonWidth = GUI.skin.button.CalcSize(label).x * padding;
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            bool triggered = false;
            if (GUILayout.Button(label, addStyle, GUILayout.Width(addButtonWidth)))
            {
                callback(arr);
                triggered = true;
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            return triggered;
        }

        protected virtual bool SmallButton(GUIContent label, int index, ListButtonEvent callback)
        {

            GUIStyle smallButton = new GUIStyle(EditorStyles.miniButton);
            float smallButtonWidth = GUI.skin.button.CalcSize(new GUIContent(label)).x;

            bool triggered = false;
            if (GUILayout.Button(label, smallButton, GUILayout.Width(smallButtonWidth)))
            {
                callback(index);
                triggered = true;
            }
            return triggered;
        }

        protected virtual bool SmallButton(GUIContent label, int[] arr, MultiListButtonEvent callback)
        {
            GUIStyle smallButton = new GUIStyle(EditorStyles.miniButton);
            float smallButtonWidth = GUI.skin.button.CalcSize(label).x;

            bool triggered = false;
            if (GUILayout.Button(label, smallButton, GUILayout.Width(smallButtonWidth)))
            {
                callback(arr);
                triggered = true;
            }
            return triggered;
        }

        public static void DrawTitle(string title)
        {
            GUIStyle labelStyle = GetLableStyle(titleFontSize, titleColor);
            EditorGUILayout.LabelField(new GUIContent(title), labelStyle);
            GUILayout.Space(titleFontSize * 0.5f);
        }

        public static void DrawLabel(string title, int size, Color color)
        {
            GUIStyle labelStyle = GetLableStyle(size, color);
            EditorGUILayout.LabelField(new GUIContent(title), labelStyle);
            GUILayout.Space(titleFontSize * 0.5f);
        }

        public static GUIStyle GetLableStyle(int size, Color color)
        {
            GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.fontSize = size;
            labelStyle.fixedHeight = size * 2;
            labelStyle.normal.textColor = color;
            return labelStyle;
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
            styleHR.fixedHeight = 1;
            styleHR.border = new RectOffset(1,1,1,0);
            GUILayout.Box("", styleHR);
        }

        /// <summary>
        /// Draws a section start (initiated by the Header attribute)
        /// </summary>
        /// <param name="targetName"></param>
        /// <param name="headerName"></param>
        /// <param name="toUpper"></param>
        /// <param name="drawInitially"></param>
        /// <returns></returns>
        public static bool DrawSectionStart(string headerName, int indent, bool open = true, FontStyle style = FontStyle.Bold, bool toUpper = true, int size = 0)
        {
            GUIStyle sectionStyle = new GUIStyle(EditorStyles.foldout);
            sectionStyle.fontStyle = style;
            if (size > 0)
            {
                sectionStyle.fontSize = size;
                sectionStyle.fixedHeight = size * 2;
            }
            Color tColor = GUI.color;
            GUI.color = sectionColor;

            if (toUpper)
            {
                headerName = headerName.ToUpper();
            }

            bool drawSection = EditorGUILayout.Foldout(open, headerName, true, sectionStyle);
            EditorGUILayout.BeginVertical();
            GUI.color = tColor;
            EditorGUI.indentLevel = indent;

            return drawSection;
        }

        /// <summary>
        /// Draws section end (initiated by next Header attribute)
        /// </summary>
        public static void DrawSectionEnd(int indent)
        {
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel = indent;
        }
    }
}