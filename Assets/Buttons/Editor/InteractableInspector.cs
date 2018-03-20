using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

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
        private SerializedProperty dataList;
        private static bool showProfiles;
        private string prefKey = "InteractableInspectorProfiles";
        private List<ListSettings> listSettings;
        private List<EventSettings> eventSettings;
        private bool enabled = false;

        private string[] eventOptions;
        private Type[] eventTypes;

        private void OnEnable()
        {
            instance = (Interactable)target;
            eventList = instance.Events;

            listSettings = new List<ListSettings>();

            dataList = serializedObject.FindProperty("Profiles");
            AdjustListSettings(dataList.arraySize);
            showProfiles = EditorPrefs.GetBool(prefKey, showProfiles);

            SetupEventOptions();

            enabled = true;
            Debug.Log("Enabled");
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
            // create a version that uses the real click event from InputManager.

            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(new GUIContent("Interactable Settings"));

            EditorGUILayout.BeginVertical("Box");

            //standard UI
            SerializedProperty enabled = serializedObject.FindProperty("Enabled");
            enabled.boolValue = EditorGUILayout.Toggle(new GUIContent("Enabled", "Is this Interactable Enabled?"), enabled.boolValue);

            SerializedProperty selected = serializedObject.FindProperty("ButtonPressFilter");
            selected.enumValueIndex = (int)(InteractionSourcePressInfo)EditorGUILayout.EnumPopup(new GUIContent("Interact Filter", "Input source for this Interactable, Default: Select"), (InteractionSourcePressInfo)selected.enumValueIndex);

            SerializedProperty isGlobal = serializedObject.FindProperty("IsGlobal");
            isGlobal.boolValue = EditorGUILayout.Toggle(new GUIContent("Is Global", "Like a modal, does not require focus"), isGlobal.boolValue);

            SerializedProperty voiceCommands = serializedObject.FindProperty("VoiceCommand");
            voiceCommands.stringValue = EditorGUILayout.TextField(new GUIContent("Voice Command", "A voice command to trigger the click event"), voiceCommands.stringValue);

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

            // profiles section
            bool isOPen = DrawSectionStart("Profiles", indentOnSectionStart + 1, showProfiles);

            if (showProfiles != isOPen)
            {
                showProfiles = isOPen;
                EditorPrefs.SetBool(prefKey, showProfiles);
            }

            if (dataList.arraySize < 1)
            {
                AddItem(0);
            }

            if (showProfiles)
            {
                for (int i = 0; i < dataList.arraySize; i++)
                {
                    EditorGUILayout.BeginVertical("Box");
                    // get profiles
                    SerializedProperty sItem = dataList.GetArrayElementAtIndex(i);

                    SerializedProperty gameObject = sItem.FindPropertyRelative("Target");
                    EditorGUILayout.PropertyField(gameObject, new GUIContent("Target", "Target gameObject for this theme properties to manipulate"));

                    // get themes
                    SerializedProperty themes = sItem.FindPropertyRelative("Themes");
                    if (themes.arraySize < 1)
                    {
                        themes.InsertArrayElementAtIndex(themes.arraySize);
                        SerializedProperty theme = themes.GetArrayElementAtIndex(themes.arraySize - 1);

                        // make sure there is only one or make unique
                        string[] themeLocations = AssetDatabase.FindAssets("n:DefaultTheme t:Theme");
                        if (themeLocations.Length > 0)
                        {
                            Theme defaultTheme = (Theme)AssetDatabase.LoadAssetAtPath(themeLocations[0], typeof(Theme));
                            theme.objectReferenceValue = defaultTheme;
                        }
                    }

                    for (int t = 0; t < themes.arraySize; t++)
                    {
                        SerializedProperty themeItem = themes.GetArrayElementAtIndex(themes.arraySize - 1);
                        EditorGUILayout.PropertyField(themeItem, new GUIContent("Theme", "Theme properties for interation feedback"));

                        EditorGUI.indentLevel = indentOnSectionStart + 2;

                        ListSettings settings = listSettings[i];
                        settings.Show = DrawSectionStart("Edit", indentOnSectionStart + 3, listSettings[i].Show, FontStyle.Normal, false);

                        if (settings.Show)
                        {
                            EditorGUILayout.BeginVertical("Box");

                            EditorGUILayout.Space();
                            EditorGUILayout.Space();
                            GUILayout.Space(20);

                            // show theme properties

                            // edit theme properties

                            EditorGUILayout.EndVertical();
                        }

                        DrawSectionEnd(indentOnSectionStart + 2);
                        listSettings[i] = settings;
                    }

                    if (i > 0)
                    {
                        RemoveButton("Remove Profile", i, RemoveProfile);
                    }

                    EditorGUILayout.EndVertical();
                }

                if (GUILayout.Button(new GUIContent("Add Profile")))
                {
                    AddProfile();
                }
            }

            DrawSectionEnd(indentOnSectionStart);
            EditorGUILayout.Space();

            // Events section
            EditorGUILayout.LabelField(new GUIContent("Events"));
            SerializedProperty events = serializedObject.FindProperty("Events");

            if (events.arraySize < 1)
            {
                // add default event
                AddEvent(events.arraySize);
            }

            for (int i = 0; i < events.arraySize; i++)
            {
                EditorGUILayout.BeginVertical("Box");
                SerializedProperty eventItem = events.GetArrayElementAtIndex(i);
                SerializedProperty uEvent = eventItem.FindPropertyRelative("Event");
                SerializedProperty eventName = eventItem.FindPropertyRelative("Name");
                SerializedProperty className = eventItem.FindPropertyRelative("ClassName");
                EditorGUILayout.PropertyField(uEvent, new GUIContent(eventName.stringValue));

                if (i > 0)
                {
                    // show event dropdown
                    int id = InteractableEvent.ReverseLookupEvents(className.stringValue, eventOptions);
                    int newId = EditorGUILayout.Popup("Select Event Type", id, eventOptions);

                    if(id != newId)
                    {
                        className.stringValue = eventOptions[newId];

                        Debug.Log(eventOptions[newId] + " / " + className.stringValue);

                        ChangeEvent(i);
                    }
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

                if (i > 0)
                {
                    RemoveButton("Remove Event", i, RemoveEvent);
                }

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

        private void AddItem(int index)
        {
            dataList.InsertArrayElementAtIndex(dataList.arraySize);
            SerializedProperty newItem = dataList.GetArrayElementAtIndex(dataList.arraySize - 1);

            listSettings.Add(new ListSettings() { Show = false, Scroll = new Vector2() });
        }

        private void AddTheme(int index)
        {
            dataList.InsertArrayElementAtIndex(dataList.arraySize);
            SerializedProperty newItem = dataList.GetArrayElementAtIndex(dataList.arraySize - 1);

            listSettings.Add(new ListSettings() { Show = false, Scroll = new Vector2() });
        }

        private void AddProfile()
        {

        }

        private void AddEvent(int index)
        {
            SerializedProperty events = serializedObject.FindProperty("Events");
            events.InsertArrayElementAtIndex(events.arraySize);
            SerializedProperty eventItem = events.GetArrayElementAtIndex(events.arraySize - 1);

            if (index == 0)
            {
                InteractableEvent iEvent = new InteractableEvent();
                iEvent.Event = new UnityEvent();
                iEvent.Receiver = new OnClickReceiver(iEvent.Event);
                iEvent.Name = iEvent.Receiver.Name;
                eventList[events.arraySize - 1] = iEvent;
                

                Debug.Log(iEvent.Receiver);
            }
        }

        private void ChangeEvent(int index)
        {
            SerializedProperty events = serializedObject.FindProperty("Events");
            SerializedProperty eventItem = events.GetArrayElementAtIndex(index);
            SerializedProperty className = eventItem.FindPropertyRelative("ClassName");
            SerializedProperty name = eventItem.FindPropertyRelative("Name");
            SerializedProperty settings = eventItem.FindPropertyRelative("Settings");

            if (index == 0)
            {
                InteractableEvent.ReceiverData data = eventList[index].AddOnClick();
                name.stringValue = data.Name;
                PropertySettingsList(settings, data.Fields);
            }
            else
            {
                if (!String.IsNullOrEmpty(className.stringValue))
                {
                    int receiverIndex = InteractableEvent.ReverseLookupEvents(className.stringValue, eventOptions);
                    InteractableEvent.ReceiverData data = eventList[index].AddReceiver(eventTypes[receiverIndex]);
                    name.stringValue = data.Name;
                    PropertySettingsList(settings, data.Fields);
                }
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
                    vector3Value.vector3Value = EditorGUILayout.Vector3Field(new GUIContent(label.stringValue, tooltip.stringValue), vector3Value.vector3Value); ;
                    break;
                case InspectorField.FieldTypes.Vector4:
                    SerializedProperty vector4Value = prop.FindPropertyRelative("Vector4Value");
                    vector4Value.vector4Value = EditorGUILayout.Vector4Field(new GUIContent(label.stringValue, tooltip.stringValue), vector4Value.vector4Value); ;
                    break;
                case InspectorField.FieldTypes.Curve:
                    SerializedProperty curveValue = prop.FindPropertyRelative("CurveValue");
                    curveValue.animationCurveValue = EditorGUILayout.CurveField(new GUIContent(label.stringValue, tooltip.stringValue), curveValue.animationCurveValue);
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

        private void RemoveProfile(int index)
        {

        }

        private void SetupEventOptions()
        {
            InteractableEvent.EventLists lists = InteractableEvent.GetEventTypes();
            eventTypes = lists.EventTypes.ToArray();
            eventOptions = lists.EventNames.ToArray();
        }
    }
}
