// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System.Collections;
using HoloToolkit.Examples.Prototyping;

namespace HoloToolkit.Examples.InteractiveElements
{
    /// <summary>
    /// updates the button rotation based on a Vector3 theme (Euler Angles)
    /// </summary>
    public class EulerRotationThemeWidget : InteractiveThemeWidget
    {
        
        private TransitionToEulerRotation mTransition;

        private Vector3InteractiveTheme mRotationTheme;

        /// <summary>
        /// Get Move to Position
        /// </summary>
        private void Awake()
        {
            mTransition = GetComponent<TransitionToEulerRotation>();
        }

        public override void SetTheme()
        {
            mRotationTheme = GetVector3Theme(ThemeTag);
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
                if(mTransition != null)
                {
                    mTransition.TargetValue = mRotationTheme.GetThemeValue(state);
                    mTransition.Run();
                }
                else
                {
                    transform.localRotation = Quaternion.Euler(mRotationTheme.GetThemeValue(state));
                }
            }
            else
            {
                IsInited = false;
            }
        }
    }
}
