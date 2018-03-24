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
        
        public List<ThemeBase> runningThemesList = new List<ThemeBase>();

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
            SetupThemes();
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
            ProfileItem.ThemeLists lists = ProfileItem.GetThemeTypes();
            runningThemesList = new List<ThemeBase>();

            for (int i = 0; i < Profiles.Count; i++)
            {
                for (int j = 0; j < Profiles[i].Themes.Count; j++)
                {
                    Theme theme = Profiles[i].Themes[j];
                    for (int n = 0; n < theme.Settings.Count; n++)
                    {
                        ThemePropertySettings settings = theme.Settings[n];
                        settings.Theme = ProfileItem.GetTheme(settings, gameObject, lists);

                        theme.Settings[n] = settings;
                        runningThemesList.Add(settings.Theme);
                    }

                    Profiles[i].Themes[j] = theme;
                }
            }
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
                    Events[i].Receiver.OnUpdate(State.GetState());
                    ReceiverBase reciever = Events[i].Receiver;
                }
            }

            for (int i = 0; i < runningThemesList.Count; i++)
            {
                if (runningThemesList[i].Loaded)
                {
                    runningThemesList[i].OnUpdate(State.GetState().Index);
                }
            }

            if (lastState != State.GetState())
            {
               
            }

            lastState = State.GetState();
        }

    }
}
