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
        }

        public override void Init(GameObject host, InteractableThemePropertySettings settings)
        {
            base.Init(host, settings);
            audioSource = Host.GetComponentInChildren<AudioSource>();
			audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1;
            source = Host.GetComponentInParent<Interactable>();								
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
                
                if (audioSource == null)
                {
                    audioSource = Host.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 1;
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
                
                if(source.name == "Sphere")
                {
                    Debug.Log("STATE CHANGE " + stateChangeCount + " / PLAY? " + playSound + " / " + ThemeProperties[1].Values[index].Int + " / " + playCounts[index] + " / i: " + index + " / li: " + lastState + " / " + ShouldPlay(index) + " / " + source.GetDimensionIndex() + " / " + isForced);

                }

                if (ShouldPlay(index) && playSound && !isForced)
                {
                    audioSource.clip = property.Values[index].AudioClip;
                    audioSource.Play();
                    playCounts[index]++;
                }

                if(lastState > -1 && lastState != index)
                {
                    stateChangeCount++;
                }
            }
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
