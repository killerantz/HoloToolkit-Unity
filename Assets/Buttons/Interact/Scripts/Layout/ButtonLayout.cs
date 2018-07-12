﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interact.Layout
{
    [ExecuteInEditMode]
    public class ButtonLayout : MonoBehaviour
    {
        [Tooltip("Where to set this object's center point in relation to the Anchor's center point")]
        public Vector3 Alignment;

        [Tooltip("A pixel to Unity unit conversion, Default: 2048x2048 pixels covers a 1x1 Unity Unit or default primitive size")]
        public float BasePixelSize = 2048;

        [Tooltip("The transform this object should be linked and aligned to")]
        public Transform Anchor;

        [Tooltip("Offset this object's position based on the same pixel based size ratio")]
        public Vector3 AnchorOffset;

        public bool OnlyInEditMode;

        /// <summary>
        /// A transform is required for alignment
        /// </summary>
        protected virtual void Awake()
        {
            if (Anchor == null)
            {
                Anchor = this.transform;
            }
        }

        /// <summary>
        /// Set this object's position
        /// </summary>
        protected virtual void UpdatePosition()
        {
            // set the default directions
            Vector3 startPosition = Anchor.localPosition;

            if (Anchor != this.transform)
            {
                startPosition = Anchor.localPosition + (Vector3.Scale(Anchor.localScale * 0.5f, Alignment));
            }

            transform.localPosition = startPosition + (AnchorOffset / BasePixelSize);
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            if (Anchor != null)
            {
#if UNITY_EDITOR
                if ((Application.isPlaying && !OnlyInEditMode) || (!Application.isPlaying))
                {
                    UpdatePosition();
                }
#else
                UpdatePosition();
#endif


            }
        }
    }
}
