// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace HoloToolkit.Unity.UX
{

    /// <summary>
    /// Button settings for the icon symbol
    /// </summary>
    public struct IconButtonSettings
    {
        public float Width;
        public float Height;
        public GUIStyle Style;
        public IconButtonCallback CallBack;
        public Material Material;
        public int index;
        public int hostIndex;
    }

    // a callback function if needed
    public delegate void IconButtonCallback(int index, string label, IconButtonSettings settings);

    /// <summary>
    /// The button used to assign an icon.
    /// </summary>
    public class IconViewerButton
    {
        // icon code
        public string Label;

        // icon code index
        public int Index;

        // button settings
        public IconButtonSettings Settings;

        /// <summary>
        /// IconViewerButton constructor, assigns values and sets up button event.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="index"></param>
        /// <param name="settings"></param>
        public IconViewerButton(string label, int index, IconButtonSettings settings)
        {
            Label = label;
            Index = index;
            Settings = settings;

            Init();
        }

        public static void Create(string label, int index, IconButtonSettings settings)
        {
            new IconViewerButton(label, index, settings);
        }

        /// <summary>
        /// Button structure and result
        /// </summary>
        private void Init()
        {
            if (GUILayout.Button(Label, Settings.Style, GUILayout.MinWidth(Settings.Width), GUILayout.Height(Settings.Height)))
            {
                // don't show a glyph or perform a function if the code is not valid
                if (Label == "")
                {
                    return;
                }

                Settings.CallBack(Index, Label, Settings);
            }
        }

        public static GUIStyle GetButtonStyle(float size, Font font, Color color)
        {
            GUIStyle ButtonStyle;
            ButtonStyle = new GUIStyle(GUI.skin.button);
            ButtonStyle.font = font;
            ButtonStyle.fontSize = (int)size;
            ButtonStyle.normal.textColor = color;
            ButtonStyle.alignment = TextAnchor.MiddleCenter;
            return ButtonStyle;
        }
    }
}

