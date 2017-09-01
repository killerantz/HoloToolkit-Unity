// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HoloToolkit.Examples.InteractiveElements
{
    /// <summary>
    /// An Interactive Theme Widget for swapping textures based on interactive state
    /// </summary>
    public class StringThemeWidget : InteractiveThemeWidget
    {
        [Tooltip("A tag for finding the theme in the scene")]
        public string ThemeTag = "defaultString";

        [Tooltip("The target Text or TextMesh object : optional, leave blank for self")]
        public GameObject Target;

        /// <summary>
        /// The theme with the texture states
        /// </summary>
        protected StringInteractiveTheme mStringTheme;
        
        private TextMesh mTextMesh;
        private Text mText;

        void Awake()
        {
            // set the target
            if (Target == null)
            {
                Target = this.gameObject;
            }

            mText = Target.GetComponent<Text>();
            mTextMesh = Target.GetComponent<TextMesh>();

            // set the renderer
            Renderer renderer = Target.GetComponent<Renderer>();

            if (mTextMesh == null && mText == null)
            {
                Debug.LogError("Textmesh or Text is not available to StringThemeWidget!");
                Destroy(this);
            }
        }

        /// <summary>
        /// Find the theme is none was manually set
        /// </summary>
        protected override void Start()
        {
            if (mStringTheme == null)
            {
                mStringTheme = GetStringTheme(ThemeTag);
            }

            base.Start();
        }

        /// <summary>
        /// From InteractiveWidget
        /// </summary>
        /// <param name="state"></param>
        public override void SetState(Interactive.ButtonStateEnum state)
        {
            base.SetState(state);

            if (mStringTheme != null)
            {
                if (mTextMesh != null)
                {
                    mTextMesh.text = mStringTheme.GetThemeValue(state);
                }
                else if (mText != null)
                {
                    mText.text = mStringTheme.GetThemeValue(state);
                }
            }
            else
            {
                IsInited = false;
            }
        }
    }
}
