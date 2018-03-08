using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace Buttons
{
    [System.Serializable]
    public struct ButtonSettings
    {
        public GameObject Target;
        public float Time;
        public ObjectProfile Profile;
        public AnimationCurve Curve;
        public List<ButtonThemes> Themes;
    }

    public struct ButtonEvents
    {
        public UnityEvent Event;
        public ButtonReceiver Receiver;
    }

    public class ButtonObject : MonoBehaviour, IInputClickHandler, IFocusable, IInputHandler
    {
        public List<ButtonSettings> Settings;
        public List<ButtonEvents> Events;

        public StatesBase States = new InteractiveStates();

        public State ButtonState;
        private State lastState;

        [HideInInspector]
        public UnityEvent MainEvent;

        public bool HasFocus { get; private set; }
        public bool HasPress { get; private set; }
        public bool IsDisabled { get; private set; }
        
        private void Awake()
        {
            if (Settings == null)
            {
                Settings = new List<ButtonSettings>();
            }

            if(Events == null)
            {
                Events = new List<ButtonEvents>();
            }

            ButtonEvents events = new ButtonEvents();
            events.Event = new UnityEvent();
            events.Receiver = new ButtonReceiver(events.Event);
            Events.Add(events);

            print(States.GetState(new bool[] { true, false, false }).ToString());
            print(States.GetStates().Length);
            UpdateState();
        }

        public void SetFocus (bool focus)
        {
            HasFocus = focus;
            UpdateState();
        }

        public void SetPress (bool press)
        {
            HasPress = press;
            UpdateState();
        }

        public void SetDisabled (bool disabled)
        {
            IsDisabled = disabled;
            UpdateState();
        }

        public void OnFocusEnter()
        {
            SetFocus(true);
        }

        public void OnFocusExit()
        {
            SetFocus(false);
        }

        public void OnInputClicked(InputClickedEventData eventData)
        {
            if(ButtonState != InteractiveStates.Disabled)
            {
                print("clicked 01");
            }
        }

        public void OnInputDown(InputEventData eventData)
        {
            SetPress(true);
        }

        public void OnInputUp(InputEventData eventData)
        {
            SetPress(false);
        }

        protected virtual void UpdateState()
        {
            ButtonState = States.GetState(new bool[] { HasFocus, HasPress, IsDisabled });
        }

        private void Update()
        {
            if (lastState != ButtonState)
            { 
                for (int i = 0; i < Events.Count; i++)
                {
                    Events[i].Receiver.OnUpdate(ButtonState);

                    ButtonReceiver reciever = Events[i].Receiver;

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


                }
            }

            lastState = ButtonState;
        }

    }
}
