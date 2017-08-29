// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HoloToolkit.Examples.Prototyping
{
    /// <summary>
    /// animates the rotation of an object with eases
    /// </summary>
    public class TransitionRotation : TransitionTo<Quaternion>
    {
        [Tooltip("will the animation be applied to the local or global transform?")]
        public bool ToLocalTransform = false;

        /// <summary>
        /// is the animation complete?
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public override bool CompareValues(Quaternion value1, Quaternion value2)
        {
            return value1 == value2;
        }

        /// <summary>
        /// get the current rotation
        /// </summary>
        /// <returns></returns>
        public override Quaternion GetValue()
        {
            if (ToLocalTransform)
            {
                return TargetObject.transform.localRotation;
            }

            return TargetObject.transform.rotation;
        }

        /// <summary>
        /// animate the rotation
        /// </summary>
        /// <param name="startValue"></param>
        /// <param name="targetValue"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        public override Quaternion LerpValues(Quaternion startValue, Quaternion targetValue, float percent)
        {
            return Quaternion.Lerp(startValue, targetValue, percent);
        }

        /// <summary>
        /// set the rotation
        /// </summary>
        /// <param name="value"></param>
        public override void SetValue(Quaternion value)
        {
            if (ToLocalTransform)
            {
                TargetObject.transform.localRotation = value;
            }
            else
            {
                TargetObject.transform.rotation = value;
            }
        }
    }
}
