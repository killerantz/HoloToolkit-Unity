// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System.Collections;
using HoloToolkit.Examples.Prototyping;
using System;

namespace HoloToolkit.Examples.InteractiveElements
{
    /// <summary>
    /// updates the button position based on the button theme
    /// </summary>
    public class PositionThemeWidget : InteractiveThemeWidget
    {

        [Tooltip("deprecated, use TransformToPosition componet to this object and leave this blank")]
        public MoveToPosition MovePositionTweener; // support for old transition

        private TransitionToPosition mTransform;

        private Vector3InteractiveTheme mPositionTheme;

        /// <summary>
        /// Get Move to Position
        /// </summary>
        private void Awake()
        {
            if (MovePositionTweener == null)
            {
                MovePositionTweener = GetComponent<MoveToPosition>();
            }

            mTransform = GetComponent<TransitionToPosition>();
        }

        public override void SetTheme()
        {
            mPositionTheme = GetVector3Theme(ThemeTag);
        }

        /// <summary>
        /// Set the position
        /// </summary>
        /// <param name="state"></param>
        public override void SetState(Interactive.ButtonStateEnum state)
        {
            base.SetState(state);
            
            if (mPositionTheme != null)
            {
                if(mTransform != null)
                {
                    mTransform.TargetValue = mPositionTheme.GetThemeValue(state);
                    mTransform.Run();
                }
                else if (MovePositionTweener != null)
                {
                    MovePositionTweener.TargetValue = mPositionTheme.GetThemeValue(state);
                    MovePositionTweener.StartRunning();
                }
                else
                {
                    transform.localPosition = mPositionTheme.GetThemeValue(state);
                }
            }
        }
    }
}
