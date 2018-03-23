using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloToolkit.Unity
{

    public class DefaultTheme : ThemeBase
    {

        // define what types of components can use this theme: Transform, Renderer, TextMesh, 
        // create list of values
        // get values from components
        // respond to state changes
        // length and order of states depends on states and Interactable
        // assign values to components

        // color abstract has a property setting for color

        public DefaultTheme() : base()
        {
            Types = new Type[] {typeof(Transform), typeof(TextMesh), typeof(TextMesh)};
            Name = "Default: Scale, Offset, Color";
            ThemeProperties.Add(
                new ThemeProperty()
                {
                    Name = "Scale",
                    Type = ThemePropertyValueTypes.Vector3,
                    Values = new List<ThemePropertyValue>()
                });
            ThemeProperties.Add(
                new ThemeProperty()
                {
                    Name = "Offset",
                    Type = ThemePropertyValueTypes.Vector3,
                    Values = new List<ThemePropertyValue>()
                });
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
            throw new NotImplementedException();
        }

        public override void SetValue(ThemeProperty property, int index, float percentage)
        {
            throw new NotImplementedException();
        }
    }
}
