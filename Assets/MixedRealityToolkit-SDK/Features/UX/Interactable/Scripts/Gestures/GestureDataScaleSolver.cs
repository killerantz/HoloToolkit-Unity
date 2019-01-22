// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SDK.UX
{
    /// <summary>
    /// A gesture data solver for turning gesture data to scale values
    /// </summary>
    public class GestureDataScaleSolver : GestureDataSolver
    {
        /// <summary>
        /// Scale the values applied from the gesture
        /// </summary>
        public float TransformMultiplier = 1;

        /// <summary>
        /// Scales all axis uniformly, false scales based on the closest gesture axis
        /// </summary>
        public bool UniformScale = true;

        /// <summary>
        /// 
        /// </summary>
        public Vector3 AxisContraints = Vector3.one;
        
        protected Vector3 currentScale = Vector3.one;
        protected float startDistance;
        protected Vector3 startScale = Vector3.one;
        protected Vector3 startPosition = Vector3.zero;
        protected Vector3 delta = Vector3.zero;


        public GestureDataScaleSolver(float multiplier, bool uniformScale = true)
        {
            TransformMultiplier = multiplier;
            UniformScale = uniformScale;
        }

        protected override TransformSolver.TransformData SetupTarget(InteractableGestureManipulator.GestureData sourceData, TransformSolver.TransformData targetTransformData, bool modifier = false)
        {
            startScale = targetTransformData.Scale;
            startPosition = targetTransformData.Position;

            gestureData = sourceData;
            target = targetTransformData;

            Vector3 startDirection = sourceData.StartDirection;
            startDistance = startDirection.magnitude;

            delta = Vector3.zero;

            return targetTransformData;
        }

        protected override TransformSolver.TransformData UpdateTarget(InteractableGestureManipulator.GestureData sourceData, TransformSolver.TransformData targetTransformData, bool modifier = false)
        {
            float distance = 1 + sourceData.Distance;
            currentScale = startScale * distance * TransformMultiplier;
            
            if (!UniformScale)
            {
                currentScale = startScale + sourceData.Direction * TransformMultiplier;
            }

            targetTransformData.Scale = currentScale;

            return targetTransformData;
        }
    }
}
