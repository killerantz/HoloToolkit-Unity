// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Microsoft.MixedReality.Toolkit.SDK.UX
{
    /// <summary>
    /// The base class for all receivers that attach to Interactables
    /// </summary>
    public abstract class ReceiverBase
    {
        public string Name;

        public bool HideUnityEvents;
        protected UnityEvent uEvent;

        public ReceiverBase(UnityEvent ev)
        {
            uEvent = ev;
        }

        public abstract void OnUpdate(InteractableStates state, Interactable source);

        public virtual void OnKeyword(float value)
        {
            // a keyword has been spoken
            // value is percent of keyword or keywords registered for this Interactable for toggles or gestures
        }
    }
}
