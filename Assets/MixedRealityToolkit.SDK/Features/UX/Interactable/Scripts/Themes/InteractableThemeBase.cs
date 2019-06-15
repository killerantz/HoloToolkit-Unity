// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.UI
{
    /// <summary>
    /// Base class for themes
    /// </summary>

    public abstract class InteractableThemeBase
    {
        public Type[] Types;
        public string Name = "Base Theme";
        public List<InteractableThemeProperty> ThemeProperties = new List<InteractableThemeProperty>();
        public List<InteractableCustomSetting> CustomSettings = new List<InteractableCustomSetting>();
        public GameObject Host;
        public Easing Ease;
        public bool NoEasing;
        public bool Loaded;
        public string AssemblyQualifiedName;

        private bool hasFirstState = false;

        protected int lastState { get; private set; } = -1;

        public abstract void SetValue(InteractableThemeProperty property, int index, float percentage);

        public abstract InteractableThemePropertyValue GetProperty(InteractableThemeProperty property);

        public virtual void Init(GameObject host, InteractableThemePropertySettings settings)
        {
            Host = host;

            for (int i = 0; i < settings.Properties.Count; i++)
            {
                InteractableThemeProperty prop = ThemeProperties[i];
                prop.ShaderOptionNames = settings.Properties[i].ShaderOptionNames;
                prop.ShaderOptions = settings.Properties[i].ShaderOptions;
                prop.PropId = settings.Properties[i].PropId;
                prop.Values = settings.Properties[i].Values;
                
                ThemeProperties[i] = prop;
            }

            for (int i = 0; i < settings.CustomSettings.Count; i++)
            {
                InteractableCustomSetting setting = CustomSettings[i];
                setting.Name = settings.CustomSettings[i].Name;
                setting.Type = settings.CustomSettings[i].Type;
                setting.Value = settings.CustomSettings[i].Value;
                CustomSettings[i] = setting;
            }

            Ease = CopyEase(settings.Easing);
            Ease.Stop();

            Loaded = true;
        }

        protected float LerpFloat(float s, float e, float t)
        {
            return (e - s) * t + s;
        }

        protected int LerpInt(int s, int e, float t)
        {
            return Mathf.RoundToInt((e - s) * t) + s;
        }

        protected Easing CopyEase(Easing ease)
        {
            Easing newEase = new Easing();
            newEase.Curve = ease.Curve;
            newEase.Enabled = ease.Enabled;
            newEase.LerpTime = ease.LerpTime;

            return newEase;
        }

        /// <summary>
        /// A way for Interactable to let themes and events know Interactable was enabled again.
        /// </summary>
        public virtual void Reset()
        {
            // called when Interactable is enabled.
        }

        public virtual void OnUpdate(int state, Interactable source, bool force = false)
        {

            if (state != lastState || force)
            {
                int themePropCount = ThemeProperties.Count;
                for (int i = 0; i < themePropCount; i++)
                {
                    InteractableThemeProperty current = ThemeProperties[i];
                    current.StartValue = GetProperty(current);
                    if (hasFirstState || force)
                    {
                        Ease.Start();
                        SetValue(current, state, Ease.GetCurved());
                        hasFirstState = true;
                    }
                    else
                    {
                        SetValue(current, state, 1);
                        if (i >= themePropCount - 1)
                        {
                            hasFirstState = true;
                        }
                    }
                    ThemeProperties[i] = current;
                }

                lastState = state;
            }
            else if (Ease.Enabled && Ease.IsPlaying())
            {
                Ease.OnUpdate();
                int themePropCount = ThemeProperties.Count;
                for (int i = 0; i < themePropCount; i++)
                {
                    InteractableThemeProperty current = ThemeProperties[i];
                    SetValue(current, state, Ease.GetCurved());
                }
            }

            lastState = state;
        }
    }
}
