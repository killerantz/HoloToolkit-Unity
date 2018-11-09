// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SDK.UX
{
    /// <summary>
    /// A gesture data solver for turning gesture data to rotation values
    /// </summary>
    public class GestureDataRotationSolver : GestureDataSolver
    {
        /// <summary>
        /// Scale the values applied from the gesture
        /// </summary>
        public float TransformMultiplier = 1;

        protected Quaternion currentRotation = Quaternion.identity;
        protected Quaternion offsetRotation = Quaternion.identity;
        protected Quaternion startRotation = Quaternion.identity;
        private Vector3 startPosition = Vector3.zero;

        public GestureDataRotationSolver(float multiplier)
        {
            TransformMultiplier = multiplier;
        }

        protected override TransformSolver.TransformData SetupTarget(InteractableGestureManipulator.GestureData sourceData, TransformSolver.TransformData targetTransformData)
        {
            startRotation = targetTransformData.Rotation;
            startPosition = targetTransformData.Position;

            gestureData = sourceData;
            target = targetTransformData;

            InteractableGestureManipulator.GestureDataInputValues[] values = sourceData.InputValues;
            
            if (values.Length > 1)
            {
                offsetRotation = InteractableGestureManipulator.GetRelativeRotation(values[0].CurrentPoint, values[1].CurrentPoint);
            }
            else
            {
                Vector3 objectStartVector = startPosition - sourceData.StartDirection;
                Vector3 objectCurrentVector = startPosition - sourceData.Currrent;
                offsetRotation = Quaternion.FromToRotation(objectStartVector, objectCurrentVector);
            }

            return targetTransformData;
        }

        protected override TransformSolver.TransformData UpdateTarget(InteractableGestureManipulator.GestureData sourceData, TransformSolver.TransformData targetTransformData)
        {
            Quaternion newRotation = Quaternion.identity;

            InteractableGestureManipulator.GestureDataInputValues[] values = sourceData.InputValues;

            if (values.Length > 1)
            {
                // two hands
                newRotation = InteractableGestureManipulator.GetRelativeRotation(values[0].CurrentPoint, values[1].CurrentPoint);
            }
            else
            {
                Vector3 objectStartVector = startPosition - sourceData.StartDirection;
                Vector3 objectCurrentVector = startPosition - sourceData.Currrent;
                newRotation = Quaternion.FromToRotation(objectStartVector, objectCurrentVector);
            }

            Quaternion updatedRotation = (newRotation * Quaternion.Inverse(offsetRotation)) * currentRotation;
            currentRotation = Quaternion.Slerp(currentRotation, updatedRotation, TransformMultiplier);
            offsetRotation = newRotation;
            

            targetTransformData.Rotation = currentRotation;

            return targetTransformData;
        }
    }
}
