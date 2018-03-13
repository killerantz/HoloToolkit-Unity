using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace HoloToolkit.Unity
{
    [System.Serializable]

    public class Interactable : MonoBehaviour
    {
        public bool Enabled;
        public InteractableStates State;
        public InteractionSourcePressInfo ButtonPressFilter = InteractionSourcePressInfo.Select;
        public bool IsGlobal = false;
        public int Dimensions = 1;
        public string VoiceCommand = "";
        public bool RequiresGaze = true;
        public List<ProfileItem> Profiles = new List<ProfileItem>();
        public List<InteractableEvent> Events = new List<InteractableEvent>();

        private State lastState;

        private void Awake()
        {
            State = new InteractableStates();
        }

        private void SetupEvents()
        {
            InteractableEvent.EventLists lists = InteractableEvent.GetEventTypes();
            
            for (int i = 0; i < Events.Count; i++)
            {
                Events[i].Receiver = InteractableEvent.GetReceiver(Events[i], lists);
            }
        }

        //collider checks and other alerts

        // state management

        private void Update()
        {
            for (int i = 0; i < Events.Count; i++)
            {
                if (Events[i].Receiver != null)
                {
                    print(i + " / " + Events[i].Receiver);
                    Events[i].Receiver.OnUpdate(State.GetCurrent());
                    ReceiverBase reciever = Events[i].Receiver;

                    /*
                    Type myType = reciever.GetType();
                    foreach (PropertyInfo prop in myType.GetProperties())
                    {
                        var attrs = (InspectorField[])prop.GetCustomAttributes(typeof(InspectorField), false);
                        foreach (var attr in attrs)
                        {
                            Debug.Log("Props: " + prop.Name + " / " + attr.Type + " / " + attr.Label);
                        }
                    }

                    foreach (FieldInfo field in myType.GetFields())
                    {
                        var attrs = (InspectorField[])field.GetCustomAttributes(typeof(InspectorField), false);
                        foreach (var attr in attrs)
                        {
                            Debug.Log("Fields: " + field.Name + " / " + attr.Type + " / " + attr.Label);
                        }
                    }
                    */
                }


            }

            if (lastState != State.GetCurrent())
            {
               
            }

            lastState = State.GetCurrent();
        }

    }
}
