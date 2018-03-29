using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloToolkit.Unity
{
    [System.Serializable]
    public struct ThemePropertySettings
    {
        public string Name;
        public Type Type;
        public ThemeBase Theme;
        public List<ThemeProperty> Properties;
        public EaseSettings Easing;
        public bool IsValid;
    }

    [CreateAssetMenu(fileName = "Theme", menuName = "Interactable/Theme", order = 1)]
    public class Theme : ScriptableObject
    {
        public string Name;
        public List<ThemePropertySettings> Settings;
    }
}
