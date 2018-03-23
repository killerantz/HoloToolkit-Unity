using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloToolkit.Unity
{
    public class ScaleTheme : ThemeBase
    {
        public ScaleTheme()
        {
            Types = new Type[] { typeof(Transform) };
            Name = "Scale Theme";
            ThemeProperties.Add(
                new ThemeProperty()
                {
                    Name = "Scale",
                    Type = ThemePropertyValueTypes.Vector3,
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
