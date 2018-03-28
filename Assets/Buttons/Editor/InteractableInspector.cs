using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HoloToolkit.Unity
{
    [CustomEditor(typeof(Interactable))]
    public class InteractableInspector : InspectorBase
    {

        public struct ListSettings
        {
            public bool Show;
            public Vector2 Scroll;
        }

        public struct EventSettings
        {
            public bool Show;
            public Vector2 Scroll;
        }

        private Interactable instance;
        private List<InteractableEvent> eventList;
        private SerializedProperty profileList;
        private static bool showProfiles;
        private string prefKey = "InteractableInspectorProfiles";
        private List<ListSettings> listSettings;
        private List<EventSettings> eventSettings;
        private bool enabled = false;

        private string[] eventOptions;
        private Type[] eventTypes;
        private string[] themeOptions;
        private Type[] themeTypes;
        private string[] shaderOptions;

        private static bool ProfilesSetup = false;

        private void OnEnable()
        {
            instance = (Interactable)target;
            eventList = instance.Events;

            listSettings = new List<ListSettings>();

            profileList = serializedObject.FindProperty("Profiles");
            AdjustListSettings(profileList.arraySize);
            showProfiles = EditorPrefs.GetBool(prefKey, showProfiles);

            SetupEventOptions();
            SetupThemeOptions();

            enabled = true;
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            // extend the preference array to handle multiple themes open and scroll values!!!
            // add  messaging!!!
            // handle dimensions
            // add profiles
            // add themes
            // handle/display properties from themes

            //!!!!! need to make sure we refresh the shader list when the target changes

            serializedObject.Update();

            EditorGUILayout.Space();
            DrawTitle("Interactable");
            //EditorGUILayout.LabelField(new GUIContent("Interactable Settings"));

            EditorGUILayout.BeginVertical("Box");

            //standard UI
            SerializedProperty enabled = serializedObject.FindProperty("Enabled");
            enabled.boolValue = EditorGUILayout.Toggle(new GUIContent("Enabled", "Is this Interactable Enabled?"), enabled.boolValue);

            SerializedProperty selected = serializedObject.FindProperty("ButtonPressFilter");
            selected.enumValueIndex = (int)(InteractionSourcePressInfo)EditorGUILayout.EnumPopup(new GUIContent("Interact Filter", "Input source for this Interactable, Default: Select"), (InteractionSourcePressInfo)selected.enumValueIndex);

            // should IsGlobal only show up on specific press types and indent?
            SerializedProperty isGlobal = serializedObject.FindProperty("IsGlobal");
            isGlobal.boolValue = EditorGUILayout.Toggle(new GUIContent("Is Global", "Like a modal, does not require focus"), isGlobal.boolValue);

            SerializedProperty voiceCommands = serializedObject.FindProperty("VoiceCommand");
            voiceCommands.stringValue = EditorGUILayout.TextField(new GUIContent("Voice Command", "A voice command to trigger the click event"), voiceCommands.stringValue);

            string minus = "\u2212";
            GUIStyle smallButton = new GUIStyle(EditorStyles.miniButton);
            float minusButtonWidth = GUI.skin.button.CalcSize(new GUIContent(minus)).x;

            // show requires gaze is voice command has a value
            if (!string.IsNullOrEmpty(voiceCommands.stringValue))
            {
                EditorGUI.indentLevel = indentOnSectionStart + 1;

                SerializedProperty requireGaze = serializedObject.FindProperty("RequiresGaze");
                requireGaze.boolValue = EditorGUILayout.Toggle(new GUIContent("Requires Gaze", "Does the voice command require gazing at this interactable?"), requireGaze.boolValue);

                EditorGUI.indentLevel = indentOnSectionStart;
            }

            SerializedProperty dimensions = serializedObject.FindProperty("Dimensions");
            dimensions.intValue = EditorGUILayout.IntField(new GUIContent("Dimensions", "Toggle or squence button levels"), dimensions.intValue);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            DrawDivider();

            string profileTitle = "Profiles";
            if (!ProfilesSetup && !showProfiles)
            {
                DrawError("Profiles need to be setup or has errors!");
            }

            // profiles section
            bool isOPen = DrawSectionStart(profileTitle, indentOnSectionStart + 1, showProfiles, GetLableStyle(titleFontSize, titleColor).fontStyle, false, titleFontSize);

            if (showProfiles != isOPen)
            {
                showProfiles = isOPen;
                EditorPrefs.SetBool(prefKey, showProfiles);
            }

            if (profileList.arraySize < 1)
            {
                AddProfile(0);
            }

            int validProfileCnt = 0;
            int themeCnt = 0;

            if (showProfiles)
            {
                for (int i = 0; i < profileList.arraySize; i++)
                {
                    EditorGUILayout.BeginVertical("Box");
                    // get profiles
                    SerializedProperty sItem = profileList.GetArrayElementAtIndex(i);
                    EditorGUI.indentLevel = indentOnSectionStart;

                    SerializedProperty gameObject = sItem.FindPropertyRelative("Target");
                    string targetName = "Profile " + (i+1);
                    if (gameObject.objectReferenceValue != null)
                    {
                        targetName = gameObject.objectReferenceValue.name;
                        validProfileCnt++;
                    }

                    EditorGUILayout.BeginHorizontal();
                    DrawLabel(targetName, 12, baseColor);
                   
                    if (GUILayout.Button(new GUIContent(minus, "Remove Profile"), smallButton, GUILayout.Width(minusButtonWidth)))
                    {
                        RemoveProfile(i);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.indentLevel = indentOnSectionStart + 1;
                    EditorGUILayout.PropertyField(gameObject, new GUIContent("Target", "Target gameObject for this theme properties to manipulate"));
                    
                    // get themes
                    SerializedProperty themes = sItem.FindPropertyRelative("Themes");

                    // make sure there are enough themes as dimensions
                    if (themes.arraySize > dimensions.intValue)
                    {
                        // make sure there are not more themes than dimensions
                    }

                    if (themes.arraySize < dimensions.intValue)
                    {
                        int cnt = themes.arraySize;
                        for (int j = cnt; j < dimensions.intValue; j++)
                        {
                            themes.InsertArrayElementAtIndex(themes.arraySize);
                            SerializedProperty theme = themes.GetArrayElementAtIndex(themes.arraySize - 1);

                            // make sure there is only one or make unique
                            string[] themeLocations = AssetDatabase.FindAssets("DefaultTheme t:Theme");
                            if (themeLocations.Length > 0)
                            {
                                string path = AssetDatabase.GUIDToAssetPath(themeLocations[0]);
                                Theme defaultTheme = (Theme)AssetDatabase.LoadAssetAtPath(path, typeof(Theme));
                                theme.objectReferenceValue = defaultTheme;
                            }
                        }
                    }

                    for (int t = 0; t < themes.arraySize; t++)
                    {
                        SerializedProperty themeItem = themes.GetArrayElementAtIndex(themes.arraySize - 1);
                        EditorGUILayout.PropertyField(themeItem, new GUIContent("Theme", "Theme properties for interation feedback"));

                        // we need the theme and target in order to figure out what properties to expose in the list
                        // or do we show them all and show alerts when a theme property is not compatable
                        if (themeItem.objectReferenceValue != null && gameObject.objectReferenceValue)
                        {
                            SerializedProperty hadDefault = sItem.FindPropertyRelative("HadDefaultTheme");
                            hadDefault.boolValue = true;
                            EditorGUI.indentLevel = indentOnSectionStart + 2;

                            string prefKey = target.name + "Profiles" + i + "_Theme" + t + "_Edit";
                            bool showSettings = EditorPrefs.GetBool(prefKey);

                            ListSettings settings = listSettings[i];
                            bool show = DrawSectionStart(themeItem.objectReferenceValue.name + " (Click to edit)", indentOnSectionStart + 3, showSettings, FontStyle.Normal, false);

                            if (show != showSettings)
                            {
                                EditorPrefs.SetBool(prefKey, show);
                            }

                            // redundent!!!!
                            settings.Show = show;

                            if (show)
                            {
                                GUIStyle box = new GUIStyle(GUI.skin.box);
                                box.margin.left = 30;

                                GUILayout.Space(5);
                                SerializedObject themeObj = new SerializedObject(themeItem.objectReferenceValue);
                                themeObj.Update();

                                SerializedProperty themeObjSettings = themeObj.FindProperty("Settings");

                                if (themeObjSettings.arraySize < 1)
                                {
                                    AddThemeProperty(new int[] { i, t, 0 });
                                }

                                for (int n = 0; n < themeObjSettings.arraySize; n++)
                                {
                                    SerializedProperty settingsItem = themeObjSettings.GetArrayElementAtIndex(n);
                                    SerializedProperty className = settingsItem.FindPropertyRelative("Name");

                                    EditorGUI.indentLevel = indentOnSectionStart;

                                    EditorGUILayout.BeginVertical(box);
                                    // a dropdown for the type of theme, they should make sense
                                    // show event dropdown
                                    int id = ReverseLookup(className.stringValue, themeOptions);

                                    EditorGUILayout.BeginHorizontal();
                                    int newId = EditorGUILayout.Popup("Theme Property", id, themeOptions);

                                    if (n > 0) {
                                        if (GUILayout.Button(new GUIContent(minus, "Remove Theme Property"), smallButton, GUILayout.Width(minusButtonWidth)))
                                        {
                                            RemoveThemeProperty(new int[] { i, t, n });
                                        }
                                    }

                                    EditorGUILayout.EndHorizontal();

                                    if (id != newId)
                                    {
                                        className.stringValue = themeOptions[newId];

                                        themeObj = ChangeThemeProperty(n, themeObj, gameObject);
                                    }

                                    SerializedProperty sProps = settingsItem.FindPropertyRelative("Properties");
                                    EditorGUI.indentLevel = indentOnSectionStart + 1;
                                    int idCount = 0;
                                    for (int p = 0; p < sProps.arraySize; p++)
                                    {
                                        SerializedProperty item = sProps.GetArrayElementAtIndex(p);

                                        SerializedProperty propId = item.FindPropertyRelative("PropId");
                                        SerializedProperty name = item.FindPropertyRelative("Name");

                                        SerializedProperty shaderList = item.FindPropertyRelative("ShaderOptions");
                                        SerializedProperty shaderNames = item.FindPropertyRelative("ShaderOptionNames");

                                        if (shaderNames.arraySize > 0)
                                        {
                                            // show shader property dropdown
                                            if (idCount < 1)
                                            {
                                                GUILayout.Space(5);
                                            }
                                            GUIStyle popupStyle = new GUIStyle(EditorStyles.popup);
                                            popupStyle.margin.right = Mathf.RoundToInt(Screen.width * 0.25f);
                                            propId.intValue = EditorGUILayout.Popup("Material " + name.stringValue + "Id", propId.intValue, SerializedPropertyToOptions(shaderNames), popupStyle);
                                            idCount++;
                                        }
                                    }
                                    EditorGUI.indentLevel = indentOnSectionStart;
                                    GUILayout.Space(5);
                                    DrawDivider();

                                    // show theme properties
                                    SerializedProperty easing = settingsItem.FindPropertyRelative("Easing");
                                    SerializedProperty ease = easing.FindPropertyRelative("EaseValues");

                                    ease.boolValue = EditorGUILayout.Toggle(new GUIContent("Easing", "should the theme animate state values"), ease.boolValue);
                                    if (ease.boolValue)
                                    {
                                        EditorGUI.indentLevel = indentOnSectionStart + 1;
                                        SerializedProperty time = easing.FindPropertyRelative("LerpTime");
                                        //time.floatValue = 0.5f;
                                        SerializedProperty curve = easing.FindPropertyRelative("Curve");
                                        //curve.animationCurveValue = AnimationCurve.Linear(0, 1, 1, 1);

                                        time.floatValue = EditorGUILayout.FloatField(new GUIContent("Duration", "animation duration"), time.floatValue);
                                        EditorGUILayout.PropertyField(curve, new GUIContent("Animation Curve"));

                                        EditorGUI.indentLevel = indentOnSectionStart;
                                    }

                                    if (n > 0)
                                    {
                                        //RemoveButton("Remove Property", new int[] {i,t,n}, RemoveThemeProperty);
                                    }
                                    EditorGUILayout.EndVertical();
                                }

                                /*
                                if (GUILayout.Button(new GUIContent("Add Theme Property")))
                                {
                                    AddThemeProperty(new int[] { i, t, 0 });
                                }*/

                                RemoveButton(new GUIContent("+", "Add Theme Property"), new int[] { i, t, 0 }, AddThemeProperty);
                                // get list of all the properties from the themes

                                RenderThemeSettings(themeObjSettings, instance.GetStates(), 30);

                                themeObj.ApplyModifiedProperties();
                            }

                            DrawSectionEnd(indentOnSectionStart + 2);
                            listSettings[i] = settings;

                            validProfileCnt++;
                        }
                        else
                        {
                            string themeMsg = "Assign a ";
                            if (gameObject.objectReferenceValue == null)
                            {
                                themeMsg += "Target ";
                            }

                            if (themeItem.objectReferenceValue == null)
                            {
                                if (gameObject.objectReferenceValue == null)
                                {
                                    themeMsg += "and ";
                                }
                                themeMsg += "Theme ";
                            }

                            themeMsg += "above to add visual effects";

                            SerializedProperty hadDefault = sItem.FindPropertyRelative("HadDefaultTheme");
                            if (!hadDefault.boolValue && t == 0)
                            {
                                string[] themeLocations = AssetDatabase.FindAssets("t:Theme, DefaultTheme");
                                
                                if (themeLocations.Length > 0)
                                {
                                    string path = AssetDatabase.GUIDToAssetPath(themeLocations[0]);
                                    Theme defaultTheme = (Theme)AssetDatabase.LoadAssetAtPath(path, typeof(Theme));
                                    themeItem.objectReferenceValue = defaultTheme;
                                    if (themeItem.objectReferenceValue != null)
                                    {
                                        hadDefault.boolValue = true;
                                    }
                                }
                                else
                                {
                                    DrawError("DefaultTheme missing from project!");
                                }
                            }

                            DrawError(themeMsg);
                        }
                    }

                    EditorGUI.indentLevel = indentOnSectionStart;

                    if (i > 0)
                    {
                       // RemoveButton(new GUIContent("Remove Profile"), i, RemoveProfile);
                    }

                    EditorGUILayout.EndVertical();

                    themeCnt += themes.arraySize;

                }

                if (GUILayout.Button(new GUIContent("Add Profile")))
                {
                    AddProfile(profileList.arraySize);
                }
            }
            else
            {
                for (int i = 0; i < profileList.arraySize; i++)
                {
                    SerializedProperty sItem = profileList.GetArrayElementAtIndex(i);
                    SerializedProperty gameObject = sItem.FindPropertyRelative("Target");
                    SerializedProperty themes = sItem.FindPropertyRelative("Themes");

                    if (gameObject.objectReferenceValue != null)
                    {
                        validProfileCnt++;
                    }

                    for (int t = 0; t < themes.arraySize; t++)
                    {
                        SerializedProperty themeItem = themes.GetArrayElementAtIndex(themes.arraySize - 1);
                        if (themeItem.objectReferenceValue != null && gameObject.objectReferenceValue)
                        {
                            validProfileCnt++;
                            SerializedProperty hadDefault = sItem.FindPropertyRelative("HadDefaultTheme");
                            hadDefault.boolValue = true;
                        }
                    }

                    themeCnt += themes.arraySize;
                }
            }

            ProfilesSetup = validProfileCnt == profileList.arraySize + themeCnt;

            DrawSectionEnd(indentOnSectionStart);
            EditorGUILayout.Space();
            DrawDivider();

            // Events section
            DrawTitle("Events");
            //EditorGUILayout.LabelField(new GUIContent("Events"));

            SerializedProperty onClick = serializedObject.FindProperty("OnClick");
            EditorGUILayout.PropertyField(onClick, new GUIContent("OnClick"));

            SerializedProperty events = serializedObject.FindProperty("Events");
            
            for (int i = 0; i < events.arraySize; i++)
            {
                EditorGUILayout.BeginVertical("Box");
                SerializedProperty eventItem = events.GetArrayElementAtIndex(i);
                SerializedProperty uEvent = eventItem.FindPropertyRelative("Event");
                SerializedProperty eventName = eventItem.FindPropertyRelative("Name");
                SerializedProperty className = eventItem.FindPropertyRelative("ClassName");
                EditorGUILayout.PropertyField(uEvent, new GUIContent(eventName.stringValue));

                // show event dropdown
                int id = ReverseLookup(className.stringValue, eventOptions);
                int newId = EditorGUILayout.Popup("Select Event Type", id, eventOptions);

                if (id != newId)
                {
                    className.stringValue = eventOptions[newId];

                    ChangeEvent(i);
                }

                // show event properties
                EditorGUI.indentLevel = indentOnSectionStart + 1;
                SerializedProperty eventSettings = eventItem.FindPropertyRelative("Settings");
                for (int j = 0; j < eventSettings.arraySize; j++)
                {
                    DisplayPropertyField(eventSettings.GetArrayElementAtIndex(j));
                }
                EditorGUI.indentLevel = indentOnSectionStart;

                EditorGUILayout.Space();

                RemoveButton(new GUIContent("Remove Event"), i, RemoveEvent);

                EditorGUILayout.EndVertical();
            }

            if (eventOptions.Length > 1)
            {
                if (GUILayout.Button(new GUIContent("Add Event")))
                {
                    AddEvent(events.arraySize);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private string[] GetEventList()
        {
            return new string[] { };
        }

        private void AdjustListSettings(int count)
        {
            int diff = count - listSettings.Count;
            if (diff > 0)
            {
                for (int i = 0; i < diff; i++)
                {
                    listSettings.Add(new ListSettings() { Show = false, Scroll = new Vector2() });
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

        private void AddProfile(int index)
        {
            profileList.InsertArrayElementAtIndex(profileList.arraySize);
            SerializedProperty newItem = profileList.GetArrayElementAtIndex(profileList.arraySize - 1);

            SerializedProperty newTarget = newItem.FindPropertyRelative("Target");
            SerializedProperty themes = newItem.FindPropertyRelative("Themes");
            newTarget.objectReferenceValue = null;

            themes.ClearArray();

            listSettings.Add(new ListSettings() { Show = false, Scroll = new Vector2() });
        }

        private void RemoveProfile(int index)
        {
            profileList.DeleteArrayElementAtIndex(index);
        }
        
        private void AddThemeProperty(int[] arr)
        {
            int profile = arr[0];
            int theme = arr[1];
            int index = arr[2];

            SerializedProperty dimensions = serializedObject.FindProperty("Dimensions");

            SerializedProperty sItem = profileList.GetArrayElementAtIndex(profile);
            SerializedProperty themes = sItem.FindPropertyRelative("Themes");
            SerializedProperty target = sItem.FindPropertyRelative("Target");

            SerializedProperty themeItem = themes.GetArrayElementAtIndex(theme);
            SerializedObject themeObj = new SerializedObject(themeItem.objectReferenceValue);
            themeObj.Update();

            SerializedProperty themeObjSettings = themeObj.FindProperty("Settings");
            themeObjSettings.InsertArrayElementAtIndex(themeObjSettings.arraySize);

            SerializedProperty settingsItem = themeObjSettings.GetArrayElementAtIndex(themeObjSettings.arraySize-1);
            SerializedProperty className = settingsItem.FindPropertyRelative("Name");
            if (themeObjSettings.arraySize == 1) {
                
                className.stringValue = "ScaleOffsetColorTheme";
            }
            else
            {
                className.stringValue = themeOptions[0];
            }

            SerializedProperty easing = settingsItem.FindPropertyRelative("Easing");

            SerializedProperty time = easing.FindPropertyRelative("LerpTime");
            SerializedProperty curve = easing.FindPropertyRelative("Curve");
            time.floatValue = 0.5f;
            curve.animationCurveValue = AnimationCurve.Linear(0, 1, 1, 1);

            themeObj = ChangeThemeProperty(themeObjSettings.arraySize - 1, themeObj, target, true);

            themeObj.ApplyModifiedProperties();
        }

        private void RemoveThemeProperty(int[] arr)
        {
            int profile = arr[0];
            int theme = arr[1];
            int index = arr[2];

            SerializedProperty sItem = profileList.GetArrayElementAtIndex(profile);
            SerializedProperty themes = sItem.FindPropertyRelative("Themes");

            SerializedProperty themeItem = themes.GetArrayElementAtIndex(theme);
            SerializedObject themeObj = new SerializedObject(themeItem.objectReferenceValue);
            themeObj.Update();

            SerializedProperty themeObjSettings = themeObj.FindProperty("Settings");
            themeObjSettings.DeleteArrayElementAtIndex(index);

            themeObj.ApplyModifiedProperties();

        }

        private SerializedObject ChangeThemeProperty(int index, SerializedObject themeObj, SerializedProperty target, bool isNew = false)
        {
            
            SerializedProperty themeObjSettings = themeObj.FindProperty("Settings");
            SerializedProperty settingsItem = themeObjSettings.GetArrayElementAtIndex(index);

            SerializedProperty className = settingsItem.FindPropertyRelative("Name");

            // get class value types
            if (!String.IsNullOrEmpty(className.stringValue))
            {
                int propIndex = ReverseLookup(className.stringValue, themeOptions);
                GameObject gameObject = (GameObject)target.objectReferenceValue;

                ThemeBase themeBase = (ThemeBase)Activator.CreateInstance(themeTypes[propIndex], gameObject);

                // does this object have the right component types
                SerializedProperty isValid = settingsItem.FindPropertyRelative("IsValid");
                bool valid = false;
                
                bool hasText = false;
                bool hasRenderer = false;

                if (gameObject != null)
                {
                    for (int i = 0; i < themeBase.Types.Length; i++)
                    {
                        Type type = themeBase.Types[i];
                        if (gameObject.gameObject.GetComponent(type))
                        {
                            if (type == typeof(TextMesh) || type == typeof(Text))
                            {
                                hasText = true;
                            }

                            if(type == typeof(Renderer))
                            {
                                hasRenderer = true;
                            }

                            valid = true;
                        }
                    }
                }

                isValid.boolValue = valid;

                // setup the values
                // get the state names

                List<ThemeProperty> properties = themeBase.ThemeProperties;

                SerializedProperty sProps = settingsItem.FindPropertyRelative("Properties");
                sProps.ClearArray();

                for (int i = 0; i < properties.Count; i++)
                {
                    sProps.InsertArrayElementAtIndex(sProps.arraySize);
                    SerializedProperty item = sProps.GetArrayElementAtIndex(sProps.arraySize - 1);

                    SerializedProperty name = item.FindPropertyRelative("Name");
                    SerializedProperty type = item.FindPropertyRelative("Type");
                    SerializedProperty values = item.FindPropertyRelative("Values");
                    if (isNew)
                    {
                        values.ClearArray();
                    }
                    
                    name.stringValue = properties[i].Name;
                    type.intValue = (int)properties[i].Type;

                    int valueCount = instance.GetStateCount();
                    State[] states = instance.GetStates();
                    //! can I find out if this has been initiated so I only set defaults first time?
                    for (int j = 0; j < valueCount; j++)
                    {
                        values.InsertArrayElementAtIndex(values.arraySize);
                        SerializedProperty valueItem = values.GetArrayElementAtIndex(values.arraySize - 1);
                        SerializedProperty valueName = valueItem.FindPropertyRelative("Name");
                        valueName.stringValue = states[j].Name;

                        if (isNew)
                        { 
                            SerializedProperty color = valueItem.FindPropertyRelative("Color");
                            color.colorValue = Color.white;
                        }
                    }
                    
                    List<ShaderPropertyType> shaderPropFilter = new List<ShaderPropertyType>();
                    // do we need a propId?
                    if (properties[i].Type == ThemePropertyValueTypes.Color)
                    {
                        if (!hasText && hasRenderer)
                        {
                            shaderPropFilter.Add(ShaderPropertyType.Color);
                        }
                        else if(!hasText && !hasRenderer)
                        {
                            valid = false;
                        }
                    }

                    if (properties[i].Type == ThemePropertyValueTypes.ShaderFloat || properties[i].Type == ThemePropertyValueTypes.shaderRange)
                    {
                        if (hasRenderer)
                        {
                            shaderPropFilter.Add(ShaderPropertyType.Float);
                            shaderPropFilter.Add(ShaderPropertyType.Range);
                        }
                        else
                        {
                            valid = false;
                        }
                    }

                    SerializedProperty propId = item.FindPropertyRelative("PropId");
                    propId.intValue = 0;

                    SerializedProperty shaderList = item.FindPropertyRelative("ShaderOptions");
                    SerializedProperty shaderNames = item.FindPropertyRelative("ShaderOptionNames");

                    shaderList.ClearArray();
                    shaderNames.ClearArray();

                    if (valid && shaderPropFilter.Count > 0)
                    {
                        ShaderProperties[]  shaderProps = GetShaderProperties(gameObject.gameObject.GetComponent<Renderer>(), shaderPropFilter.ToArray());
                        for (int n = 0; n < shaderProps.Length; n++)
                        {
                            shaderList.InsertArrayElementAtIndex(shaderList.arraySize);
                            SerializedProperty shaderListItem = shaderList.GetArrayElementAtIndex(shaderList.arraySize - 1);
                            SerializedProperty shaderListName = shaderListItem.FindPropertyRelative("Name");
                            SerializedProperty shaderListType = shaderListItem.FindPropertyRelative("Type");
                            SerializedProperty shaderListRange = shaderListItem.FindPropertyRelative("Range");

                            shaderListName.stringValue = shaderProps[n].Name;
                            shaderListType.intValue = (int)shaderProps[n].Type;
                            shaderListRange.vector2Value = shaderProps[n].Range;
                            
                            shaderNames.InsertArrayElementAtIndex(shaderNames.arraySize);
                            SerializedProperty names = shaderNames.GetArrayElementAtIndex(shaderNames.arraySize - 1);
                            names.stringValue = shaderProps[n].Name;
                        }

                    }
                }

                if (!valid)
                {
                    isValid.boolValue = false;
                }
            }

            return themeObj;
            
        }

        private static void RenderThemeSettings(SerializedProperty settings, State[] states, int margin)
        {
            
            GUIStyle box = new GUIStyle(GUI.skin.box);
            box.margin.left = margin;
            EditorGUILayout.BeginVertical(box);
            for (int n = 0; n < states.Length; n++)
            {

                DrawLabel(states[n].Name, 12, titleColor);

                EditorGUI.indentLevel = indentOnSectionStart + 1;

                for (int j = 0; j < settings.arraySize; j++)
                {
                    SerializedProperty settingsItem = settings.GetArrayElementAtIndex(j);
                    SerializedProperty className = settingsItem.FindPropertyRelative("Name");
                    
                    SerializedProperty properties = settingsItem.FindPropertyRelative("Properties");

                    for (int i = 0; i < properties.arraySize; i++)
                    {
                        SerializedProperty propertyItem = properties.GetArrayElementAtIndex(i);

                        SerializedProperty name = propertyItem.FindPropertyRelative("Name");
                        SerializedProperty type = propertyItem.FindPropertyRelative("Type");
                        SerializedProperty values = propertyItem.FindPropertyRelative("Values");
                        SerializedProperty propId = propertyItem.FindPropertyRelative("PropId");
                        SerializedProperty options = propertyItem.FindPropertyRelative("ShaderOptions");
                        SerializedProperty names = propertyItem.FindPropertyRelative("ShaderOptionNames");

                        if (n >= values.arraySize)
                        {
                            continue;
                        }

                        SerializedProperty item = values.GetArrayElementAtIndex(n);
                        SerializedProperty floatValue = item.FindPropertyRelative("Float");
                        SerializedProperty vector2Value = item.FindPropertyRelative("Vector2");

                        switch ((ThemePropertyValueTypes)type.intValue)
                        {
                            case ThemePropertyValueTypes.Float:
                                floatValue.floatValue = EditorGUILayout.FloatField(new GUIContent(name.stringValue, ""), floatValue.floatValue);
                                break;
                            case ThemePropertyValueTypes.Int:
                                SerializedProperty intValue = item.FindPropertyRelative("Int");
                                intValue.intValue = EditorGUILayout.IntField(new GUIContent(name.stringValue, ""), intValue.intValue);
                                break;
                            case ThemePropertyValueTypes.Color:
                                SerializedProperty colorValue = item.FindPropertyRelative("Color");
                                colorValue.colorValue = EditorGUILayout.ColorField(new GUIContent(name.stringValue, ""), colorValue.colorValue);
                                break;
                            case ThemePropertyValueTypes.ShaderFloat:
                                floatValue.floatValue = EditorGUILayout.FloatField(new GUIContent(name.stringValue, ""), floatValue.floatValue);
                                break;
                            case ThemePropertyValueTypes.shaderRange:
                                vector2Value.vector2Value = EditorGUILayout.Vector2Field(new GUIContent(name.stringValue, ""), vector2Value.vector2Value);
                                break;
                            case ThemePropertyValueTypes.Vector2:
                                vector2Value.vector2Value = EditorGUILayout.Vector2Field(new GUIContent(name.stringValue, ""), vector2Value.vector2Value);
                                break;
                            case ThemePropertyValueTypes.Vector3:
                                SerializedProperty vector3Value = item.FindPropertyRelative("Vector3");
                                vector3Value.vector3Value = EditorGUILayout.Vector3Field(new GUIContent(name.stringValue, ""), vector3Value.vector3Value);
                                break;
                            case ThemePropertyValueTypes.Vector4:
                                SerializedProperty vector4Value = item.FindPropertyRelative("Vector4");
                                vector4Value.vector4Value = EditorGUILayout.Vector4Field(new GUIContent(name.stringValue, ""), vector4Value.vector4Value);
                                break;
                            case ThemePropertyValueTypes.Quaternion:
                                SerializedProperty quaternionValue = item.FindPropertyRelative("Quaternion");
                                Vector4 vect4 = new Vector4(quaternionValue.quaternionValue.x, quaternionValue.quaternionValue.y, quaternionValue.quaternionValue.z, quaternionValue.quaternionValue.w);
                                vect4 = EditorGUILayout.Vector4Field(new GUIContent(name.stringValue, ""), vect4);
                                quaternionValue.quaternionValue = new Quaternion(vect4.x, vect4.y, vect4.z, vect4.w);
                                break;
                            case ThemePropertyValueTypes.Texture:
                                SerializedProperty texture = item.FindPropertyRelative("Texture");
                                EditorGUILayout.PropertyField(texture, new GUIContent(name.stringValue, ""), false);
                                break;
                            case ThemePropertyValueTypes.Material:
                                SerializedProperty material = item.FindPropertyRelative("Material");
                                EditorGUILayout.PropertyField(material, new GUIContent(name.stringValue, ""), false);
                                break;
                            case ThemePropertyValueTypes.AudioClip:
                                SerializedProperty audio = item.FindPropertyRelative("AudioClip");
                                EditorGUILayout.PropertyField(audio, new GUIContent(name.stringValue, ""), false);
                                break;
                            case ThemePropertyValueTypes.Animaiton:
                                SerializedProperty animation = item.FindPropertyRelative("Animation");
                                EditorGUILayout.PropertyField(animation, new GUIContent(name.stringValue, ""), false);
                                break;
                            case ThemePropertyValueTypes.GameObject:
                                SerializedProperty gameObjectValue = item.FindPropertyRelative("Int");
                                EditorGUILayout.PropertyField(gameObjectValue, new GUIContent(name.stringValue, ""), false);
                                break;
                            case ThemePropertyValueTypes.String:
                                SerializedProperty stringValue = item.FindPropertyRelative("String");
                                stringValue.stringValue = EditorGUILayout.TextField(new GUIContent(name.stringValue, ""), stringValue.stringValue);
                                break;
                            case ThemePropertyValueTypes.Bool:
                                SerializedProperty boolValue = item.FindPropertyRelative("Bool");
                                boolValue.boolValue = EditorGUILayout.Toggle(new GUIContent(name.stringValue, ""), boolValue.boolValue);
                                break;
                            default:
                                break;
                        }
                    }
                }

                EditorGUI.indentLevel = indentOnSectionStart;
            }

            GUILayout.Space(5);

            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
        }

        private void AddEvent(int index)
        {
            SerializedProperty events = serializedObject.FindProperty("Events");
            events.InsertArrayElementAtIndex(events.arraySize);
            SerializedProperty eventItem = events.GetArrayElementAtIndex(events.arraySize - 1);
            
        }

        private void ChangeEvent(int index)
        {
            SerializedProperty events = serializedObject.FindProperty("Events");
            SerializedProperty eventItem = events.GetArrayElementAtIndex(index);
            SerializedProperty className = eventItem.FindPropertyRelative("ClassName");
            SerializedProperty name = eventItem.FindPropertyRelative("Name");
            SerializedProperty settings = eventItem.FindPropertyRelative("Settings");
            

            if (!String.IsNullOrEmpty(className.stringValue))
            {
                int receiverIndex = ReverseLookup(className.stringValue, eventOptions);
                InteractableEvent.ReceiverData data = eventList[index].AddReceiver(eventTypes[receiverIndex]);
                name.stringValue = data.Name;
                PropertySettingsList(settings, data.Fields);
            }
        }

        private static void PropertySettingsList(SerializedProperty settings, List<InteractableEvent.FieldData> data)
        {
            settings.ClearArray();

            for (int i = 0; i < data.Count; i++)
            {
                settings.InsertArrayElementAtIndex(settings.arraySize);
                SerializedProperty settingItem = settings.GetArrayElementAtIndex(settings.arraySize - 1);

                UpdatePropertySettings(settingItem, (int)data[i].Attributes.Type, data[i].Value);

                SerializedProperty type = settingItem.FindPropertyRelative("Type");
                SerializedProperty tooltip = settingItem.FindPropertyRelative("Tooltip");
                SerializedProperty label = settingItem.FindPropertyRelative("Label");
                SerializedProperty options = settingItem.FindPropertyRelative("Options");

                type.enumValueIndex = (int)data[i].Attributes.Type;
                tooltip.stringValue = data[i].Attributes.Tooltip;
                label.stringValue = data[i].Attributes.Label;
                options.ClearArray();

                if (data[i].Attributes.Options != null)
                {
                    for (int j = 0; j < data[i].Attributes.Options.Length; j++)
                    {
                        options.InsertArrayElementAtIndex(j);
                        SerializedProperty item = options.GetArrayElementAtIndex(j);
                        item.stringValue = data[i].Attributes.Options[j];
                    }
                }
            }
        }

        private static void UpdatePropertySettings(SerializedProperty prop, int type, object update)
        {
            SerializedProperty intValue = prop.FindPropertyRelative("IntValue");
            SerializedProperty stringValue = prop.FindPropertyRelative("StringValue");

            switch ((InspectorField.FieldTypes)type)
            {
                case InspectorField.FieldTypes.Float:
                    SerializedProperty floatValue = prop.FindPropertyRelative("FloatValue");
                    floatValue.floatValue = (float)update;
                    break;
                case InspectorField.FieldTypes.Int:
                    intValue.intValue = (int)update;
                    break;
                case InspectorField.FieldTypes.String:
                    
                    stringValue.stringValue = (string)update;
                    break;
                case InspectorField.FieldTypes.Bool:
                    SerializedProperty boolValue = prop.FindPropertyRelative("BoolValue");
                    boolValue.boolValue = (bool)update;
                    break;
                case InspectorField.FieldTypes.Color:
                    SerializedProperty colorValue = prop.FindPropertyRelative("ColorValue");
                    colorValue.colorValue = (Color)update;
                    break;
                case InspectorField.FieldTypes.DropdownInt:
                    intValue.intValue = (int)update;
                    break;
                case InspectorField.FieldTypes.DropdownString:
                    stringValue.stringValue = (string)update;
                    break;
                case InspectorField.FieldTypes.GameObject:
                    SerializedProperty gameObjectValue = prop.FindPropertyRelative("GameObjectValue");
                    gameObjectValue.objectReferenceValue = (GameObject)update;
                    break;
                case InspectorField.FieldTypes.ScriptableObject:
                    SerializedProperty scriptableObjectValue = prop.FindPropertyRelative("ScriptableObjectValue");
                    scriptableObjectValue.objectReferenceValue = (ScriptableObject)update;
                    break;
                case InspectorField.FieldTypes.Object:
                    SerializedProperty objectValue = prop.FindPropertyRelative("ObjectValue");
                    objectValue.objectReferenceValue = (UnityEngine.Object)update;
                    break;
                case InspectorField.FieldTypes.Material:
                    SerializedProperty materialValue = prop.FindPropertyRelative("MaterialValue");
                    materialValue.objectReferenceValue = (Material)update;
                    break;
                case InspectorField.FieldTypes.Texture:
                    SerializedProperty textureValue = prop.FindPropertyRelative("TextureValue");
                    textureValue.objectReferenceValue = (Texture)update;
                    break;
                case InspectorField.FieldTypes.Vector2:
                    SerializedProperty vector2Value = prop.FindPropertyRelative("Vector2Value");
                    vector2Value.vector2Value = (Vector2)update;
                    break;
                case InspectorField.FieldTypes.Vector3:
                    SerializedProperty vector3Value = prop.FindPropertyRelative("Vector3Value");
                    vector3Value.vector3Value = (Vector3)update;
                    break;
                case InspectorField.FieldTypes.Vector4:
                    SerializedProperty vector4Value = prop.FindPropertyRelative("Vector4Value");
                    vector4Value.vector4Value = (Vector4)update;
                    break;
                case InspectorField.FieldTypes.Curve:
                    SerializedProperty curveValue = prop.FindPropertyRelative("CurveValue");
                    curveValue.animationCurveValue = (AnimationCurve)update;
                    break;
                case InspectorField.FieldTypes.Quaternion:
                    SerializedProperty quaternionValue = prop.FindPropertyRelative("QuaternionValue");
                    quaternionValue.quaternionValue = (Quaternion)update;
                    break;
                case InspectorField.FieldTypes.AudioClip:
                    SerializedProperty audioClip = prop.FindPropertyRelative("AudioClipValue");
                    audioClip.objectReferenceValue = (AudioClip)update;
                    break;
                default:
                    break;
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

        private static void DisplayPropertyField(SerializedProperty prop)
        {
            SerializedProperty type = prop.FindPropertyRelative("Type");
            SerializedProperty label = prop.FindPropertyRelative("Label");
            SerializedProperty tooltip = prop.FindPropertyRelative("Tooltip");
            SerializedProperty options = prop.FindPropertyRelative("Options");

            SerializedProperty intValue = prop.FindPropertyRelative("IntValue");
            SerializedProperty stringValue = prop.FindPropertyRelative("StringValue");

            switch ((InspectorField.FieldTypes)type.intValue)
            {
                case InspectorField.FieldTypes.Float:
                    SerializedProperty floatValue = prop.FindPropertyRelative("FloatValue");
                    floatValue.floatValue = EditorGUILayout.FloatField(new GUIContent(label.stringValue, tooltip.stringValue), floatValue.floatValue);
                    break;
                case InspectorField.FieldTypes.Int:
                    intValue.intValue = EditorGUILayout.IntField(new GUIContent(label.stringValue, tooltip.stringValue), intValue.intValue);
                    break;
                case InspectorField.FieldTypes.String:
                    stringValue.stringValue = EditorGUILayout.TextField(new GUIContent(label.stringValue, tooltip.stringValue), stringValue.stringValue);
                    break;
                case InspectorField.FieldTypes.Bool:
                    SerializedProperty boolValue = prop.FindPropertyRelative("BoolValue");
                    boolValue.boolValue = EditorGUILayout.Toggle(new GUIContent(label.stringValue, tooltip.stringValue), boolValue.boolValue);
                    break;
                case InspectorField.FieldTypes.Color:
                    SerializedProperty colorValue = prop.FindPropertyRelative("ColorValue");
                    colorValue.colorValue = EditorGUILayout.ColorField(new GUIContent(label.stringValue, tooltip.stringValue), colorValue.colorValue);
                    break;
                case InspectorField.FieldTypes.DropdownInt:
                    intValue.intValue = EditorGUILayout.Popup(label.stringValue, intValue.intValue, GetOptions(options));
                    break;
                case InspectorField.FieldTypes.DropdownString:
                    string[] stringOptions = GetOptions(options);
                    int selection = GetOptionsIndex(options, stringValue.stringValue);
                    int newIndex = EditorGUILayout.Popup(label.stringValue, intValue.intValue, stringOptions);
                    if (selection != newIndex)
                    {
                        stringValue.stringValue = stringOptions[newIndex];
                    }
                    break;
                case InspectorField.FieldTypes.GameObject:
                    SerializedProperty gameObjectValue = prop.FindPropertyRelative("GameObjectValue");
                    EditorGUILayout.PropertyField(gameObjectValue, new GUIContent(label.stringValue, tooltip.stringValue), false);
                    break;
                case InspectorField.FieldTypes.ScriptableObject:
                    SerializedProperty scriptableObjectValue = prop.FindPropertyRelative("ScriptableObjectValue");
                    EditorGUILayout.PropertyField(scriptableObjectValue, new GUIContent(label.stringValue, tooltip.stringValue), false);
                    break;
                case InspectorField.FieldTypes.Object:
                    SerializedProperty objectValue = prop.FindPropertyRelative("ObjectValue");
                    EditorGUILayout.PropertyField(objectValue, new GUIContent(label.stringValue, tooltip.stringValue), true);
                    break;
                case InspectorField.FieldTypes.Material:
                    SerializedProperty materialValue = prop.FindPropertyRelative("MaterialValue");
                    EditorGUILayout.PropertyField(materialValue, new GUIContent(label.stringValue, tooltip.stringValue), false);
                    break;
                case InspectorField.FieldTypes.Texture:
                    SerializedProperty textureValue = prop.FindPropertyRelative("TextureValue");
                    EditorGUILayout.PropertyField(textureValue, new GUIContent(label.stringValue, tooltip.stringValue), false);
                    break;
                case InspectorField.FieldTypes.Vector2:
                    SerializedProperty vector2Value = prop.FindPropertyRelative("Vector2Value");
                    vector2Value.vector2Value = EditorGUILayout.Vector2Field(new GUIContent(label.stringValue, tooltip.stringValue), vector2Value.vector2Value);
                    break;
                case InspectorField.FieldTypes.Vector3:
                    SerializedProperty vector3Value = prop.FindPropertyRelative("Vector3Value");
                    vector3Value.vector3Value = EditorGUILayout.Vector3Field(new GUIContent(label.stringValue, tooltip.stringValue), vector3Value.vector3Value);
                    break;
                case InspectorField.FieldTypes.Vector4:
                    SerializedProperty vector4Value = prop.FindPropertyRelative("Vector4Value");
                    vector4Value.vector4Value = EditorGUILayout.Vector4Field(new GUIContent(label.stringValue, tooltip.stringValue), vector4Value.vector4Value);
                    break;
                case InspectorField.FieldTypes.Curve:
                    SerializedProperty curveValue = prop.FindPropertyRelative("CurveValue");
                    curveValue.animationCurveValue = EditorGUILayout.CurveField(new GUIContent(label.stringValue, tooltip.stringValue), curveValue.animationCurveValue);
                    break;
                case InspectorField.FieldTypes.Quaternion:
                    SerializedProperty quaternionValue = prop.FindPropertyRelative("QuaternionValue");
                    Vector4 vect4 = new Vector4(quaternionValue.quaternionValue.x, quaternionValue.quaternionValue.y, quaternionValue.quaternionValue.z, quaternionValue.quaternionValue.w);
                    vect4 = EditorGUILayout.Vector4Field(new GUIContent(label.stringValue, tooltip.stringValue), vect4);
                    quaternionValue.quaternionValue = new Quaternion(vect4.x, vect4.y, vect4.z, vect4.w);
                    break;
                case InspectorField.FieldTypes.AudioClip:
                    SerializedProperty audioClip = prop.FindPropertyRelative("AudioClipValue");
                    EditorGUILayout.PropertyField(audioClip, new GUIContent(label.stringValue, tooltip.stringValue), false);
                    break;
                default:
                    break;
            }
        }

        private void RemoveEvent(int index)
        {
            SerializedProperty events = serializedObject.FindProperty("Events");
            if (events.arraySize > index)
            {
                events.DeleteArrayElementAtIndex(index);
            }
        }

        private void SetupEventOptions()
        {
            InteractableEvent.EventLists lists = InteractableEvent.GetEventTypes();
            eventTypes = lists.EventTypes.ToArray();
            eventOptions = lists.EventNames.ToArray();
        }

        private void SetupThemeOptions()
        {
            ProfileItem.ThemeLists lists = ProfileItem.GetThemeTypes();
            themeOptions = lists.Names.ToArray();
            themeTypes = lists.Types.ToArray();
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

        public static ShaderProperties[] GetShaderProperties(Renderer renderer, ShaderPropertyType[] filter)
        {
            List<ShaderProperties> properties = new List<ShaderProperties>();
            if (renderer != null)
            {
                Material material = ThemeBase.GetValidMaterial(renderer);

                if (material != null)
                {
                    int count = ShaderUtil.GetPropertyCount(material.shader);

                    for (int i = 0; i < count; i++)
                    {
                        string name = ShaderUtil.GetPropertyName(material.shader, i);
                        ShaderPropertyType type = ShaderUtilConvert(ShaderUtil.GetPropertyType(material.shader, i));
                        bool isHidden = ShaderUtil.IsShaderPropertyHidden(material.shader, i);
                        Vector2 range = new Vector2(ShaderUtil.GetRangeLimits(material.shader, i, 1), ShaderUtil.GetRangeLimits(material.shader, i, 2));

                        if (!isHidden && HasShaderPropertyType(filter, type))
                        {
                            properties.Add(new ShaderProperties() { Name = name, Type = type, Range = range });
                        }
                    }
                }
            }
            return properties.ToArray();
        }

        public static ShaderPropertyType ShaderUtilConvert(ShaderUtil.ShaderPropertyType type)
        {
            ShaderPropertyType shaderType;
            switch (type)
            {
                case ShaderUtil.ShaderPropertyType.Color:
                    shaderType = ShaderPropertyType.Color;
                    break;
                case ShaderUtil.ShaderPropertyType.Vector:
                    shaderType = ShaderPropertyType.Vector;
                    break;
                case ShaderUtil.ShaderPropertyType.Float:
                    shaderType = ShaderPropertyType.Float;
                    break;
                case ShaderUtil.ShaderPropertyType.Range:
                    shaderType = ShaderPropertyType.Range;
                    break;
                case ShaderUtil.ShaderPropertyType.TexEnv:
                    shaderType = ShaderPropertyType.TexEnv;
                    break;
                default:
                    shaderType = ShaderPropertyType.None;
                    break;
            }
            return shaderType;
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

        public static bool HasShaderPropertyType(ShaderPropertyType[] filter, ShaderPropertyType type)
        {
            for (int i = 0; i < filter.Length; i++)
            {
                if (filter[i] == type)
                {
                    return true;
                }
            }

            return false;
        }

        
    }
}
