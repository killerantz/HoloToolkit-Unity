// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SDK.UX
{
    public abstract class GestureDataSolver
    {
        /// <summary>
        /// Get the cached source TransformData
        /// </summary>
        public InteractableGestureManipulator.GestureData GestureData { get { return gestureData; } }

        /// <summary>
        /// Get the cached target TransformData
        /// </summary>
        public TransformSolver.TransformData TargetTransformData { get { return target; } }

        protected InteractableGestureManipulator.GestureData gestureData;

        protected TransformSolver.TransformData target;
        protected Transform transform;

        public GestureDataSolver()
        {
            // set any settings or config
        }

        /// <summary>
        /// Internal version of setup the target
        /// Save any cached properties or start values
        /// </summary>
        /// <param name="sourceTransformData"></param>
        /// <param name="targetTransformData"></param>
        /// <returns></returns>
        protected abstract TransformSolver.TransformData SetupTarget(InteractableGestureManipulator.GestureData sourceData, TransformSolver.TransformData targetTransformData);

        /// <summary>
        /// Save any cache or start values
        /// </summary>
        /// <param name="sourceTransformData"></param>
        /// <param name="targetTransform"></param>
        /// <returns></returns>
        public virtual TransformSolver.TransformData SetupTarget(InteractableGestureManipulator.GestureData sourceData, Transform targetTransform)
        {
            TransformSolver.TransformData targetData = TransformSolver.GetTransformData(targetTransform);
            gestureData = sourceData;
            target = SetupTarget(sourceData, targetData);
            transform = targetTransform;
            return target;
        }

        /// <summary>
        /// Internal version of Update
        /// Process based on deltas sense last update or start
        /// </summary>
        /// <param name="sourceTransformData"></param>
        /// <param name="targetTransformData"></param>
        /// <returns></returns>
        protected abstract TransformSolver.TransformData UpdateTarget(InteractableGestureManipulator.GestureData sourceData, TransformSolver.TransformData targetTransformData);

        /// <summary>
        /// Process based on delta sense last update or start
        /// </summary>
        /// <param name="sourceTransformData"></param>
        /// <param name="targetTransform"></param>
        /// <returns></returns>
        public virtual TransformSolver.TransformData UpdateTarget(InteractableGestureManipulator.GestureData sourceData, Transform targetTransform)
        {
            TransformSolver.TransformData targetData = TransformSolver.GetTransformData(targetTransform);
            gestureData = sourceData;
            target = UpdateTarget(sourceData, targetData);
            transform = targetTransform;
            return target;
        }

        public static TransformSolver.TransformData GestureDataToTransformData(InteractableGestureManipulator.GestureData sourceData)
        {
            TransformSolver.TransformData data = new TransformSolver.TransformData();
            data.Position = sourceData.Currrent;
            data.Rotation = Quaternion.identity;
            if (sourceData.InputValues.Length > 0)
            {
                data.Rotation = sourceData.InputValues[0].CurrentRotation;
            }
            
            data.Scale = Vector3.one;
            data.Forward = sourceData.HeadRay;
            data.Right = sourceData.CameraRight;
            data.Up = Vector3.Cross(data.Forward, data.Right);

            return data;

        }
    }
}
