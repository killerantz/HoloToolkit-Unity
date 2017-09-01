// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System.Collections;
using HoloToolkit.Examples.Prototyping;

namespace HoloToolkit.Examples.InteractiveElements
{
    /// <summary>
    /// updates the button rotation based on a Quaternion theme
    /// </summary>
    public class RotationThemeWidget : InteractiveThemeWidget
    {

        [Tooltip("Move to Position, a component for animating position")]
        public RotateToValue RotationTweener; // support obsolete transition

        private TransitionToRotation mTransition;

        private QuaternionInteractiveTheme mRotationTheme;
        
        /// <summary>
        /// Get Move to Position
        /// </summary>
        private void Awake()
        {
            if (RotationTweener == null)
            {
                RotationTweener = GetComponent<RotateToValue>();
            }

            mTransition = GetComponent<TransitionToRotation>();
        }

        public override void SetTheme()
        {
            mRotationTheme = GetQuaternionTheme(ThemeTag);
        }

        /// <summary>
        /// Set the position
        /// </summary>
        /// <param name="state"></param>
        public override void SetState(Interactive.ButtonStateEnum state)
        {
            base.SetState(state);
            
            if (mRotationTheme != null)
            {
                if (mTransition != null)
                {
                    mTransition.TargetValue = mRotationTheme.GetThemeValue(state);
                    mTransition.Run();
                }
                else if (RotationTweener != null)
                {
                    RotationTweener.TargetValue = mRotationTheme.GetThemeValue(state);
                    RotationTweener.StartRunning();
                }
                else
                {
                    transform.localRotation = mRotationTheme.GetThemeValue(state);
                }
            }
            else
            {
                IsInited = false;
            }
        }
    }
}
