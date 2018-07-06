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
    /// A scrollable icon viewer.
    /// </summary>
    public class IconScrollView
    {
        // scroller view position
        public Vector2 ScrollPosition = Vector2.zero;

        // array of charcodes
        protected IconData[] iconCharCodes;
        // the icon font to pass to event
        protected Font iconFont;
        // how many columns per row
        protected int columnCount = 9;
        // the width of the button
        protected float buttonWidth = 40;
        //height of the button
        protected float buttonHeight = 40;
        // the initial font size
        protected float fontSize = 20;
        // font material to pass to event
        protected Material fontMaterial;
        // based on window scale changes, we will scale the buttons
        protected float sizeMultiplier;
        // should we show all the icons or just the enabled ones?
        protected bool showAllIcons;
        //host id to help id this set compared to other sets
        protected int hostId;
        //button callback
        protected IconButtonCallback buttonCallback;

        protected Color enabledColor = new Color(1, 1, 1);
        protected Color disabledColor = new Color(1, 0.3f, 0.3f);

        public IconScrollView(Vector2 buttonSize, float scale, Vector2 scroll, Font font, float size, Material material, IconData[] charCodes, int columns, bool showAll, IconButtonCallback onClick, int id = 0)
        {
            sizeMultiplier = scale;
            buttonHeight = buttonSize.y;
            buttonWidth = buttonSize.x;
            iconFont = font;
            fontSize = size;
            fontMaterial = material;
            iconCharCodes = charCodes;
            columnCount = columns;
            showAllIcons = showAll;
            hostId = id;

            buttonCallback = onClick;
            ScrollPosition = scroll;

            ShowIcons();

        }

        public static IconScrollView Create(Vector2 buttonSize, float scale, Vector2 scroll, Font font, float size, Material material, IconData[] charCodes, int columns, bool showAll, IconButtonCallback onClick, int id = 0)
        {
            IconScrollView window = new IconScrollView(buttonSize, scale, scroll, font, size, material, charCodes, columns, showAll, onClick, id);
            return window;
        }

        public static List<IconData> GetCodes(string[] codes)
        {
            List<IconData> icons = new List<IconData>();
            for (int i = 0; i < codes.Length; i++)
            {
                IconData icon = new IconData();
                icon.Enabled = true;
                icon.Code = codes[i];
                icons.Add(icon);
            }
            return icons;
        }

        public static List<IconData> GetCodesFromFont(Font font)
        {
            string path = AssetDatabase.GetAssetPath(font);
            TrueTypeFontImporter fontData = (TrueTypeFontImporter)AssetImporter.GetAtPath(path);

            fontData.fontTextureCase = FontTextureCase.Unicode;
            fontData.SaveAndReimport();

            List<IconData> icons = new List<IconData>();
            CharacterInfo[] info = font.characterInfo;
            for (int i = 0; i < info.Length; i++)
            {
                IconData icon = new IconData();
                char index = (char)info[i].index;

                icon.Enabled = true;
                icon.Code = index.ToString();
                icons.Add(icon);
            }

            fontData.fontTextureCase = FontTextureCase.Dynamic;
            fontData.SaveAndReimport();

            return icons;
        }


        public static SerializedProperty GetSerializedCodesFromFont(Font font, SerializedProperty list)
        {
            string path = AssetDatabase.GetAssetPath(font);
            TrueTypeFontImporter fontData = (TrueTypeFontImporter)AssetImporter.GetAtPath(path);

            fontData.fontTextureCase = FontTextureCase.Unicode;
            fontData.SaveAndReimport();

            CharacterInfo[] info = font.characterInfo;
            for (int i = 1; i < info.Length; i++)
            {
                char index = (char)info[i].index;

                list.InsertArrayElementAtIndex(list.arraySize);
                SerializedProperty sItem = list.GetArrayElementAtIndex(list.arraySize - 1);

                SerializedProperty enabled = sItem.FindPropertyRelative("Enabled");
                enabled.boolValue = true;

                SerializedProperty sCode = sItem.FindPropertyRelative("Code");
                sCode.stringValue = index.ToString();
            }

            fontData.fontTextureCase = FontTextureCase.Dynamic;
            fontData.SaveAndReimport();

            return list;
        }

        public virtual void ShowIcons()
        {

            // set up the button settings
            IconButtonSettings settings = new IconButtonSettings();
            settings.Width = buttonWidth * sizeMultiplier;
            settings.Height = buttonHeight * sizeMultiplier;
            settings.Style = IconViewerButton.GetButtonStyle(fontSize * sizeMultiplier, iconFont, enabledColor);
            settings.CallBack = buttonCallback;
            settings.Material = fontMaterial;

            IconData[] iconArray = iconCharCodes;
            int iconArrayLength = iconCharCodes.Length;

            // we will always round up to the column count so the last row of buttons maintains the unifor size.
            int listLength = RoundUpListLength(iconArrayLength, 0);
            int rows = Mathf.CeilToInt(iconCharCodes.Length / columnCount);

            List<GUILayoutOption> options = new List<GUILayoutOption>();
            if (showAllIcons)
            {
                float height = Math.Min(buttonHeight * rows * 1.65f * sizeMultiplier, buttonHeight * 8 * 1.65f * sizeMultiplier);
                options.Add(GUILayout.Height(height));
            }

            ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition, false, false, options.ToArray());

            EditorGUILayout.BeginVertical("Box");

            // figure out how many icons there are
            int colCount = 0;
            int removed = 0;

            // set up the first row
            GUILayout.BeginHorizontal();

            for (int i = 0; i < listLength; i++)
            {
                // add buttons
                if (i < iconArrayLength)
                {
                    if (iconArray[i].Enabled || showAllIcons)
                    {
                        settings.Style.normal.textColor = iconArray[i].Enabled ? enabledColor : disabledColor;
                        settings.index = i;
                        settings.hostIndex = hostId;
                        IconViewerButton.Create(iconArray[i].Code, i, settings);
                        colCount += 1;
                    }
                    else
                    {
                        removed++;
                        listLength = RoundUpListLength(iconArrayLength, removed % columnCount);

                    }
                }
                else
                {
                    IconViewerButton.Create("", i, settings);
                    colCount += 1;
                }

                // end the row if the column count is reached
                if (colCount >= columnCount)
                {
                    GUILayout.EndHorizontal();
                    if (i < listLength - 1)
                    {
                        GUILayout.BeginHorizontal();
                    }

                    colCount = 0;
                }
            }

            // end the row some how continued past zero (was not rounded properly)
            if (colCount > 0)
            {
                GUILayout.EndHorizontal();
            }

            // end scroll view
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        protected int RoundUpListLength(int length, int removed)
        {
            int remainder = (length - removed) % columnCount;

            if (remainder > 0)
            {
                return length + (columnCount - remainder);
            }

            return length;
        }
    }
}

