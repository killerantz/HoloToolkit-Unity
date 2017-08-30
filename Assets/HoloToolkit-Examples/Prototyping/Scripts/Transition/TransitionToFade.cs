// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HoloToolkit.Examples.Prototyping
{
    /// <summary>
    /// A color blending animation component, handles multiple materials
    /// </summary>

    public class TransitionToFade : TransitionTo<float>
    {

        [Tooltip("Select the shader for the color property to animate: _Color is default")]
        public ColorObject.ShaderColorTypes ShaderColorType = Prototyping.ColorObject.ShaderColorTypes.Color;

        private ColorAbstraction colorAbstraction;

        protected override void Awake()
        {
            base.Awake();
            GetColorAbstraction();
        }

        /// <summary>
        /// a color abstraction that colors materials and text objects
        /// </summary>
        /// <returns></returns>
        private ColorAbstraction GetColorAbstraction()
        {
            if (colorAbstraction == null)
            {
                colorAbstraction = new ColorAbstraction(TargetObject, ShaderColorType);
            }

            return colorAbstraction;
        }

        /// <summary>
        /// is the animation complete
        /// </summary>
        /// <param name="value1">Color</param>
        /// <param name="value2">Color</param>
        /// <returns></returns>
        public override bool CompareValues(float value1, float value2)
        {
            return value1 == value2;
        }

        /// <summary>
        /// get the current alpha
        /// </summary>
        /// <returns>Color</returns>
        public override float GetValue()
        {
            var colorAbs = GetColorAbstraction();
            return colorAbs.GetAlpha();
        }

        /// <summary>
        /// animate the alpha
        /// </summary>
        /// <param name="startValue">Color</param>
        /// <param name="targetValue">Color</param>
        /// <param name="percent">Color</param>
        /// <returns>Color</returns>
        public override float LerpValues(float startValue, float targetValue, float percent)
        {
            return percent;
        }

        /// <summary>
        /// update the alpha value
        /// </summary>
        /// <param name="value">Color</param>
        public override void SetValue(float value)
        {
            GetColorAbstraction().SetAlpha(value);
        }
    }
}
