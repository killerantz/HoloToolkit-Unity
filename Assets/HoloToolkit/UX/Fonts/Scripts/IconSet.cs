// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fonts
{
    [System.Serializable]
    public struct IconData
    {
        public string Code;
        public bool Enabled;
    }

    [System.Serializable]
    public class IconSetData
    {
        public bool Enabled;
        public string Name;
        public Material Material;
        public Font Font;
        public List<IconData> CharCodes = new List<IconData>();
        public string Guid;

        public void SetCharCodes(List<IconData> data)
        {
            CharCodes = data;
        }

        public void SetCharCodeEnabled(int index, bool enabled)
        {
            IconData data = CharCodes[index];
            data.Enabled = enabled;
            CharCodes[index] = data;
        }

        public bool getCharCodeEnabled(int index)
        {
            return CharCodes[index].Enabled;
        }
    }

    [CreateAssetMenu(fileName = "IconSet", menuName = "Fonts/IconSet", order = 1001)]
    public class IconSet : ScriptableObject
    {
        [HideInInspector]
        public List<IconSetData> Data = new List<IconSetData>();

        private int stackSize = 10;
        
    }
}