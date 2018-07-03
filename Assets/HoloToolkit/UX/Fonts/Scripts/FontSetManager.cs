// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Fonts
{
    [System.Serializable]
    public class FontData
    {
        public Font Font;
        public string Name;
        public List<IconData> IconData;
    }

    //[CreateAssetMenu(fileName = "FontSetManager", menuName = "Fonts/FontSetManager", order = 100)]
    public class FontSetManager : ScriptableObject
    {
        public FontSet FontSet;

        public IconSet IconSet;

        public List<FontData> FontList = new List<Fonts.FontData>();

        public List<FontData> IconList = new List<Fonts.FontData>();

        private static FontSetManager instance = null;

        public static FontSetManager Instance()
        {
            if (instance == null)
            {
                FontSetManager[] all = Resources.LoadAll<FontSetManager>("");

                if (all.Length > 1)
                {
                    Debug.LogError("There should only be one Fonts/Resources/FontSetManager in the project. please remove extra instances or extra versions of Packages/Fonts.");
                }
                else if(all.Length < 1)
                {
                    Debug.LogError("Fonts/Resourcecs/FontSetManager cannot be found and is required for this function, please reload the Fonts package.");
                }
                else
                {
                    instance = all[0];
                }
            }

            return instance;
        }

    }
}
