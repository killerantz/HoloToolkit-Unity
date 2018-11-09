// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SDK.UX
{
    /// <summary>
    /// A gesture data solver for turning gesture data to position values
    /// </summary>
    public class GestureDataPositionSolver : GestureDataSolver
    {

        /// <summary>
        /// Scale the values applied from the gesture
        /// </summary>
        public float TransformMultiplier = 1;

        /// <summary>
        /// Projects position from gesture source or controller
        /// Adds rotation from gesture/controller rotation
        /// </summary>
        public bool ProjectPositionFromSource = false;
        
        protected Vector3 currentPostion = Vector3.zero;
        protected Vector3 startPosition = Vector3.zero;

        protected InteractablePositionSolver positionSolver;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="multiplier">Multiply or scale the gesture value applied</param>
        /// <param name="projectFromSource">Project the position from the source of the gesture, adds controller rotation</param>
        public GestureDataPositionSolver(float multiplier, bool projectFromSource)
        {
            TransformMultiplier = multiplier;
            ProjectPositionFromSource = projectFromSource;
        }

        protected override TransformSolver.TransformData SetupTarget(InteractableGestureManipulator.GestureData sourceData, TransformSolver.TransformData targetTransformData)
        {
            startPosition = targetTransformData.Position;
            
            gestureData = sourceData;
            target = targetTransformData;

            if (ProjectPositionFromSource)
            {
                // TODO: do not turn this around, but keep offset facing me
                positionSolver = new InteractablePositionSolver(false, true, true, false);
                TransformSolver.TransformData data = GestureDataSolver.GestureDataToTransformData(sourceData);
                positionSolver.SetupTarget(data, transform);
            }

            return targetTransformData;
        }

        protected override TransformSolver.TransformData UpdateTarget(InteractableGestureManipulator.GestureData sourceData, TransformSolver.TransformData targetTransformData)
        {
            Vector3 direction = sourceData.Direction;
            currentPostion = startPosition + direction.normalized * Mathf.Abs(sourceData.Distance) * TransformMultiplier;

            InteractableGestureManipulator.GestureDataInputValues[] values = sourceData.InputValues;

            if (values.Length > 1)
            {
                // two hands
                currentPostion = startPosition + sourceData.CompoundDirection.normalized * Mathf.Abs(sourceData.CompoundDistance) / values.Length;
            }

            targetTransformData.Position = currentPostion;

            if (ProjectPositionFromSource)
            {
                // TODO: do not turn this around, but keep offset facing me
                positionSolver = new InteractablePositionSolver(false, true, true, false);
                TransformSolver.TransformData data = GestureDataSolver.GestureDataToTransformData(sourceData);
                targetTransformData = positionSolver.UpdateTarget(data, transform);
            }

            Debug.Log(currentPostion + " / " + startPosition + " / " + sourceData.Direction + " / " + sourceData.Distance + " / "+ TransformMultiplier);

            return targetTransformData;
        }
    }
}
