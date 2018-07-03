// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Fonts
{
    public class FontUtils
    {
        public static string Assets()
        {
            return "Assets/";
        }

        public static string DirectoryAssets()
        {
            return "Assets\\";
        }

        public static string Editor()
        {
            return "/Editor";
        }

        public static string HashSuffix()
        {
            return "x00";
        }

        public const string DefaultFontSetName = "DefaultFontSet";
        public const string DefaultIconSetName = "DefaultIconSet";

        public static string GetCleanPath(string path)
        {
            string cleanPath = Path.GetDirectoryName(path);
            //remove "Assets/" from the beginning of the path
            if (cleanPath.IndexOf(Assets()) == 0 || cleanPath.IndexOf("Assets\\") == 0)
            {
                cleanPath = cleanPath.Substring(Assets().Length);
            }
            else if(cleanPath.IndexOf(DirectoryAssets()) == 0)
            {
                cleanPath = cleanPath.Substring(DirectoryAssets().Length);
            }
            return cleanPath;
        }

        public static string GetCleanEditorPath(string path)
        {
            // remove "/Editor" from the end of the path
            if (path.IndexOf(Editor()) > -1 && (path.Length == path.LastIndexOf(Editor()) + Editor().Length))
            {
                path = path.Substring(0, path.LastIndexOf(Editor()));
            }

            return path;
        }

        public static Material CreateMaterial(Object target, Font font, string name, string suffix)
        {
            Material material = new Material(Shader.Find("Fonts/FontShader3D"));
            material.mainTexture = font.material.mainTexture;

            // now save it next to the font
            string materialLocation = GetCleanPath(AssetDatabase.GetAssetPath(target));

            if (!System.IO.Directory.Exists(Application.dataPath + "/" + materialLocation + "/Materials"))
            {
                AssetDatabase.CreateFolder(Assets() + materialLocation, "Materials");
                AssetDatabase.Refresh();
            }

            name = name.Replace(" ", "");

            string file = UniqueFileName(materialLocation + "/Materials", name + suffix, ".mat");

            AssetDatabase.CreateAsset(material, Assets() + file);

            AssetDatabase.Refresh();

            Material newMaterial = (Material)AssetDatabase.LoadAssetAtPath(Assets() + file, typeof(Material));

            return newMaterial;
        }

        public static string UniqueFileName(string path, string fileName, string suffix)
        {
            string name = "/" + fileName;
            int number = 0;

            bool isNotUnique = true;
            string file = path + name + suffix;

            while (isNotUnique)
            {
                if (number > 0)
                {
                    file = path + name + number + suffix;
                }

                if (!System.IO.File.Exists(Application.dataPath + "/" + file))
                {
                    isNotUnique = false;
                }

                number++;
            }

            return file;
        }

        public static void CreateFontSets(string path, List<FontData> data)
        {
            if (!System.IO.Directory.Exists(Application.dataPath + "/" + path))
            {
                AssetDatabase.CreateFolder("Assets", path);
            }

            if (!System.IO.File.Exists(Application.dataPath + "/" + path + "/" + DefaultFontSetName + ".asset"))
            {
                //create default set
                FontSet set = ScriptableObject.CreateInstance<FontSet>();
                set.Enabled = true;
                AssetDatabase.CreateAsset(set, Assets() + path + "/" + DefaultFontSetName + ".asset");
                AssetDatabase.Refresh();
            }

            FontSet newSet = (FontSet)AssetDatabase.LoadAssetAtPath(Assets() + path + "/" + DefaultFontSetName + ".asset", typeof(FontSet));

            List<FontSetData> newData = new List<FontSetData>();
            // create materials
            for (int i = 0; i < data.Count; i++)
            {
                FontSetData set = new FontSetData();
                set.Font = data[i].Font;
                set.Name = data[i].Name;
                set.Material = CreateMaterial(newSet, set.Font, set.Name, "_FontMat");
                set.Enabled = true;

                newData.Add(set);
            }

            newSet.Data = newData;
            EditorUtility.SetDirty(newSet);
        }

        public static void CreateIconSets(string path, List<FontData> data)
        {
            if (!System.IO.Directory.Exists(Application.dataPath + "/" + path))
            {
                AssetDatabase.CreateFolder("Assets", path);
            }

            if (!System.IO.File.Exists(Application.dataPath + "/" + path + "/" + DefaultIconSetName + ".asset"))
            {
                //create default set
                IconSet set = ScriptableObject.CreateInstance<IconSet>();
                // see what char codes are possible
                AssetDatabase.CreateAsset(set, Assets() + path + "/" + DefaultIconSetName + ".asset");
                AssetDatabase.Refresh();
            }

            IconSet newSet = (IconSet)AssetDatabase.LoadAssetAtPath(Assets() + path + "/" + DefaultIconSetName + ".asset", typeof(IconSet));

            List<IconSetData> newData = new List<IconSetData>();
            // create materials
            for (int i = 0; i < data.Count; i++)
            {
                IconSetData set = new IconSetData();
                set.Font = data[i].Font;
                set.Name = data[i].Name;
                set.Material = CreateMaterial(newSet, set.Font, set.Name, "_IconMat");
                set.CharCodes = data[i].IconData;
                set.Enabled = true;

                newData.Add(set);
            }

            newSet.Data = newData;
            EditorUtility.SetDirty(newSet);

        }

        public static void GenerateFontsMenu(string[] fontSets, string path)
        {
            if (!System.IO.Directory.Exists(Application.dataPath + "/" + path + "/Editor"))
            {
                AssetDatabase.CreateFolder(Assets() + path, "Editor");
            }

            string scriptFile = Application.dataPath + "/" + path + "/Editor/GeneratedFontsMenu.cs";

            int menuCount = 0;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("// -----------------------------------=== + ===--------------------------------------");
            builder.AppendLine("//      <Auto-generated>");
            builder.AppendLine("//          This code was generated by Fonts.");
            builder.AppendLine("//");
            builder.AppendLine("//          Changes to this file may result in incorrect behavior.");
            builder.AppendLine("//          To make changes to the Fonts Menu, create and modify a FontSet then");
            builder.AppendLine("//          Use the Fonts menu to regenerate this file.");
            builder.AppendLine("//      </Auto-genterated>");
            builder.AppendLine("// -----------------------------------=== + ===--------------------------------------");
            builder.AppendLine("using UnityEngine;");
            builder.AppendLine("using UnityEditor;");
            builder.AppendLine("");
            builder.AppendLine("namespace Fonts");
            builder.AppendLine("{");
            builder.AppendLine("    public static class GeneratedFontsMenu");
            builder.AppendLine("    {");
            builder.AppendLine("");

            int seperator = 12;

            for (int i = 0; i < fontSets.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(fontSets[i]);
                FontSet fontSet = (FontSet)AssetDatabase.LoadAssetAtPath(assetPath, typeof(FontSet));
                for (int f = 0; f < fontSet.Data.Count; f++)
                {
                    FontSetData data = fontSet.Data[f];
                    data.Guid = f + "-1";

                    // check if font set is there and enabled
                    if (fontSet.Enabled && data.Enabled && data.Material != null)
                    {
                        // create new font set menus
                        builder.AppendLine("        [MenuItem(\"Fonts/" + data.Name + "\", false, + " + seperator.ToString() + ")]");
                        builder.AppendLine("        private static void FontsMenuItem" + (menuCount).ToString() + "()");
                        builder.AppendLine("        {");
                        builder.AppendLine("            FontList.ApplyFont(" + (menuCount).ToString() + ");");
                        builder.AppendLine("        }");
                        builder.AppendLine("");
                        data.Guid = menuCount + HashSuffix();
                        builder.AppendLine("");

                        // add context menu for TextMesh
                        builder.AppendLine("        [MenuItem(\"CONTEXT/TextMesh/" + data.Name + "\", false, + " + seperator.ToString() + ")]");
                        builder.AppendLine("        private static void FontsTextMeshContext" + (menuCount).ToString() + "()");
                        builder.AppendLine("        {");
                        builder.AppendLine("            FontList.ApplyFont(" + (menuCount).ToString() + ");");
                        builder.AppendLine("        }");
                        builder.AppendLine("");
                        data.Guid = menuCount + HashSuffix();
                        builder.AppendLine("");

                        // add context menu for Text
                        builder.AppendLine("        [MenuItem(\"CONTEXT/Text/" + data.Name + "\", false, + " + seperator.ToString() + ")]");
                        builder.AppendLine("        private static void FontsTextContext" + (menuCount).ToString() + "()");
                        builder.AppendLine("        {");
                        builder.AppendLine("            FontList.ApplyFont(" + (menuCount).ToString() + ");");
                        builder.AppendLine("        }");
                        builder.AppendLine("");
                        data.Guid = menuCount + HashSuffix();

                        menuCount++;
                    }

                    fontSet.Data[f] = data;
                }
                
                if (fontSet.Enabled)
                {
                    seperator += 11;
                }
            }

            builder.AppendLine("        [MenuItem(\"Fonts/Icon Viewer\", false, 0)]");
            builder.AppendLine("        private static void ShowWindow()");
            builder.AppendLine("        {");
            builder.AppendLine("            //Show existing window instance. If one doesn't exist, make one.");
            builder.AppendLine("            IconViewer window = (IconViewer)EditorWindow.GetWindow(typeof(IconViewer), true, \"Icon Viewer\");");
            builder.AppendLine("");
            builder.AppendLine("            // create the drop-down list of font options and load the default font");
            builder.AppendLine("            window.FontSelection();");
            builder.AppendLine("        }");
            builder.AppendLine("");

            builder.AppendLine("");
            builder.AppendLine("    }");
            builder.AppendLine("}");

            if (System.IO.File.Exists(scriptFile))
            {
                System.IO.File.Delete(scriptFile);
            }
            System.IO.File.WriteAllText(scriptFile, builder.ToString(), System.Text.Encoding.UTF8);
            AssetDatabase.ImportAsset(Assets() + path + "/Editor/GeneratedFontsMenu.cs");
        }
    }
}
