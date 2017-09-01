﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System.Collections;
using HoloToolkit.Examples.Prototyping;
using System;

namespace HoloToolkit.Examples.InteractiveElements
{
    /// <summary>
    /// updates the button scale based on the button theme
    /// </summary>
    public class ScaleThemeWidget : InteractiveThemeWidget
    {
        [Tooltip("deprecated, use TransformToScale componet to this object and leave this blank")]
        public ScaleToValue ScaleTweener;

        private TransitionToScale mTransition;

        private Vector3InteractiveTheme mScaleTheme;
        private Material mMaterial;

        /// <summary>
        /// Get Scale to Value
        /// </summary>
        private void Awake()
        {
            if (ScaleTweener == null)
            {
                ScaleTweener = GetComponent<ScaleToValue>();
            }

            mTransition = GetComponent<TransitionToScale>();
        }

        public override void SetTheme()
        {
            mScaleTheme = GetVector3Theme(ThemeTag);
        }

        /// <summary>
        /// Set the Scale
        /// </summary>
        /// <param name="state"></param>
        public override void SetState(Interactive.ButtonStateEnum state)
        {
            base.SetState(state);
            
            if (mScaleTheme != null)
            {
                if(mTransition != null)
                {
                    mTransition.TargetValue = mScaleTheme.GetThemeValue(state);
                    mTransition.Run();
                }
                if (ScaleTweener != null)
                {
                    ScaleTweener.TargetValue = mScaleTheme.GetThemeValue(state);
                    ScaleTweener.StartRunning();
                }
                else
                {
                    transform.localScale = mScaleTheme.GetThemeValue(state);
                }
            }
        }
    }
}
