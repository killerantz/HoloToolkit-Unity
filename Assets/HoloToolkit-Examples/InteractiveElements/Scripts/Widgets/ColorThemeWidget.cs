// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Examples.Prototyping;
using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloToolkit.Examples.InteractiveElements
{
    /// <summary>
    /// Changes the color of a material based on the Interactive state and the assigned theme
    /// </summary>
    public class ColorThemeWidget : InteractiveThemeWidget
    {
        [Tooltip("Select the shader color property to animate: _Color is default - overrided by ColorTransition if attached or assigned.")]
        public ColorObject.ShaderColorTypes ShaderColorType = ColorObject.ShaderColorTypes.Color;

        private TransitionToColor mTransition;

        protected ColorInteractiveTheme mColorTheme;
        protected ColorAbstraction mColorAbstraction;

        private void Awake()
        {
            // set up the color abstraction layer
            mColorAbstraction = new ColorAbstraction(gameObject, ShaderColorType);

            mTransition = GetComponent<TransitionToColor>();
        }

        public override void SetTheme()
        {
            mColorTheme = GetColorTheme(ThemeTag);
        }

        /// <summary>
        /// Set or fade the colors
        /// </summary>
        /// <param name="state"></param>
        public override void SetState(Interactive.ButtonStateEnum state)
        {
            base.SetState(state);

            if (mColorTheme != null)
            {
                if(mTransition != null)
                {
                    mTransition.TargetValue = mColorTheme.GetThemeValue(state);
                    mTransition.Run();
                }
                else
                {
                    mColorAbstraction.SetColor(mColorTheme.GetThemeValue(state));
                }
            }
            else
            {
                IsInited = false;
            }
        }
    }
}
