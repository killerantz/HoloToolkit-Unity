﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Core.Utilities.InspectorFields;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Microsoft.MixedReality.Toolkit.SDK.UX
{
    /// <summary>
    /// A basic focus event receiver
    /// </summary>
    public class InteractableOnFocusReceiver : ReceiverBase
    {
        [InspectorField(Type = InspectorField.FieldTypes.Float, Label = "On Focus Delay", Tooltip = "Delay the onFocus event")]
        public float OnFocusDelay = 0;

        [InspectorField(Type = InspectorField.FieldTypes.Event, Label = "On Focus Off", Tooltip = "Focus has left the object")]
        public UnityEvent OnFocusOff = new UnityEvent();

        [InspectorField(Type = InspectorField.FieldTypes.Float, Label = "On Focus Off Delay", Tooltip = "Delay the onFocusOff event")]
        public float OnFocusOffDelay = 0;

        private bool hadFocus;
        private State lastState;

        private Coroutine timer;

        public InteractableOnFocusReceiver(UnityEvent ev) : base(ev)
        {
            Name = "OnFocus";
        }

        public override void OnUpdate(InteractableStates state, Interactable source)
        {
            bool changed = state.CurrentState() != lastState;

            bool hasFocus = state.GetState(InteractableStates.InteractableStateEnum.Focus).Value > 0;

            if (hadFocus != hasFocus && changed)
            {
                if (timer != null)
                {
                    source.StopCoroutine(timer);
                }

                if (hasFocus)
                {
                    if (OnFocusDelay > 0)
                    {
                        timer = source.StartCoroutine(DelayTimer(OnFocusDelay, uEvent));
                    }
                    else
                    {
                        uEvent.Invoke();
                    }
                }
                else
                {
                    if (OnFocusOffDelay > 0)
                    {
                        timer = source.StartCoroutine(DelayTimer(OnFocusOffDelay, OnFocusOff));
                    }
                    else
                    {
                        OnFocusOff.Invoke();
                    }
                }
            }

            hadFocus = hasFocus;
            lastState = state.CurrentState();
        }

        private IEnumerator DelayTimer(float time, UnityEvent onEvent)
        {
            yield return new WaitForSeconds(time);
            timer = null;
            onEvent.Invoke();
        }
    }
}
