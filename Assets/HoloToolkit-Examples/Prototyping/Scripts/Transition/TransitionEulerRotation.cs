﻿// Copyright (c) Microsoft Corporation. All rights reserved.
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
    public class TransitionEulerRotation : TransitionTo<Vector3>
    {
        [Tooltip("will the animation be applied to the local or global transform?")]
        public bool ToLocalTransform = false;

        /// <summary>
        /// is the animation complete?
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public override bool CompareValues(Vector3 value1, Vector3 value2)
        {
            return value1 == value2;
        }

        /// <summary>
        /// get the current rotation
        /// </summary>
        /// <returns></returns>
        public override Vector3 GetValue()
        {
            if (ToLocalTransform)
            {
                return TargetObject.transform.localRotation.eulerAngles;
            }

            return TargetObject.transform.rotation.eulerAngles;
        }

        /// <summary>
        /// animate the rotation
        /// </summary>
        /// <param name="startValue"></param>
        /// <param name="targetValue"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        public override Vector3 LerpValues(Vector3 startValue, Vector3 targetValue, float percent)
        {
            return Vector3.Lerp(startValue, targetValue, percent);
        }

        /// <summary>
        /// set the rotation
        /// </summary>
        /// <param name="value"></param>
        public override void SetValue(Vector3 value)
        {
            if (ToLocalTransform)
            {
                TargetObject.transform.localRotation = Quaternion.Euler(value);
            }
            else
            {
                TargetObject.transform.rotation = Quaternion.Euler(value);
            }
        }
    }
}