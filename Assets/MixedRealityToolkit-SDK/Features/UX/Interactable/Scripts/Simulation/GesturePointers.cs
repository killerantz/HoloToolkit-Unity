// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Core.Definitions.Physics;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.Devices;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.InputSystem;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.InputSystem.Handlers;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.Physics;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.TeleportSystem;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SDK.UX
{
    public class GesturePointers : MonoBehaviour
    {
        public InteractableGestureManipulator Manipulator;
        public SimulatedPointer[] Pointers;

        private List<Interactable.PointerData> pointerData = new List<Interactable.PointerData>();

        private bool started = false;

        private void Update()
        {
            pointerData = new List<Interactable.PointerData>();
            for (int i = 0; i < Pointers.Length; i++)
            {
                if (Pointers[i].gameObject.activeSelf)
                {
                    Interactable.PointerData data = new Interactable.PointerData();
                    data.Pointer = Pointers[i];
                    data.ActionScore = 1;
                    data.HasFocus = true;
                    data.HasPress = true;
                    pointerData.Add(data);
                }
            }

            InteractableGestureManipulator.GestureStatusEnum status = InteractableGestureManipulator.GestureStatusEnum.None;
            if (pointerData.Count > 0 || started)
            {
                if (pointerData.Count > 0)
                {
                    if (started)
                    {
                        status = InteractableGestureManipulator.GestureStatusEnum.Update;
                    }
                    else
                    {
                        status = InteractableGestureManipulator.GestureStatusEnum.Start;
                        started = true;
                    }
                }
                else
                {
                    started = false;
                }

                if (Manipulator != null)
                {
                    Manipulator.OnUpdate(pointerData.ToArray(), status);
                }
            }
        }
    }
}
