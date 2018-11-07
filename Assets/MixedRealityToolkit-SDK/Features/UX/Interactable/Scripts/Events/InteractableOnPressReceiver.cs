// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Core.Utilities.InspectorFields;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Microsoft.MixedReality.Toolkit.SDK.UX
{
    /// <summary>
    /// Basic press event receiver
    /// </summary>
    public class InteractableOnPressReceiver : ReceiverBase
    {
        [InspectorField(Type = InspectorField.FieldTypes.Float, Label = "On Press Delay", Tooltip = "Delay the pressed event")]
        public float OnPressDelay = 0;

        [InspectorField(Type = InspectorField.FieldTypes.Event, Label = "On Release", Tooltip = "The button is released")]
        public UnityEvent OnRelease = new UnityEvent();

        [InspectorField(Type = InspectorField.FieldTypes.Float, Label = "On Release Delay", Tooltip = "Delay the released event")]
        public float OnReleaseDelay = 0;
        
        private bool hasDown;
        private State lastState;

        private Coroutine timer;

        public InteractableOnPressReceiver(UnityEvent ev) : base(ev)
        {
            Name = "OnPress";
        }

        public override void OnUpdate(InteractableStates state, Interactable source)
        {
            bool changed = state.CurrentState() != lastState;

            bool hadDown = hasDown;
            hasDown = state.GetState(InteractableStates.InteractableStateEnum.Pressed).Value > 0;

            bool focused = state.GetState(InteractableStates.InteractableStateEnum.Focus).Value > 0;

            if (changed && hasDown != hadDown && focused)
            {
                if (timer != null)
                {
                    source.StopCoroutine(timer);
                }

                if (hasDown)
                {
                    if (OnPressDelay > 0)
                    {
                        timer = source.StartCoroutine(DelayTimer(OnPressDelay, uEvent));
                    }
                    else
                    {
                        uEvent.Invoke();
                    }                }
                else
                {
                    if (OnReleaseDelay > 0)
                    {
                        timer = source.StartCoroutine(DelayTimer(OnReleaseDelay, OnRelease));
                    }
                    else
                    {
                        OnRelease.Invoke();
                    }
                }
            }
            
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
