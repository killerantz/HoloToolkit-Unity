// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.UI
{
    public class InteractableAudioTheme : InteractableThemeBase
    {
        private AudioSource audioSource;

        private int stateChangeCount;
        private int[] playCounts = null;
        private bool isForced = false;
        private int clickCount = 0;

        public float VolumeOverride = 1;

        private bool inited = false;

        public InteractableAudioTheme()
        {
            Types = new Type[] { typeof(Transform) };
            Name = "Audio Theme";
            NoEasing = true;
            ThemeProperties.Add(
                new InteractableThemeProperty()
                {
                    Name = "Audio Clip",
                    Type = InteractableThemePropertyValueTypes.AudioClip,
                    Values = new List<InteractableThemePropertyValue>(),
                    Default = new InteractableThemePropertyValue() { AudioClip = null }
                });
            ThemeProperties.Add(
                new InteractableThemeProperty()
                {
                    Name = "Audio Play Limit",
                    Tooltip = "The times audio can play after interaction started, will reset when interaction ends",
                    Type = InteractableThemePropertyValueTypes.Int,
                    Values = new List<InteractableThemePropertyValue>(),
                    Default = new InteractableThemePropertyValue() { Int = 0 }
                });

            CustomSettings.Add(
                new InteractableCustomSetting()
                {
                    Name = "Volume",
                    Type = InteractableThemePropertyValueTypes.Float,
                    Value = new InteractableThemePropertyValue() { Float = 1 }
                });
            CustomSettings.Add(
                new InteractableCustomSetting()
                {
                    Name = "OnClick Clip",
                    Type = InteractableThemePropertyValueTypes.AudioClip
                });
        }

        public override void Init(GameObject host, InteractableThemePropertySettings settings)
        {
            base.Init(host, settings);
            audioSource = Host.GetComponentInChildren<AudioSource>();

            if (audioSource == null)
            {
                SetupAudioSource(Host);
                ConfigureAudioSource();
            }
        }

        public override void Reset()
        {
            base.Reset();
            if (stateChangeCount > 1)
            {
                stateChangeCount = 0;
            }
        }

        public override void OnUpdate(int state, Interactable source, bool force = false)
        {
            isForced = force;
            if (!inited)
            {
                clickCount = source.ClickCount;
                inited = true;
            }

            base.OnUpdate(state, source, force);
            isForced = false;

            if (source.ClickCount != clickCount)
            {
                if (CustomSettings[1].Value.AudioClip != null)
                {
                    PlayAudioClip(CustomSettings[1].Value.AudioClip);
                }
                clickCount = source.ClickCount;

            }

            VolumeOverride = CustomSettings[0].Value.Float;

        }

        public override InteractableThemePropertyValue GetProperty(InteractableThemeProperty property)
        {
            InteractableThemePropertyValue start = new InteractableThemePropertyValue();
            AudioSource audioSource = Host.GetComponentInChildren<AudioSource>();
            if (audioSource != null)
            {
                start.AudioClip = audioSource.clip;
            }
            return start;
        }

        public override void SetValue(InteractableThemeProperty property, int index, float percentage)
        {
            if (property == ThemeProperties[0])
            {
                if (SetupAudioSource(Host))
                {
                    ConfigureAudioSource();
                }

                bool playSound = false;
                if (playCounts == null || index == 0)
                {
                    playCounts = new int[property.Values.Count];
                    playSound = true;
                }
                else
                {
                    int playLimit = ThemeProperties[1].Values[index].Int;
                    playSound = playLimit == 0 || playLimit > playCounts[index];
                }

                if (ShouldPlay(index) && playSound && !isForced)
                {
                    PlayAudioClip(property.Values[index].AudioClip);
                    playCounts[index]++;
                }

                if (lastState > -1 && lastState != index)
                {
                    stateChangeCount++;
                }
            }
        }

        private void PlayAudioClip(AudioClip clip)
        {
            if (clip != null)
            {
                audioSource.volume = VolumeOverride;
                audioSource.clip = clip;
                audioSource.Play();
            }
        }

        private bool SetupAudioSource(GameObject host)
        {
            if (audioSource == null)
            {
                audioSource = host.AddComponent<AudioSource>();
                return true;
            }

            return false;
        }

        private void ConfigureAudioSource()
        {
            VolumeOverride = Mathf.Clamp01(VolumeOverride);
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1;
            audioSource.volume = VolumeOverride;
        }

        private bool ShouldPlay(int state)
        {

            if (stateChangeCount > 0 || (state != lastState && lastState > -1 && state > 0))
            {
                return true;
            }

            return false;
        }
    }
}
