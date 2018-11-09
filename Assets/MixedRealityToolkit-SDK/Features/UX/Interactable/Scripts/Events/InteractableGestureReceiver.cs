// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Core.Definitions.Devices;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.InputSystem;
using Microsoft.MixedReality.Toolkit.Core.Utilities.InspectorFields;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Microsoft.MixedReality.Toolkit.SDK.UX
{
    /// <summary>
    /// A receiver that listens to pressed events and monitors pointers to capture gesture data
    /// </summary>
    public class InteractableGestureReceiver : ReceiverBase
    {
        /// <summary>
        /// The minimum hand or pointers for a gesture to start and stop
        /// </summary>
        [InspectorField(Type = InspectorField.FieldTypes.Int, Label = "Pointer Minimum", Tooltip = "How many pointers are we needed to drive this gesture?")]
        public int PointerMinimum = 1;
        
        // can I get a list in a InspectorField?
        [InspectorField(Type = InspectorField.FieldTypes.ObjectArray, Label = "Gestuer Manipulator", Tooltip = "A GameObject containing a InteractableGestureManipulator")]
        public List<UnityEngine.Object> Manipulators;

        protected List<Interactable.PointerData> pointerData = new List<Interactable.PointerData>();

        protected bool gestureStarted = false;

        public InteractableGestureReceiver(UnityEvent ev) : base(ev)
        {
            Name = "OnGesture";
            HideUnityEvents = true;
        }

        /// <summary>
        /// take input data and figure out when a gesture is happening
        /// route the gesture and pointer data to a gestureManipulator
        /// </summary>
        /// <param name="state"></param>
        /// <param name="source"></param>
        public override void OnUpdate(InteractableStates state, Interactable source)
        {
            pointerData = source.GetPointerData();

            int pointerCount = 0;
            for (int i = 0; i < pointerData.Count; i++)
            {
                // This is not the best way to decide if a pressed event has changed
                // there could be corner cases with false positives if multi buttons were pressed going in
                // I need to have a better comparison for action id set in the Interactable on Press Event to compare with the controller action mapping id's
                // they are currently showing as zero
                if (pointerData[i].HasPress)
                {
                    pointerCount += 1;
                    if (pointerData[i].Pointer.Controller != null)
                    {
                        MixedRealityInteractionMapping[] mappings = pointerData[i].Pointer.Controller.Interactions;
                        int count = 0;
                        for (int j = 0; j < mappings.Length; j++)
                        {
                            count += Mathf.RoundToInt(mappings[j].FloatData);
                            Debug.Log(mappings[j].InputType);
                        }

                        if (count < pointerData[i].ActionScore)
                        {
                            source.UpdatePointerPressed(pointerData[i].Pointer, false);
                            pointerCount -= 1;
                        }
                    }
                }
            }

            bool hasGesture = pointerCount >= PointerMinimum;
            bool updated = hasGesture != gestureStarted;

            InteractableGestureManipulator.GestureStatusEnum status = InteractableGestureManipulator.GestureStatusEnum.None;

            if (hasGesture)
            {
                status = InteractableGestureManipulator.GestureStatusEnum.Update;
            }
            else if (updated && hasGesture)
            {
                status = InteractableGestureManipulator.GestureStatusEnum.Start;
            }

            if (pointerCount >= PointerMinimum || updated)
            {
                // loop through manipulators and send updates
                for (int i = 0; i < Manipulators.Count; i++)
                {
                    if (Manipulators[i] != null)
                    {
                        GameObject gameObject = (GameObject)Manipulators[i];
                        if (gameObject != null)
                        {
                            InteractableGestureManipulator manipulator = gameObject.GetComponent<InteractableGestureManipulator>();
                            if(manipulator != null)
                            {
                                manipulator.OnUpdate(pointerData.ToArray(), status);
                            }
                        }
                    }
                }

            }
            else
            {
                gestureStarted = false;
            }
        }
    }
}
