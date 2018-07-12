﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.Events;

namespace Interact
{
    public class InteractivePress : Interactive
    {
        public UnityEvent OnPress;
        public UnityEvent OnRelease;

        public override void OnInputDown(InputEventData eventData)
        {
            base.OnInputDown(eventData);
            OnPress.Invoke();
        }

        /// <summary>
        /// All tab, hold, and gesture events are completed
        /// </summary>
        public override void OnInputUp(InputEventData eventData)
        {
            bool ignoreRelease = mCheckRollOff;
            base.OnInputUp(eventData);
            if (!ignoreRelease)
            {
                OnRelease.Invoke();
            }
        }
    }
}
