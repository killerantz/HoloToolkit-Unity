using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HoloToolkit.Unity
{
    public class ColorTheme : ShaderTheme
    {

        public ColorTheme()
        {
            Types = new Type[] {typeof(Renderer), typeof(TextMesh), typeof(Text)};
            Name = "Color Theme";
            ThemeProperties = new List<ThemeProperty>();
            ThemeProperties.Add(
                new ThemeProperty()
                {
                    Name = "Color",
                    Type = ThemePropertyValueTypes.Color,
                    Values = new List<ThemePropertyValue>()
                });
        }

        public override ThemePropertyValue GetProperty(ThemeProperty property)
        {
            throw new System.NotImplementedException();
        }

        public override void SetValue(ThemeProperty property, int index, float percentage)
        {
            throw new System.NotImplementedException();
        }
    }
}
