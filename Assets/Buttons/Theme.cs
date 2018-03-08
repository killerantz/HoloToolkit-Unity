using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloToolkit.Unity
{
    [CreateAssetMenu(fileName = "Theme", menuName = "Themes/Theme", order = 1)]
    public class Theme : ScriptableObject
    {
        public string Name;

        public struct Vector3States
        {
            public Vector3 Default;
            public Vector3 Focus;
            public Vector3 Press;
            public Vector3 Disabled;
        }

        public struct ColorStates
        {
            public Color Default;
            public Color Focus;
            public Color Press;
            public Color Disabled;
        }

        public struct FloatStates
        {
            public float Default;
            public float Focus;
            public float Press;
            public float Disabled;
        }

        protected void SetVector3States()
        {

        }
    }
}
