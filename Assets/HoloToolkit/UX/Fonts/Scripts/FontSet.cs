// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fonts
{
    [System.Serializable]
    public struct FontSetData
    {
        public string Name;
        public Material Material;
        public Font Font;
        public bool Enabled;
        public string Guid;
    }

    [CreateAssetMenu(fileName = "FontSet", menuName = "Fonts/FontSet", order = 1001)]
    public class FontSet : ScriptableObject
    {
        [HideInInspector]
        public bool Enabled = true;

        [HideInInspector]
        public List<FontSetData> Data = new List<FontSetData>();
    }
}
