using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace HoloToolkit.Unity
{
    [System.Serializable]
    public class InteractableEvent
    {
        public string Name;
        public UnityEvent Event;
        public string ClassName;
        public ReceiverBase Receiver;
        public List<EventSetting> Settings;

        public struct EventLists
        {
            public List<Type> EventTypes;
            public List<String> EventNames;
        }

        [System.Serializable]
        public struct EventSetting
        {
            public InspectorField.FieldTypes Type;
            public string Label;
            public string Tooltip;
            public int IntValue;
            public string StringValue;
            public float FloatValue;
            public bool BoolValue;
            public GameObject GameObjectValue;
            public ScriptableObject ScriptableObjectValue;
            public UnityEngine.Object ObjectValue;
            public Material MaterialValue;
            public Texture TextureValue;
            public Color ColorValue;
            public Vector2 Vector2Value;
            public Vector3 Vector3Value;
            public Vector4 Vector4Value;
        }

        public struct ReceiverData
        {
            public string Name;
            public List<InspectorField> Fields;
        }
        
        public string AddOnClick()
        {
            OnClickReceiver receiver = new OnClickReceiver(Event);
            ClassName = "OnClickReceiver";
            return receiver.Name;
        }

        public string AddReceiver(Type type)
        {
            ReceiverBase receiver = (ReceiverBase)Activator.CreateInstance(type, Event);
            // get the settings for the inspector

            List<InspectorField> fields = new List<InspectorField>();

            Type myType = receiver.GetType();
            int index = 0;

            //Debug.Log(myType + " / " + myType.GetProperties().Length + " / " + myType.GetFields().Length);
            foreach (PropertyInfo prop in myType.GetProperties())
            {
                var attrs = (InspectorField[])prop.GetCustomAttributes(typeof(InspectorField), false);
                foreach (var attr in attrs)
                {
                    fields.Add(attr);
                    Debug.Log("Props: " + prop.Name + " / " + attr.Type + " / " + attr.Label + " / " + prop.GetValue(receiver, null ));
                }

                index++;
            }

            index = 0;
            foreach (FieldInfo field in myType.GetFields())
            {
                var attrs = (InspectorField[])field.GetCustomAttributes(typeof(InspectorField), false);
                foreach (var attr in attrs)
                {
                    fields.Add(attr);
                    Debug.Log("Fields: " + field.Name + " / " + attr.Type + " / " + attr.Label + " / " + field.GetValue(receiver));
                }
                index++;
            }

            return receiver.Name;
        }

        public static EventLists GetEventTypes()
        {
            List<Type> eventTypes = new List<Type>();
            List<string> names = new List<string>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsSubclassOf(typeof(ReceiverBase)))
                    {
                        eventTypes.Add(type);
                        names.Add(type.Name);
                    }
                }
            }

            EventLists lists = new EventLists();
            lists.EventTypes = eventTypes;
            lists.EventNames = names;
            return lists;
        }

        public static int ReverseLookupEvents(string name, string[] options)
        {
            for (int i = 0; i < options.Length; i++)
            {
                if (options[i] == name)
                {
                    return i;
                }
            }

            return 0;
        }

        public static ReceiverBase GetReceiver(InteractableEvent iEvent, EventLists lists)
        {
            int index = ReverseLookupEvents(iEvent.ClassName, lists.EventNames.ToArray());
            Type eventType = lists.EventTypes[index];
            // apply the settings?
            return (ReceiverBase)Activator.CreateInstance(eventType, iEvent.Event);
        }
    }

}
