using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HoloToolkit.Unity
{
    public enum InspectorTypes { Float, Int, String, Property, Drop, Bool }
    public class CustomInspectorField : Attribute
    {
        public InspectorTypes Type { get; set; }
        public string Label { get; set; }
    }

    [System.Serializable]
    public class InteractableReceiver
    {
        
    }
}
