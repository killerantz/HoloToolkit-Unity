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
            base.OnInspectorGUI();

            // extend the preference array to handle multiple themes open and scroll values!!!
            // add  messaging!!!
            // handle dimensions
            // add profiles
            // add themes
            // handle/display properties from themes

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

                        UpdateEvent(i);
                    }

                    EditorGUILayout.Space();
                    RemoveButton("Remove Event", i, RemoveEvent);
                }

                // show event properties


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

        private void UpdateEvent(int index)
        {
            SerializedProperty events = serializedObject.FindProperty("Events");
            SerializedProperty eventItem = events.GetArrayElementAtIndex(index);
            SerializedProperty className = eventItem.FindPropertyRelative("ClassName");
            SerializedProperty name = eventItem.FindPropertyRelative("Name");

            if (index == 0)
            {
                name.stringValue = eventList[index].AddOnClick();
            }
            else
            {
                if (!String.IsNullOrEmpty(className.stringValue))
                {
                    int receiverIndex = InteractableEvent.ReverseLookupEvents(className.stringValue, eventOptions);
                    name.stringValue = eventList[index].AddReceiver(eventTypes[receiverIndex]);
                }
            }

            //Debug.Log("Update BBBBBBBBBBBBBB: " + className.stringValue + " / " + name.stringValue);
        }

        private void RemoveEvent(int index)
        {

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
