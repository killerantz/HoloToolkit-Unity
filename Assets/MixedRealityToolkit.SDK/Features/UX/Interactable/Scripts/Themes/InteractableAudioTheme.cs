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
        private Interactable source = null;
        private bool isForced = false;
        private int clickCount = 0;

        public AudioClip OnClickClip;
        public float VolumeOverride = 1;
        public float FadeSpeed = 0.1f;

        private float fadeTimer = 0;


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
                    Type = InteractableThemePropertyValueTypes.Int,
                    Values = new List<InteractableThemePropertyValue>(),
                    Default = new InteractableThemePropertyValue() { Int = 0 }
                });

            //add custom settings to handle onclick events;
            // use click count
            // add volume

            // add fade speed to fade between clips	
            // do we cache clips? wait until finished?
        }

        public override void Init(GameObject host, InteractableThemePropertySettings settings)
        {
            base.Init(host, settings);
            audioSource = Host.GetComponentInChildren<AudioSource>();

            if(audioSource == null)
            {
                SetupAudioSource(Host);
                ConfigureAudioSource();
            }

            source = Host.GetComponentInParent<Interactable>();
            clickCount = source.ClickCount;
            fadeTimer = FadeSpeed;
        }

		public override void Reset()
        {
            base.Reset();
            if(stateChangeCount > 1)
            {
                stateChangeCount = 0;
            }
        }

        public override void OnUpdate(int state, bool force = false)
        {
            isForced = force;
            base.OnUpdate(state, force);
            isForced = false;

            if(source.ClickCount != clickCount)
            {
                //clicked
                //play click audio

                Debug.Log("PLAY ON CLICK AUDIO!!!!");
                OnClickClip = ThemeProperties[0].Values[0].AudioClip;
                PlayAudioClip(OnClickClip);
                clickCount = source.ClickCount;

            }
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
            if(property == ThemeProperties[0])
            {
                float timeFromStart = Time.time;
                
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

                if(lastState > -1 && lastState != index)
                {
                    stateChangeCount++;
                }
            }
        }

        private void PlayAudioClip(AudioClip clip)
        {
            if(clip != null)
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

            if(stateChangeCount > 0 || (state != lastState && lastState > -1 && state > 0))
            {
                return true;
            }

            return false;
        }
    }
}
