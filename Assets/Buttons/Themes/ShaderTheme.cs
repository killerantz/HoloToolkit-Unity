using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HoloToolkit.Unity
{
    public class ShaderTheme : ThemeBase
    {
        public ShaderTheme() : base()
        {
            Types = new Type[] { typeof(Renderer)};
            Name = "Shader Float";
            ThemeProperties.Add(
                new ThemeProperty()
                {
                    Name = "Offset",
                    Type = ThemePropertyValueTypes.ShaderFloat,
                    Values = new List<ThemePropertyValue>()
                });
        }

        public override void SetValue(ThemeProperty property, int index, float percentage)
        {
            if (Host == null)
                return;

            Renderer renderer = Host.GetComponent<Renderer>();
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            switch (property.Type)
            {
                case ThemePropertyValueTypes.Color:
                    Color newColor = Color.Lerp(property.StartValue.Color, property.Values[index].Color, percentage);
                    block.SetColor(property.PropId, newColor);
                    break;
                case ThemePropertyValueTypes.ShaderFloat:
                    float newValue = LerpFloat(property.StartValue.Float, property.Values[index].Float, percentage);
                    block.SetFloat(property.PropId, newValue);
                    break;
                default:
                    break;
            }
            
            renderer.SetPropertyBlock(block);
        }

        public override ThemePropertyValue GetProperty(ThemeProperty property)
        {
            if (Host == null)
                return new ThemePropertyValue();

            Renderer renderer = Host.GetComponent<Renderer>();
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            
            ThemePropertyValue start = new ThemePropertyValue();

            switch (property.Type)
            {
                case ThemePropertyValueTypes.Color:
                    start.Color = block.GetVector(property.PropId);
                    break;
                case ThemePropertyValueTypes.ShaderFloat:
                    start.Float = block.GetFloat(property.PropId);
                    break;
                default:
                    break;
            }

            return start;
        }
    }
}
