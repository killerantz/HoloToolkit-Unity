// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System.Collections;
using HoloToolkit.Unity;

namespace HoloToolkit.Examples.Prototyping
{
    /// <summary>
    /// scales an object based on the selected value in the array
    /// Supports ScaletoValue for animaiton and easing, ...auto detected.
    /// </summary>
    public class CycleScale : CycleArray<Vector3>
    {
        private TransitionToScale mTransition;

        private ScaleToValue mScaler; // support for obsolete transition

        protected override void Awake()
        {
            base.Awake();

            mTransition = TargetObject.GetComponent<TransitionToScale>();
            mScaler = TargetObject.GetComponent<ScaleToValue>();
        }

        /// <summary>
        /// Set the scale value or animate scale
        /// </summary>
        /// <param name="index"></param>
        public override void SetIndex(int index)
        {
            base.SetIndex(index);

            Vector3 item = Current;

            if (mTransition != null)
            {
                mTransition.TargetValue = item;
                mTransition.Run();
            }
            else if (mScaler != null)
            {
                mScaler.TargetValue = item;
                mScaler.StartRunning();
            }
            else
            {
                TargetObject.transform.localScale = item;
            }
        }
    }
}
