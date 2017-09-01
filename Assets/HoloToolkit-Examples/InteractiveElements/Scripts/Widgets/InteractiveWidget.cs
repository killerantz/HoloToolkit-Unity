﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System.Collections;

namespace HoloToolkit.Examples.InteractiveElements
{
    /// <summary>
    /// InteractiveState can exist on a child element of the game object containing the Interactive component.
    /// Extend this class to make custom behaviors that listen from state updates from Interactive
    /// </remarks>
    public class InteractiveWidget : MonoBehaviour
    {
        [Tooltip("The Interactive that will update the widget, optional: use if the widget is a sibling of the Interactive or if the parent Interactive is child of another Interactive")]
        public Interactive InteractiveHost;

        // the Interactive state
        protected Interactive.ButtonStateEnum State;
        protected bool IsInited = false;
        
        /// <summary>
        /// Interactive calls this method on state change
        /// </summary>
        /// <param name="state">
        /// Enum containing the following states:
        /// DefaultState: normal state of the button
        /// FocusState: gameObject has gaze
        /// PressState: currently being pressed
        /// SelectedState: selected and has no other interaction
        /// FocusSelected: selected with gaze
        /// PressSelected: selected and pressed
        /// Disabled: button is disabled
        /// DisabledSelected: the button is not interactive, but in it's alternate state (toggle button)
        /// </param>
        public virtual void SetState(Interactive.ButtonStateEnum state)
        {
            switch (state)
            {
                case Interactive.ButtonStateEnum.Default:
                    break;
                case Interactive.ButtonStateEnum.Focus:
                    break;
                case Interactive.ButtonStateEnum.Press:
                    break;
                case Interactive.ButtonStateEnum.Selected:
                    break;
                case Interactive.ButtonStateEnum.FocusSelected:
                    break;
                case Interactive.ButtonStateEnum.PressSelected:
                    break;
                case Interactive.ButtonStateEnum.Disabled:
                    break;
                case Interactive.ButtonStateEnum.DisabledSelected:
                    break;
                default:
                    break;
            }

            State = state;
			IsInited = true;				
        }

        /// <summary>
        /// get the state of the available InteractiveHost
        /// </summary>
        /// <returns></returns>
        protected Interactive.ButtonStateEnum GetInteractiveHostState()
        {
            if(InteractiveHost == null)
            {
                InteractiveHost = GetComponentInParent<Interactive>();
            }

            if (InteractiveHost != null)
            {
                return InteractiveHost.State;
            }

            return Interactive.ButtonStateEnum.Default;
        }

        /// <summary>
        /// check for state updates
        /// </summary>
        protected virtual void Update()
        {
            Interactive.ButtonStateEnum state = GetInteractiveHostState();
            if (State != state)
            {
                SetState(state);
            }
        }
    }
}
