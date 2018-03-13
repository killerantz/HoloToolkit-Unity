using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HoloToolkit.Unity
{
    public class InspectorField : Attribute
    {
        public InspectorTypes Type { get; set; }
        public string Label { get; set; }
        public string Tooltip { get; set; }
    }
}
