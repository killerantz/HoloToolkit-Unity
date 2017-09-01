// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System.Collections;
using HoloToolkit.Unity;

namespace HoloToolkit.Examples.Prototyping
{
    /// <summary>
    /// sets the rotation (eulerAngles) of an object to the selected value in the array.
    /// Add RotateToValue for animaiton and easing, auto detected.
    /// </summary>
    public class CycleRotation : CycleArray<Vector3>
    {
        [Tooltip("use the local rotation - overrides the UseLocalTransform value of RotateToValue")]
        public bool UseLocalRotation = false;

        private TransitionToRotation mTransform;
        private RotateToValue mRotation; //support for obsolete transtion

        protected override void Awake()
        {
            base.Awake();
            mTransform = TargetObject.GetComponent<TransitionToRotation>();
            mRotation = TargetObject.GetComponent<RotateToValue>();
        }

        /// <summary>
        /// set the rotation from the vector 3 euler angle
        /// </summary>
        /// <param name="index"></param>
        public override void SetIndex(int index)
        {
            base.SetIndex(index);

            // get the rotation and convert it to a Quaternion
            Vector3 item = Array[Index];

            Quaternion rotation = Quaternion.identity;
            rotation.eulerAngles = item;

            // set the rotation
            if (mTransform != null)
            {
                mTransform.ToLocalTransform = UseLocalRotation;
                mTransform.TargetValue = rotation;
                mTransform.Run();
            }
            else if(mRotation != null)
            {
                mRotation.ToLocalTransform = UseLocalRotation;
                mRotation.TargetValue = rotation;
                mRotation.StartRunning();
            }
            else
            {
                if (UseLocalRotation)
                {
                    TargetObject.transform.localRotation = rotation;
                }
                else
                {
                    TargetObject.transform.rotation = rotation;
                }
            }
        }

    }
}
