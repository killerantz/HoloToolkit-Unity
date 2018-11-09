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

        protected override TransformSolver.TransformData SetupTarget(InteractableGestureManipulator.GestureData sourceData, TransformSolver.TransformData targetTransformData)
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

        protected override TransformSolver.TransformData UpdateTarget(InteractableGestureManipulator.GestureData sourceData, TransformSolver.TransformData targetTransformData)
        {
            Vector3 direction = sourceData.Direction;
            float scaleDelta = direction.magnitude - startDistance;

            currentScale = startScale * (1 + scaleDelta) * TransformMultiplier;
            targetTransformData.Scale = currentScale;
            
            Quaternion gestureOrientation = Quaternion.identity;

            if (!UniformScale)
            {
                InteractableGestureManipulator.GestureDataInputValues[] values = sourceData.InputValues;

                if (values.Length > 1)
                {
                    gestureOrientation = InteractableGestureManipulator.GetRelativeRotation(values[0].CurrentPoint, values[1].CurrentPoint);
                }
                else
                {
                    Vector3 objectStartVector = startPosition - sourceData.StartDirection;
                    Vector3 objectCurrentVector = startPosition - sourceData.Currrent;
                    gestureOrientation = Quaternion.FromToRotation(objectStartVector, objectCurrentVector);
                }

                Quaternion objectOrientation = Quaternion.LookRotation(targetTransformData.Forward, targetTransformData.Up);
                Quaternion objectDifference = objectOrientation * Quaternion.Inverse(gestureOrientation);

                Quaternion axisOrientation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                Quaternion axisDifference = objectOrientation * Quaternion.Inverse(gestureOrientation);

                Vector3 rotatedDirection = Quaternion.Inverse(objectDifference) * direction;
                Vector3 rotatedConstraints = Quaternion.Inverse(axisDifference) * AxisContraints;
                
                targetTransformData.Scale = startScale + Vector3.Scale(rotatedDirection, rotatedConstraints) * TransformMultiplier;
            }
            
            return targetTransformData;
        }
    }
}
