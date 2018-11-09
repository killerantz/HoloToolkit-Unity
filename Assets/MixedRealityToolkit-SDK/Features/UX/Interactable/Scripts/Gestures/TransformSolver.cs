// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SDK.UX
{
    public abstract class TransformSolver
    {
        public struct TransformData
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Scale;
            public Vector3 Forward;
            public Vector3 Up;
            public Vector3 Right;
        }
        
        /// <summary>
        /// Get the cached source TransformData
        /// </summary>
        public TransformData SourceTransformData { get { return source; } }

        /// <summary>
        /// Get the cached target TransformData
        /// </summary>
        public TransformData TargetTransformData { get { return target; } }

        protected TransformData target;
        protected TransformData source;
        protected Transform transform;
        
        public TransformSolver()
        {
            // set any settings or config
        }

        /// <summary>
        /// Convert a transform to TransformData
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static TransformData GetTransformData(Transform transform)
        {
            TransformData data = new TransformData();
            data.Position = transform.position;
            data.Scale = transform.localScale;
            data.Rotation = transform.rotation;
            data.Forward = transform.forward;
            data.Right = transform.right;
            data.Up = transform.up;
            return data;
        }

        /// <summary>
        /// Convert transform properties to TransformData
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        /// <param name="forward"></param>
        /// <param name="up"></param>
        /// <returns></returns>
        public static TransformData GetTransformData(Vector3 position, Quaternion rotation, Vector3 scale, Vector3 forward, Vector3 up, Vector3 right)
        {
            TransformData data = new TransformData();
            data.Position = position;
            data.Scale = scale;
            data.Rotation = rotation;
            data.Forward = forward;
            data.Right = right;
            data.Up = up;
            return data;
        }

        /// <summary>
        /// Internal version of setup the target
        /// Save any cached properties or start values
        /// </summary>
        /// <param name="sourceTransformData"></param>
        /// <param name="targetTransformData"></param>
        /// <returns></returns>
        protected abstract TransformData SetupTarget(TransformData sourceTransformData, TransformData targetTransformData);

        /// <summary>
        /// Save any cache or start values
        /// </summary>
        /// <param name="sourceTransformData"></param>
        /// <param name="targetTransform"></param>
        /// <returns></returns>
        public virtual TransformData SetupTarget(TransformData sourceTransformData, Transform targetTransform)
        {
            TransformData targetData = GetTransformData(targetTransform);
            source = sourceTransformData;
            target = SetupTarget(sourceTransformData, targetData);
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
        protected abstract TransformData UpdateTarget(TransformData sourceTransformData, TransformData targetTransformData);

        /// <summary>
        /// Process based on delta sense last update or start
        /// </summary>
        /// <param name="sourceTransformData"></param>
        /// <param name="targetTransform"></param>
        /// <returns></returns>
        public virtual TransformData UpdateTarget(TransformData sourceTransformData, Transform targetTransform)
        {
            TransformData targetData = GetTransformData(targetTransform);
            source = sourceTransformData;
            target = UpdateTarget(sourceTransformData, targetData);
            transform = targetTransform;
            return target;
        }
    }
}
