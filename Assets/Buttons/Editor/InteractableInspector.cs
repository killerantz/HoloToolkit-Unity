using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HoloToolkit.Unity
{
    [CustomEditor(typeof(Interactable))]
    public class InteractableInspector : Editor
    {

        private Interactable inspector;
        private SerializedObject inspectorRef;
        private SerializedProperty dataList;

        private bool showProfiles;


        // Styles
        private static GUIStyle toggleButtonOffStyle = null;
        private static GUIStyle toggleButtonOnStyle = null;
        private static GUIStyle sectionStyle = null;
        private static GUIStyle toolTipStyle = null;
        private static GUIStyle inProgressStyle = null;

        // Colors
        protected readonly static Color defaultColor = new Color(1f, 1f, 1f);
        protected readonly static Color disabledColor = new Color(0.6f, 0.6f, 0.6f);
        protected readonly static Color borderedColor = new Color(0.8f, 0.8f, 0.8f);
        protected readonly static Color warningColor = new Color(1f, 0.85f, 0.6f);
        protected readonly static Color errorColor = new Color(1f, 0.55f, 0.5f);
        protected readonly static Color successColor = new Color(0.8f, 1f, 0.75f);
        protected readonly static Color objectColor = new Color(0.85f, 0.9f, 1f);
        protected readonly static Color helpBoxColor = new Color(0.70f, 0.75f, 0.80f, 0.5f);
        protected readonly static Color sectionColor = new Color(0.85f, 0.9f, 1f);
        protected readonly static Color darkColor = new Color(0.1f, 0.1f, 0.1f);
        protected readonly static Color objectColorEmpty = new Color(0.75f, 0.8f, 0.9f);
        protected readonly static Color profileColor = new Color(0.88f, 0.7f, .97f);
        protected readonly static Color handleColorSquare = new Color(0.0f, 0.9f, 1f);
        protected readonly static Color handleColorCircle = new Color(1f, 0.5f, 1f);
        protected readonly static Color handleColorSphere = new Color(1f, 0.5f, 1f);
        protected readonly static Color handleColorAxis = new Color(0.0f, 1f, 0.2f);
        protected readonly static Color handleColorRotation = new Color(0.0f, 1f, 0.2f);
        protected readonly static Color handleColorTangent = new Color(0.1f, 0.8f, 0.5f, 0.7f);

        private static int indentOnSectionStart = 0;

        public const float DottedLineScreenSpace = 4.65f;

        private void OnEnable()
        {
            inspector = (Interactable)target;
            inspectorRef = new SerializedObject(inspector);
            dataList = inspectorRef.FindProperty("Profiles");


        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            CreateStyles();

            showProfiles = DrawSectionStart("Profiles", showProfiles);

            serializedObject.Update();

            if (showProfiles && dataList != null)
            {
                for (int i = 0; i < dataList.arraySize; i++)
                {
                    
                    ProfileItem item = inspector.Profiles[i];
                    SerializedProperty sItem = dataList.GetArrayElementAtIndex(i);

                    SerializedProperty gameObject = sItem.FindPropertyRelative("Target");

                    EditorGUILayout.PropertyField(gameObject, new GUIContent("Target"));
                    

                }

                // add content if true;
               
            }

            DrawSectionEnd();

            serializedObject.ApplyModifiedProperties();

        }

        /// <summary>
        /// Draws a section start (initiated by the Header attribute)
        /// </summary>
        /// <param name="targetName"></param>
        /// <param name="headerName"></param>
        /// <param name="toUpper"></param>
        /// <param name="drawInitially"></param>
        /// <returns></returns>
        public static bool DrawSectionStart(string headerName, bool open = true)
        {
            
            EditorGUILayout.Space();
            Color tColor = GUI.color;
            GUI.color = sectionColor;

            headerName = headerName.ToUpper();

            bool drawSection = EditorGUILayout.Foldout(open, headerName, true, sectionStyle);
            EditorGUILayout.BeginVertical();
            GUI.color = tColor;

            indentOnSectionStart = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;// indentOnSectionStart + 1;

            return drawSection;
        }

        /// <summary>
        /// Draws section end (initiated by next Header attribute)
        /// </summary>
        public static void DrawSectionEnd()
        {
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel = indentOnSectionStart;
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

        private void CreateStyles()
        {
            if (toggleButtonOffStyle == null)
            {
                toggleButtonOffStyle = "ToolbarButton";
                toggleButtonOffStyle.fontSize = 9;
                toggleButtonOnStyle = new GUIStyle(toggleButtonOffStyle);
                toggleButtonOnStyle.normal.background = toggleButtonOnStyle.active.background;

                sectionStyle = new GUIStyle(EditorStyles.foldout);
                sectionStyle.fontStyle = FontStyle.Bold;

                toolTipStyle = new GUIStyle(EditorStyles.wordWrappedMiniLabel);
                toolTipStyle.fontStyle = FontStyle.Normal;
                toolTipStyle.alignment = TextAnchor.LowerLeft;

                inProgressStyle = new GUIStyle(EditorStyles.wordWrappedMiniLabel);
                inProgressStyle.fontStyle = FontStyle.Italic;
                inProgressStyle.alignment = TextAnchor.LowerLeft;
            }
        }
    }
}
