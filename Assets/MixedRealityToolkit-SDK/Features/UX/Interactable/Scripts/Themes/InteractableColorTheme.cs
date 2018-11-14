// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if TMP_PRESENT
using TMPro;
#endif // TMP_PRESENT

namespace Microsoft.MixedReality.Toolkit.SDK.UX.Interactable.Themes
{
    public class InteractableColorTheme : InteractableShaderTheme
    {
        public InteractableColorTheme()
        {
            Types = new Type[] { typeof(Renderer), typeof(TextMesh), typeof(Text) };
            Name = "Color Theme";
            ThemeProperties = new List<InteractableThemeProperty>();
            ThemeProperties.Add(
                new InteractableThemeProperty()
                {
                    Name = "Color",
                    Type = InteractableThemePropertyValueTypes.Color,
                    Values = new List<InteractableThemePropertyValue>(),
                    Default = new InteractableThemePropertyValue() { Color = Color.white}
                });
        }

        protected delegate bool SetColorDelegate(Color color);
        protected delegate Color GetColorDelegate(out bool success);
        protected SetColorDelegate SetColorValue;
        protected GetColorDelegate GetColorValue;


        public override InteractableThemePropertyValue GetProperty(InteractableThemeProperty property)
        {
            InteractableThemePropertyValue color = new InteractableThemePropertyValue();
            TextMesh mesh = Host.GetComponent<TextMesh>();
            if (mesh != null)
            {
                color.Color = mesh.color;
                return color;
            }

            Text text = Host.GetComponent<Text>();
            if (text != null)
            {
                color.Color = text.color;
                return color;
            }

            bool success;
            if (GetColorValue != null)
            {
                color.Color = GetColorValue(out success);
                if (success)
                {
                    return color;
                }
            }
            else
            {
                color.Color = GetTextMeshProColor(out success);
                if (success)
                {
                    GetColorValue = GetTextMeshProColor;
                    return color;
                }

                color.Color = GetTextMeshColor(out success);
                if (success)
                {
                    GetColorValue = GetTextMeshColor;
                    return color;
                }

                color.Color = GetTextColor(out success);
                if (success)
                {
                    GetColorValue = GetTextColor;
                    return color;
                }

                GetColorValue = GetRendererColor;
            }

            return base.GetProperty(property);
        }

        public override void SetValue(InteractableThemeProperty property, int index, float percentage)
        {
            Color color = Color.Lerp(property.StartValue.Color, property.Values[index].Color, percentage);
            TextMesh mesh = Host.GetComponent<TextMesh>();

            if(SetColorValue != null)
            {
                if (SetColorValue(color))
                {
                    return;
                }
            }
            else
            {
                if (SetTextMeshProColor(color))
                {
                    SetColorValue = SetTextMeshProColor;
                    return;
                }

                if (SetTextMeshColor(color))
                {
                    SetColorValue = SetTextMeshColor;
                    return;
                }

                if (SetTextColor(color))
                {
                    SetColorValue = SetTextColor;
                    return;
                }

                SetColorValue = SetRendererColor;
            }

            base.SetValue(property, index, percentage);

        }

        protected bool SetTextColor(Color color)
        {
            Text text = Host.GetComponent<Text>();
            if (text != null)
            {
                text.color = color;
                return true;
            }
            return false;
        }

        protected Color GetTextColor(out bool success)
        {
            Color color = Color.white;
            Text text = Host.GetComponent<Text>();
            if (text != null)
            {
                color = text.color;
                success = true;
                return color;
            }
            success = false;
            return color;
        }

        protected bool SetTextMeshColor(Color color)
        {
            TextMesh mesh = Host.GetComponent<TextMesh>();
            if (mesh != null)
            {
                mesh.color = color;
                return true;
            }
            return false;
        }

        protected Color GetTextMeshColor(out bool success)
        {
            Color color = Color.white;
            TextMesh mesh = Host.GetComponent<TextMesh>();
            if (mesh != null)
            {
                color = mesh.color;
                success = true;
                return color;
            }
            success = false;
            return color;
        }

        protected bool SetTextMeshProColor(Color color)
        {
#if TMP_PRESENT
            TextMeshPro tmp = Host.GetComponent<TextMeshPro>();
            if (tmp != null)
            {
                tmp.color = color;
                return true;
            }
#endif // TMP_PRESENT
            return false;
        }

        protected Color GetTextMeshProColor(out bool success)
        {
            Color color = Color.white;
#if TMP_PRESENT
            TextMeshPro tmp = Host.GetComponent<TextMeshPro>();
            if (tmp != null)
            {
                color = tmp.color;
                success = true;
                return color;
            }
#endif // TMP_PRESENT
            success = false;
            return color;
        }

        protected bool SetRendererColor(Color color)
        {
            // placeholder
            return false;
        }
        
        protected Color GetRendererColor(out bool success)
        {
            success = false;
            return Color.white;
        }
    }
}
