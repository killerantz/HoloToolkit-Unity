// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Text;

namespace Fonts
{
	public class FontList
	{
		private static FontList Instance = new FontList();

		private string mSelectedObjectError = "Please select a game object in the scene with a TextMesh or Text component and try again.";

        public static bool ShowSegoeFonts = false;
        
        [MenuItem("Fonts/Generate Fonts Menu", false, 1000)]
        public static void CreateFontMenu()
        {
            // see if the GenerateFontMenu.cs exists in the project.
            string menuPath = "FontSets";
            FontSetManager manager = FontSetManager.Instance();

            if(manager == null)
            {
                Debug.Log("Ending because no manager was found!!!!!");
                return;
            }

            string[] menu = AssetDatabase.FindAssets("GeneratedFontsMenu");

            if (menu.Length > 0)
            {
                // if so, get the location and replace it
                menuPath = FontUtils.GetCleanPath(AssetDatabase.GUIDToAssetPath(menu[0]));
                menuPath = FontUtils.GetCleanEditorPath(menuPath);

                if (menu.Length > 1)
                {
                    Debug.LogError("There should only be one ../Editor/GeneratedFontsMenu.cs, please remove any extras in the project.");
                    Debug.LogError("The updated menu script is going here: " + FontUtils.Assets() + menuPath);
                }
            }
            
            bool defaultFontSetFound = false;
            // check for DefaultFontSet
            string[] fontSetGuids = AssetDatabase.FindAssets("t:FontSet");
            string fontSetPath = "FontSets";
            if (fontSetGuids.Length > 0)
            {
                string newPath = FontUtils.GetCleanPath(AssetDatabase.GUIDToAssetPath(fontSetGuids[0]));
                if (newPath.IndexOf(".Examples") == -1)
                {
                    fontSetPath = newPath;
                }

                for (int i = 0; i < fontSetGuids.Length; i++)
                {
                    string fPath = AssetDatabase.GUIDToAssetPath(fontSetGuids[i]);
                    if (fPath.IndexOf("/" + FontUtils.DefaultFontSetName + ".asset") > -1)
                    {
                        defaultFontSetFound = true;
                        break;
                    }

                }
            }

            bool defaultIconSetFound = false;
            // check for DefaultIconSet
            string[] iconSetGuids = AssetDatabase.FindAssets("t:IconSet");
            string iconSetPath = "FontSets";
            if (iconSetGuids.Length > 0)
            {
                string newPath = FontUtils.GetCleanPath(AssetDatabase.GUIDToAssetPath(iconSetGuids[0]));
                if (newPath.IndexOf(".Examples") == -1)
                {
                    iconSetPath = newPath;
                }

                for (int i = 0; i < iconSetGuids.Length; i++)
                {
                    string iPath = AssetDatabase.GUIDToAssetPath(iconSetGuids[i]);
                    if (iPath.IndexOf("/" + FontUtils.DefaultIconSetName + ".asset") > -1)
                    {
                        defaultIconSetFound = true;
                        break;
                    }

                }
            }

            // if none exists, then create them in the Assets folder.
            // if one exists, create everything else at one of the locations
            if (menu.Length < 1)
            {
                if (fontSetGuids.Length > 0)
                {
                    menuPath = fontSetPath;
                }
            }

            // create the default IconSet
            if (iconSetGuids.Length < 1 || !defaultIconSetFound)
            {
                if (manager != null)
                {
                    List<FontData> dataList = new List<FontData>(manager.IconList);
                    FontUtils.CreateIconSets(iconSetPath, dataList);
                }
            }

            if (fontSetGuids.Length < 1 || !defaultFontSetFound) {
                // create the default FontSet
                if (manager != null)
                {
                    List<FontData> dataList = new List<FontData>(manager.FontList);
                    FontUtils.CreateFontSets(fontSetPath, dataList);
                }
                // get all the fontsets
                fontSetGuids = AssetDatabase.FindAssets("t:FontSet");
            }

            // create the menu
            FontUtils.GenerateFontsMenu(fontSetGuids, menuPath);
            
        }
        
        protected static FontSetData GetFontSetByGuid(string guid)
        {
            string[] fontSetGuids = AssetDatabase.FindAssets("t:FontSet");
            
            for (int i = 0; i < fontSetGuids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(fontSetGuids[i]);
                FontSet fontSet = (FontSet)AssetDatabase.LoadAssetAtPath(assetPath, typeof(FontSet));

                for (int f = 0; f < fontSet.Data.Count; f++)
                {
                    FontSetData data = fontSet.Data[f];
                    if (data.Guid == guid)
                    {
                        return data;
                    }
                }
            }

            return new FontSetData();
        }

        public static void ApplyFont(int id)
		{
            FontSetData data = GetFontSetByGuid(id + FontUtils.HashSuffix());

            if (string.IsNullOrEmpty(data.Guid))
            {

                Debug.Log("Error: No FontSet found! ");

                EditorUtility.DisplayDialog(
                        "Error: The FontSet was not found!",
                        "The FontSet for this font was not found, make sure it was not deleted. Please update the FontSet and select the Generate Fonts Menu option in the Fonts menu.",
                        "OK");
                return;
            }

            // get the selected object in the scene
            GameObject[] selectedObjects = Selection.gameObjects;
            bool wrongObjectSelected = false;
            bool noMaterial = false;
            bool noFont = false;
            
            if (selectedObjects.Length > 0)
			{
                for (int i = 0; i < selectedObjects.Length; i++)
                {
                    if (wrongObjectSelected || noMaterial || noFont)
                        break;

                    // make sure it's a valid Text or TextMesh and if the correct font is assigned
                    TextMesh mesh = selectedObjects[i].GetComponent<TextMesh>();
                    Text text = selectedObjects[i].GetComponent<Text>();

                    // set the font properties
                    Material material = data.Material;
                    Font font = data.Font;

                    noMaterial = material == null;
                    noFont = font == null;

                    if (noMaterial == false && noFont == false)
                    {
                        //assign icon code
                        if (mesh != null)
                        {
                            Instance.SetFontAndMaterial(null, mesh, font, material);
                        }
                        else if (text != null)
                        {
                            Instance.SetFontAndMaterial(text, null, font, material);
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
                Debug.Log("Selection Error: No Object with TextMesh Selected!");

				EditorUtility.DisplayDialog(
						"Selection Error: No Object with TextMesh Selected",
						Instance.mSelectedObjectError,
						"OK");
			}

            if (noFont)
            {
                Debug.Log("Font Error: Font not found: " + data.Font.name);

                EditorUtility.DisplayDialog(
                        "Font Error: Font not found: " + data.Font.name,
                        "The requested font was not found in the project, make sure it is assigned in the FontSet.",
                        "OK");
            }

            if (noMaterial)
            {
                Debug.Log("Material Error: Material not found: " + data.Material.name);

                EditorUtility.DisplayDialog(
                        "Material Error: Material not found: " + data.Material.name,
                        "The requested material was not found in the project, make sure it is assigned in the FontSet.",
                        "OK");
            }
        }

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
