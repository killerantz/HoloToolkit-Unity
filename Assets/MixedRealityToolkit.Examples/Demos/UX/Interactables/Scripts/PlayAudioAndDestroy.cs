// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.UI
{
    [RequireComponent(typeof(AudioSource))]
    public class PlayAudioAndDestroy : MonoBehaviour
    {
        private void OnEnable()
        {
            AudioSource source = GetComponent<AudioSource>();
            source.spatialBlend = 1;
            source.Play();
        }

        private void Update()
        {
            AudioSource source = GetComponent<AudioSource>();
            if (!source.isPlaying)
            {
                Destroy(gameObject);
            }

        }
    }
}
