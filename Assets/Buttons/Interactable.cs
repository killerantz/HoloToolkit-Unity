using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace HoloToolkit.Unity
{
    [System.Serializable]

    public class Interactable : MonoBehaviour, IInputClickHandler, IFocusable, IInputHandler
    {
        public bool Enabled;
        public InteractableStates State;
        public InteractionSourcePressInfo ButtonPressFilter = InteractionSourcePressInfo.Select;
        public bool IsGlobal = false;
        public int Dimensions = 1;
        public string VoiceCommand = "";
        public bool RequiresGaze = true;
        public List<ProfileItem> Profiles = new List<ProfileItem>();
        public UnityEvent OnClick;
        public List<InteractableEvent> Events = new List<InteractableEvent>();

        public bool HasFocus { get; private set; }
        public bool HasPress { get; private set; }
        public bool IsDisabled { get; private set; }

        private State lastState;

        // these should get simplified and moved
        // create a ScriptableObject for managing states!!!!
        public int GetStateCount()
        {
            InteractableStates states = new InteractableStates();
            return states.GetStates().Length;
        }

        public State[] GetStates()
        {
            InteractableStates states = new InteractableStates();
            return states.GetStates();
        }

        protected virtual void Awake()
        {
            State = new InteractableStates();
            SetupEvents();
        }

        protected virtual void SetupEvents()
        {
            InteractableEvent.EventLists lists = InteractableEvent.GetEventTypes();
            
            for (int i = 0; i < Events.Count; i++)
            {
                Events[i].Receiver = InteractableEvent.GetReceiver(Events[i], lists);
                // apply settings
            }
        }

        protected virtual void SetupThemes()
        {

        }

        //collider checks and other alerts

        // state management
        public virtual void SetFocus(bool focus)
        {
            HasFocus = focus;
            UpdateState();
        }

        public virtual void SetPress(bool press)
        {
            HasPress = press;
            UpdateState();
        }

        public virtual void SetDisabled(bool disabled)
        {
            IsDisabled = disabled;
            UpdateState();
        }

        public virtual void OnFocusEnter()
        {
            SetFocus(true);
        }

        public virtual void OnFocusExit()
        {
            SetFocus(false);
        }

        public virtual void OnInputClicked(InputClickedEventData eventData)
        {
            if (State.GetState() != InteractableStates.Disabled && eventData.PressType == ButtonPressFilter)
            {
                OnClick.Invoke();
            }
        }

        public virtual void OnInputDown(InputEventData eventData)
        {
            SetPress(true);
        }

        public virtual void OnInputUp(InputEventData eventData)
        {
            SetPress(false);
        }

        protected virtual void UpdateState()
        {
            State.CompareStates(new bool[] { HasFocus, HasPress, IsDisabled });
        }

        protected virtual void Update()
        {
            for (int i = 0; i < Events.Count; i++)
            {
                if (Events[i].Receiver != null)
                {
                    //print(i + " / " + Events[i].Receiver);
                    Events[i].Receiver.OnUpdate(State.GetState());
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

            if (lastState != State.GetState())
            {
               
            }

            lastState = State.GetState();
        }

    }
}
