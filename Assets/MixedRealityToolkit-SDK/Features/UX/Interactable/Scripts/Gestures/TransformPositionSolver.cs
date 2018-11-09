// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SDK.UX
{
    public class InteractablePositionSolver : TransformSolver
    {
        /// <summary>
        /// Lock the x axis if the object is set to face the reference object
        /// </summary>
        public bool KeepUpRight = false;

        /// <summary>
        /// Does not center the object to the reference object's transform.forward vector
        /// </summary>
        public bool KeepStartingOffset = true;

        /// <summary>
        /// Force the object to always face the reference object
        /// </summary>
        public bool FaceObject = true;

        /// <summary>
        /// Force the object to keep relative to the reference object's transform.forward
        /// </summary>
        public bool KeepInFront = true;
        
        // the position different between the objects position and the reference object's transform.forward
        protected Vector3 offsetDirection;

        // this object's direction
        protected Vector3 targetDirection;

        // the offset rotation at start
        protected Quaternion offsetRotation;

        // the starting point
        protected Vector3 startPosition;

        // the offset distance at start
        protected float offsetDistance = 0;
        protected Vector3 normalzedOffsetDirection;
        
        public InteractablePositionSolver(bool faceObject = true, bool keepStartingOffset = true, bool keepInFront = true, bool keepUpRight = true)
        {
            FaceObject = faceObject;
            KeepStartingOffset = keepStartingOffset;
            KeepInFront = keepInFront;
            KeepUpRight = keepUpRight;
        }

        /// <summary>
        /// cache start values
        /// </summary>
        /// <param name="sourceTransformData"></param>
        /// <param name="targetTransformData"></param>
        /// <returns></returns>
        protected override TransformData SetupTarget(TransformData sourceTransformData, TransformData targetTransformData)
        {
            TransformData data = targetTransformData;
            offsetDirection = targetTransformData.Position - sourceTransformData.Position;
            offsetDistance = offsetDirection.magnitude;
            targetDirection = sourceTransformData.Forward.normalized;
            normalzedOffsetDirection = offsetDirection.normalized;
            startPosition = targetTransformData.Position;
            offsetRotation = Quaternion.FromToRotation(targetDirection, normalzedOffsetDirection);
            return data;
        }

        /// <summary>
        /// update based on deltas
        /// </summary>
        /// <param name="sourceTransformData"></param>
        /// <param name="targetTransformData"></param>
        /// <returns></returns>
        protected override TransformData UpdateTarget(TransformData sourceTransformData, TransformData targetTransformData)
        {
            TransformData data = targetTransformData;
            Vector3 newDirection = sourceTransformData.Forward;

            // move the object in front of the reference object
            if (KeepInFront)
            {
                if (KeepStartingOffset)
                {
                    newDirection = Vector3.Normalize(offsetRotation * sourceTransformData.Forward);
                }
            }
            else
            {
                newDirection = normalzedOffsetDirection;
                // could we allow drifting?
            }

            Vector3 newPosition = sourceTransformData.Position + newDirection * offsetDistance;
            
            // update the position
            data.Position = newPosition;

            // rotate to face the reference object
            if (FaceObject)
            {
                Quaternion forwardRotation = Quaternion.LookRotation(newPosition - sourceTransformData.Position);
                data.Rotation = forwardRotation;
            }
            else
            {
                Quaternion newRotation = Quaternion.LookRotation(newPosition - startPosition);
                data.Rotation = newRotation;
            }

            // lock the x axis
            if (KeepUpRight)
            {
                Quaternion upRotation = Quaternion.FromToRotation(targetTransformData.Up, Vector3.up);
                data.Rotation = upRotation * data.Rotation;
            }

            return data;
        }
    }
}
