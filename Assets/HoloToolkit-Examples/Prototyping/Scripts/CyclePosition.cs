// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System.Collections;
using HoloToolkit.Unity;

namespace HoloToolkit.Examples.Prototyping
{
    /// <summary>
    /// updates the position of an object based on currently selected values from the array.
    /// Use MoveToPosition for easing... Auto detected
    /// </summary>
    public class CyclePosition : CycleArray<Vector3>
    {
        [Tooltip("use local position instead of position. Overrides MoveToPosition ToLocalPosition setting.")]
        public bool UseLocalPosition = false;

        private TransitionToPosition mTransition;

        // to support obsolete Transitions
        private MoveToPosition mMoveTranslator;

        protected override void Awake()
        {
            base.Awake();
            mTransition = TargetObject.GetComponent<TransitionToPosition>();
            mMoveTranslator = TargetObject.GetComponent<MoveToPosition>();
        }

        /// <summary>
        /// set the position
        /// </summary>
        /// <param name="index"></param>
        public override void SetIndex(int index)
        {
            base.SetIndex(index);

            Vector3 item = Array[Index];

            // use MoveTo Position
            if (mTransition != null)
            {
                mTransition.ToLocalTransform = UseLocalPosition;
                mTransition.TargetValue = item;
                mTransition.Run();
            }
            else if(mMoveTranslator != null) // support for obsolete transition
            {
                mMoveTranslator.ToLocalTransform = UseLocalPosition;
                mMoveTranslator.TargetValue = item;
                mMoveTranslator.StartRunning();
            }
            else
            {
                if (UseLocalPosition)
                {
                    TargetObject.transform.localPosition = item;
                }
                else
                {
                    TargetObject.transform.position = item;
                }
                
            }
        }

    }
}
