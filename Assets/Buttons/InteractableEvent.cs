using System;
using System.Collections;
using System.Collections.Generic;
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
        public Type EventType;
        public ReceiverBase Receiver;

        public struct EventLists
        {
            public List<Type> EventTypes;
            public List<String> EventNames;
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
            return (ReceiverBase)Activator.CreateInstance(eventType, iEvent.Event);
        }
    }

}
