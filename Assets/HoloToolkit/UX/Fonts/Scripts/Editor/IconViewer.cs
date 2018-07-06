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
    /// An Editor viewer of icon symbols that can be applyed to text objects.
    /// </summary>
    public class IconViewer : EditorWindow
    {
        // long or official name of the font
        private static string fontName;
        // file name in Unity of the font file
        private static string fileName;
        // array of charcodes
        private IconData[] iconCharCodes;
        // instructions that show up in the viewer window
        private string instructions = "Select a Text or TextMesh Component in the Scene then choose an Icon. This icon requires the font ";
        private string instructionsCont = " to be imported into your project and the font assigned to the component.";
        private Font iconFont;
        // how many columns per row
        private int columnCount = 9;
        // the width of the button
        private float buttonWidth = 40;
        //height of the button
        private float buttonHeight = 40;
        // the initial font size
        private float fontSize = 20;
        // the initial width of the window, based on 9 40x40 buttons side by side with a few pixel gutter
        private float baseWidth = 420;
        // index of currently selected icon
        private int selectedIndex = 0;
        // the cached index, so we can tell when the selection changes
        private int cachedIndex = -1;
        // the font name list for the drop down menu
        private string[] fontOptions;
        // the data set of symbol fonts and char codes
        //private IconViewerCodes.IconCodeData[] iconCodes;
        // the scroll position of the window
        private Vector2 scrollPosition = Vector2.zero;

        private string notSelected = "Selected item is not a TextMesh or Text object.";
        private string notSameFont = "font will be applied to selected item.";
        private string selectedObjectError = "Please select a game object in the scene with a TextMesh or Text component and try again.";

        private Material fontMaterial;

        private List<IconSetData> setDataList;

        /// <summary>
        /// Load the font and set the currently selected font properties
        /// </summary>
        public virtual void FontSelection()
        {
            string[] iconSetGuids = AssetDatabase.FindAssets("t:IconSet");
            setDataList = new List<IconSetData>();
            List<string> options = new List<string>();

            for (int i = 0; i < iconSetGuids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(iconSetGuids[i]);
                IconSet iconSet = (IconSet)AssetDatabase.LoadAssetAtPath(assetPath, typeof(IconSet));
                for (int f = 0; f < iconSet.Data.Count; f++)
                {
                    if (iconSet.Data[f].Enabled && iconSet.Data[f].Material != null && iconSet.Data[f].Font != null)
                    {
                        options.Add(iconSet.Data[f].Name);
                        setDataList.Add(iconSet.Data[f]);
                    }
                }
            }

            // build the dropdown list
            fontOptions = options.ToArray();

            if (selectedIndex > setDataList.Count)
            {
                EditorUtility.DisplayDialog(
                           "Make sure at least one IconSet is enabled",
                           "Icon Sets Not Found",
                           "OK");

                return;
            }

            // set the font properties
            fontName = setDataList[selectedIndex].Name;

            //load the selected font
            iconFont = setDataList[selectedIndex].Font;

            iconCharCodes = setDataList[selectedIndex].CharCodes.ToArray();
            fontMaterial = setDataList[selectedIndex].Material;

            scrollPosition = new Vector2();

            // cache the current selection
            cachedIndex = selectedIndex;
        }

        /// <summary>
        /// updates every frame
        /// </summary>
        void OnGUI()
        {
            // get the current size of the window and figure out the scale amount - only enlarges as the window is made wider.
            float sizeMultiplier = EditorGUIUtility.currentViewWidth / baseWidth;
            if (sizeMultiplier < 1)
            {
                sizeMultiplier = 1;
            }

            GUIStyle popupStyle = new GUIStyle(EditorStyles.popup);
            popupStyle.margin = new RectOffset(0, 0, 0, 5);
            popupStyle.normal.textColor = Color.white;

            bool hasObject = HasTextObject();
            bool hasFont = false;

            GUIStyle toolbarStyle = new GUIStyle(EditorStyles.toolbar);
            toolbarStyle.fixedHeight = hasObject ? 80 : 100;

            if (hasObject)
            {
                hasFont = HasSameFont(iconFont);
                if (!hasFont)
                {
                    toolbarStyle.fixedHeight = 100;
                }
            }

            // set up the drop-down list area
            EditorGUILayout.BeginVertical(toolbarStyle);
            selectedIndex = EditorGUI.Popup(new Rect(0, 5, position.width, 20), "Choose A Font: ", selectedIndex, fontOptions, popupStyle);

            if (cachedIndex != selectedIndex)
            {
                FontSelection();
            }

            GUILayout.Space(40);

            // set up the instructions
            GUIStyle infoStyle = new GUIStyle();
            infoStyle.normal.textColor = Color.white;
            infoStyle.wordWrap = true;

            GUIContent infoContent = new GUIContent(instructions + fontName + instructionsCont);
            EditorGUILayout.LabelField(infoContent.text, infoStyle, GUILayout.ExpandHeight(true));
            
            if (!hasObject)
            {
                DisplayStatusMessage(notSelected, true);
            }
            else if (!hasFont)
            {
                DisplayStatusMessage("The " + fontName + " " + notSameFont, false);
            }

            EditorGUILayout.EndVertical();

            IconScrollView window = IconScrollView.Create(new Vector2(buttonWidth, buttonHeight), sizeMultiplier, scrollPosition, iconFont, fontSize, fontMaterial, iconCharCodes, columnCount, false, ButtonEvent);
            scrollPosition = window.ScrollPosition;
        }

        protected static void DisplayStatusMessage(string message, bool isAlert)
        {
            GUIContent alertContent = new GUIContent("");
            GUIStyle alertStyle = new GUIStyle();
            alertStyle.normal.textColor = isAlert ? new Color(1f, 0.55f, 0.5f) : new Color(1f, 0.85f, 0.6f);
            alertStyle.wordWrap = true;
            alertStyle.margin = new RectOffset(0, 0, 5, 5);

            alertContent = new GUIContent(message);
            EditorGUILayout.LabelField(alertContent.text, alertStyle, GUILayout.ExpandHeight(true));
        }

        /// <summary>
        /// see if all selected items are a text or textMesh object
        /// </summary>
        /// <returns></returns>
        protected bool HasTextObject()
        {
            int hasTextCount = 0;

            GameObject[] selectedObjects = Selection.gameObjects;

            if (selectedObjects.Length > 0)
            {
                for (int i = 0; i < selectedObjects.Length; i++)
                {
                    TextMesh mesh = selectedObjects[i].GetComponent<TextMesh>();
                    Text text = selectedObjects[i].GetComponent<Text>();

                    if (mesh != null)
                    {
                        hasTextCount++;
                    }
                    else if (text != null)
                    {
                        hasTextCount++;
                    }
                }
            }
            else
            {
                return false;
            }

            return hasTextCount == selectedObjects.Length;
        }

        /// <summary>
        /// see if all selected item have the same font as the selected font
        /// </summary>
        /// <param name="font"></param>
        /// <returns></returns>
        protected bool HasSameFont(Font font)
        {
            int sameFontCount = 0;

            GameObject[] selectedObjects = Selection.gameObjects;

            if (selectedObjects.Length > 0)
            {
                for (int i = 0; i < selectedObjects.Length; i++)
                {
                    TextMesh mesh = selectedObjects[i].GetComponent<TextMesh>();
                    Text text = selectedObjects[i].GetComponent<Text>();

                    if (mesh != null)
                    {
                        if (mesh.font == font)
                        {
                            sameFontCount++;
                        }
                    }
                    else if (text != null)
                    {
                        if (text.font == font)
                        {
                            sameFontCount++;
                        }
                    }
                }
            }
            else
            {
                return false;
            }

            return sameFontCount == selectedObjects.Length;
        }

        /// <summary>
        /// A button event, if needed
        /// </summary>
        /// <param name="index"></param>
        public virtual void ButtonEvent(int index, string label, IconButtonSettings settings)
        {
            // button was clicked
            // show errors
            bool wrongObjectSelected = false;

            // get the selected object in the scene
            GameObject[] selectedObjects = Selection.gameObjects;

            if (selectedObjects.Length > 0)
            {

                // check if all the objects have the correct font
                bool usesSameFont = true;
                for (int i = 0; i < selectedObjects.Length; i++)
                {
                    if (!usesSameFont)
                        break;
                    // make sure it's a valid Text or TextMesh and if the correct font is assigned
                    TextMesh mesh = selectedObjects[i].GetComponent<TextMesh>();
                    Text text = selectedObjects[i].GetComponent<Text>();

                    //assign icon code
                    if (mesh != null)
                    {
                        usesSameFont = mesh.font == settings.Style.font;
                    }
                    else if (text != null)
                    {
                        usesSameFont = text.font == settings.Style.font;
                    }
                }

                bool applyFont = true;

                if (applyFont)
                {
                    for (int i = 0; i < selectedObjects.Length; i++)
                    {
                        if (wrongObjectSelected)
                            break;
                        // make sure it's a valid Text or TextMesh and if the correct font is assigned
                        TextMesh mesh = selectedObjects[i].GetComponent<TextMesh>();
                        Text text = selectedObjects[i].GetComponent<Text>();

                        //assign icon code
                        if (mesh != null)
                        {
                            mesh.text = label;

                            if (mesh.font != settings.Style.font)
                            {
                                SetFontAndMaterial(null, mesh, settings.Style.font, settings.Material);
                            }
                        }
                        else if (text != null)
                        {
                            text.text = label;

                            if (text.font != settings.Style.font)
                            {
                                SetFontAndMaterial(text, null, settings.Style.font, settings.Material);
                            }
                        }
                        else
                        {
                            wrongObjectSelected = true;
                        }
                    }
                }
            }
            else
            {
                wrongObjectSelected = true;
            }

            //wrong type of object selected
            if (wrongObjectSelected)
            {
                EditorUtility.DisplayDialog(
                        "Selection Error: No Object with TextMesh Selected",
                        selectedObjectError,
                        "OK");
            }
        }

        /// <summary>
        /// set the font and material for the selected objects
        /// </summary>
        /// <param name="text"></param>
        /// <param name="textMesh"></param>
        /// <param name="font"></param>
        /// <param name="material"></param>
        private void SetFontAndMaterial(Text text, TextMesh textMesh, Font font, Material material)
        {
            if (material != null)
            {
                if (text != null)
                {
                    text.font = font;
                    text.material = material;
                }
                if (textMesh != null)
                {
                    textMesh.font = font;
                    Renderer renderer = textMesh.gameObject.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = material;
                    }
                }
            }
        }
    }
}

