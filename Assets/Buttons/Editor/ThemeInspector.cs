using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace HoloToolkit.Unity
{
    [CustomEditor(typeof(Theme))]
    public class ThemeInspector : InteractableInspector
    {
        private SerializedProperty settings;

        protected override void OnEnable()
        {
            listSettings = new List<InteractableInspector.ListSettings>();

            settings = serializedObject.FindProperty("Settings");
            AdjustListSettings(settings.arraySize);
            
            SetupThemeOptions();
        }


        public override void OnInspectorGUI()
        {
            // TODO: !!!!! need access to a game object to get shader info
            // TODO: !!!!! need access to states to get state info
            // TODO: !!!!! need to make sure we refresh the shader list when the target changes
            // TODO: !!!!! neet to get shader props, use default if one has not been set.

            //base.OnInspectorGUI();
            serializedObject.Update();

            if (settings.arraySize < 1)
            {
                AddThemeProperty(new int[] { 0 });
            }

            RenderThemeSettings(settings, serializedObject, themeOptions, null, new int[] { 0, 0, 0 });
            
            RemoveButton(new GUIContent("+", "Add Theme Property"), new int[] { 0 }, AddThemeProperty);
            // get list of all the properties from the themes

            RenderThemeStates(settings, GetStates(), 30);

            serializedObject.ApplyModifiedProperties();
        }

        protected override void RemoveThemeProperty(int[] arr)
        {
            int index = arr[0];
            settings.DeleteArrayElementAtIndex(index);
        }

        protected override State[] GetStates()
        {
            // TODO: make sure we are getting current states that were saved and not overwrite states
            InteractableStates states = new InteractableStates(InteractableStates.Default);
            return states.GetStates();
        }


        /*
        

        private void AdjustListSettings(int count)
        {
            int diff = count - listSettings.Count;
            if (diff > 0)
            {
                for (int i = 0; i < diff; i++)
                {
                    listSettings.Add(new InteractableInspector.ListSettings() { Show = false, Scroll = new Vector2() });
                }
            }
            else if (diff < 0)
            {
                int removeCnt = 0;
                for (int i = listSettings.Count - 1; i > -1; i--)
                {
                    if (removeCnt > diff)
                    {
                        listSettings.RemoveAt(i);
                        removeCnt--;
                    }
                }
            }
        }

        private void AddThemeProperty(int[] arr)
        {
            int index = arr[0];
            
            settings.InsertArrayElementAtIndex(settings.arraySize);

            SerializedProperty settingsItem = settings.GetArrayElementAtIndex(settings.arraySize - 1);
            SerializedProperty className = settingsItem.FindPropertyRelative("Name");
            if (settings.arraySize == 1)
            {

                className.stringValue = "ScaleOffsetColorTheme";
            }
            else
            {
                className.stringValue = themeOptions[0];
            }

            SerializedProperty easing = settingsItem.FindPropertyRelative("Easing");

            SerializedProperty time = easing.FindPropertyRelative("LerpTime");
            SerializedProperty curve = easing.FindPropertyRelative("Curve");
            time.floatValue = 0.5f;
            curve.animationCurveValue = AnimationCurve.Linear(0, 1, 1, 1);

            ChangeThemeProperty(settings.arraySize - 1, true);
        }

        private void RemoveThemeProperty(int[] arr)
        {
            int index = arr[0];
            
            settings.DeleteArrayElementAtIndex(index);

        }

        private void ChangeThemeProperty(int index, bool isNew = false)
        {
            SerializedProperty settingsItem = settings.GetArrayElementAtIndex(index);

            SerializedProperty className = settingsItem.FindPropertyRelative("Name");

            // get class value types
            if (!String.IsNullOrEmpty(className.stringValue))
            {
                int propIndex = ReverseLookup(className.stringValue, themeOptions);
                GameObject gameObject = null;// (GameObject)target.objectReferenceValue;

                ThemeBase themeBase = (ThemeBase)Activator.CreateInstance(themeTypes[propIndex], gameObject);

                // does this object have the right component types
                SerializedProperty isValid = settingsItem.FindPropertyRelative("IsValid");
                bool valid = false;

                bool hasText = false;
                bool hasRenderer = false;

                if (gameObject != null)
                {
                    for (int i = 0; i < themeBase.Types.Length; i++)
                    {
                        Type type = themeBase.Types[i];
                        if (gameObject.gameObject.GetComponent(type))
                        {
                            if (type == typeof(TextMesh) || type == typeof(Text))
                            {
                                hasText = true;
                            }

                            if (type == typeof(Renderer))
                            {
                                hasRenderer = true;
                            }

                            valid = true;
                        }
                    }
                }

                isValid.boolValue = valid;

                // setup the values
                // get the state names

                List<ThemeProperty> properties = themeBase.ThemeProperties;

                SerializedProperty sProps = settingsItem.FindPropertyRelative("Properties");
                sProps.ClearArray();

                for (int i = 0; i < properties.Count; i++)
                {
                    sProps.InsertArrayElementAtIndex(sProps.arraySize);
                    SerializedProperty item = sProps.GetArrayElementAtIndex(sProps.arraySize - 1);

                    SerializedProperty name = item.FindPropertyRelative("Name");
                    SerializedProperty type = item.FindPropertyRelative("Type");
                    SerializedProperty values = item.FindPropertyRelative("Values");
                    if (isNew)
                    {
                        values.ClearArray();
                    }

                    name.stringValue = properties[i].Name;
                    type.intValue = (int)properties[i].Type;
                    
                    State[] states = GetStates();
                    int valueCount = states.Length;
                    //! can I find out if this has been initiated so I only set defaults first time?
                    for (int j = 0; j < valueCount; j++)
                    {
                        values.InsertArrayElementAtIndex(values.arraySize);
                        SerializedProperty valueItem = values.GetArrayElementAtIndex(values.arraySize - 1);
                        SerializedProperty valueName = valueItem.FindPropertyRelative("Name");
                        valueName.stringValue = states[j].Name;

                        if (isNew)
                        {
                            SerializedProperty color = valueItem.FindPropertyRelative("Color");
                            color.colorValue = Color.white;
                        }
                    }

                    List<ShaderPropertyType> shaderPropFilter = new List<ShaderPropertyType>();
                    // do we need a propId?
                    if (properties[i].Type == ThemePropertyValueTypes.Color)
                    {
                        if (!hasText && hasRenderer)
                        {
                            shaderPropFilter.Add(ShaderPropertyType.Color);
                        }
                        else if (!hasText && !hasRenderer)
                        {
                            valid = false;
                        }
                    }

                    if (properties[i].Type == ThemePropertyValueTypes.ShaderFloat || properties[i].Type == ThemePropertyValueTypes.shaderRange)
                    {
                        if (hasRenderer)
                        {
                            shaderPropFilter.Add(ShaderPropertyType.Float);
                            shaderPropFilter.Add(ShaderPropertyType.Range);
                        }
                        else
                        {
                            valid = false;
                        }
                    }

                    SerializedProperty propId = item.FindPropertyRelative("PropId");
                    propId.intValue = 0;

                    SerializedProperty shaderList = item.FindPropertyRelative("ShaderOptions");
                    SerializedProperty shaderNames = item.FindPropertyRelative("ShaderOptionNames");

                    shaderList.ClearArray();
                    shaderNames.ClearArray();

                    if (valid && shaderPropFilter.Count > 0)
                    {
                        ShaderProperties[] shaderProps = GetShaderProperties(gameObject.gameObject.GetComponent<Renderer>(), shaderPropFilter.ToArray());
                        for (int n = 0; n < shaderProps.Length; n++)
                        {
                            shaderList.InsertArrayElementAtIndex(shaderList.arraySize);
                            SerializedProperty shaderListItem = shaderList.GetArrayElementAtIndex(shaderList.arraySize - 1);
                            SerializedProperty shaderListName = shaderListItem.FindPropertyRelative("Name");
                            SerializedProperty shaderListType = shaderListItem.FindPropertyRelative("Type");
                            SerializedProperty shaderListRange = shaderListItem.FindPropertyRelative("Range");

                            shaderListName.stringValue = shaderProps[n].Name;
                            shaderListType.intValue = (int)shaderProps[n].Type;
                            shaderListRange.vector2Value = shaderProps[n].Range;

                            shaderNames.InsertArrayElementAtIndex(shaderNames.arraySize);
                            SerializedProperty names = shaderNames.GetArrayElementAtIndex(shaderNames.arraySize - 1);
                            names.stringValue = shaderProps[n].Name;
                        }

                    }
                }

                if (!valid)
                {
                    isValid.boolValue = false;
                }
            }

        }

        private static void RenderStateSettings(SerializedProperty settings, State[] states, int margin)
        {

            GUIStyle box = new GUIStyle(GUI.skin.box);
            box.margin.left = margin;
            EditorGUILayout.BeginVertical(box);
            for (int n = 0; n < states.Length; n++)
            {

                DrawLabel(states[n].Name, 12, titleColor);

                EditorGUI.indentLevel = indentOnSectionStart + 1;

                for (int j = 0; j < settings.arraySize; j++)
                {
                    SerializedProperty settingsItem = settings.GetArrayElementAtIndex(j);
                    SerializedProperty className = settingsItem.FindPropertyRelative("Name");

                    SerializedProperty properties = settingsItem.FindPropertyRelative("Properties");

                    for (int i = 0; i < properties.arraySize; i++)
                    {
                        SerializedProperty propertyItem = properties.GetArrayElementAtIndex(i);

                        SerializedProperty name = propertyItem.FindPropertyRelative("Name");
                        SerializedProperty type = propertyItem.FindPropertyRelative("Type");
                        SerializedProperty values = propertyItem.FindPropertyRelative("Values");
                        SerializedProperty propId = propertyItem.FindPropertyRelative("PropId");
                        SerializedProperty options = propertyItem.FindPropertyRelative("ShaderOptions");
                        SerializedProperty names = propertyItem.FindPropertyRelative("ShaderOptionNames");

                        if (n >= values.arraySize)
                        {
                            continue;
                        }

                        SerializedProperty item = values.GetArrayElementAtIndex(n);
                        SerializedProperty floatValue = item.FindPropertyRelative("Float");
                        SerializedProperty vector2Value = item.FindPropertyRelative("Vector2");

                        switch ((ThemePropertyValueTypes)type.intValue)
                        {
                            case ThemePropertyValueTypes.Float:
                                floatValue.floatValue = EditorGUILayout.FloatField(new GUIContent(name.stringValue, ""), floatValue.floatValue);
                                break;
                            case ThemePropertyValueTypes.Int:
                                SerializedProperty intValue = item.FindPropertyRelative("Int");
                                intValue.intValue = EditorGUILayout.IntField(new GUIContent(name.stringValue, ""), intValue.intValue);
                                break;
                            case ThemePropertyValueTypes.Color:
                                SerializedProperty colorValue = item.FindPropertyRelative("Color");
                                colorValue.colorValue = EditorGUILayout.ColorField(new GUIContent(name.stringValue, ""), colorValue.colorValue);
                                break;
                            case ThemePropertyValueTypes.ShaderFloat:
                                floatValue.floatValue = EditorGUILayout.FloatField(new GUIContent(name.stringValue, ""), floatValue.floatValue);
                                break;
                            case ThemePropertyValueTypes.shaderRange:
                                vector2Value.vector2Value = EditorGUILayout.Vector2Field(new GUIContent(name.stringValue, ""), vector2Value.vector2Value);
                                break;
                            case ThemePropertyValueTypes.Vector2:
                                vector2Value.vector2Value = EditorGUILayout.Vector2Field(new GUIContent(name.stringValue, ""), vector2Value.vector2Value);
                                break;
                            case ThemePropertyValueTypes.Vector3:
                                SerializedProperty vector3Value = item.FindPropertyRelative("Vector3");
                                vector3Value.vector3Value = EditorGUILayout.Vector3Field(new GUIContent(name.stringValue, ""), vector3Value.vector3Value);
                                break;
                            case ThemePropertyValueTypes.Vector4:
                                SerializedProperty vector4Value = item.FindPropertyRelative("Vector4");
                                vector4Value.vector4Value = EditorGUILayout.Vector4Field(new GUIContent(name.stringValue, ""), vector4Value.vector4Value);
                                break;
                            case ThemePropertyValueTypes.Quaternion:
                                SerializedProperty quaternionValue = item.FindPropertyRelative("Quaternion");
                                Vector4 vect4 = new Vector4(quaternionValue.quaternionValue.x, quaternionValue.quaternionValue.y, quaternionValue.quaternionValue.z, quaternionValue.quaternionValue.w);
                                vect4 = EditorGUILayout.Vector4Field(new GUIContent(name.stringValue, ""), vect4);
                                quaternionValue.quaternionValue = new Quaternion(vect4.x, vect4.y, vect4.z, vect4.w);
                                break;
                            case ThemePropertyValueTypes.Texture:
                                SerializedProperty texture = item.FindPropertyRelative("Texture");
                                EditorGUILayout.PropertyField(texture, new GUIContent(name.stringValue, ""), false);
                                break;
                            case ThemePropertyValueTypes.Material:
                                SerializedProperty material = item.FindPropertyRelative("Material");
                                EditorGUILayout.PropertyField(material, new GUIContent(name.stringValue, ""), false);
                                break;
                            case ThemePropertyValueTypes.AudioClip:
                                SerializedProperty audio = item.FindPropertyRelative("AudioClip");
                                EditorGUILayout.PropertyField(audio, new GUIContent(name.stringValue, ""), false);
                                break;
                            case ThemePropertyValueTypes.Animaiton:
                                SerializedProperty animation = item.FindPropertyRelative("Animation");
                                EditorGUILayout.PropertyField(animation, new GUIContent(name.stringValue, ""), false);
                                break;
                            case ThemePropertyValueTypes.GameObject:
                                SerializedProperty gameObjectValue = item.FindPropertyRelative("Int");
                                EditorGUILayout.PropertyField(gameObjectValue, new GUIContent(name.stringValue, ""), false);
                                break;
                            case ThemePropertyValueTypes.String:
                                SerializedProperty stringValue = item.FindPropertyRelative("String");
                                stringValue.stringValue = EditorGUILayout.TextField(new GUIContent(name.stringValue, ""), stringValue.stringValue);
                                break;
                            case ThemePropertyValueTypes.Bool:
                                SerializedProperty boolValue = item.FindPropertyRelative("Bool");
                                boolValue.boolValue = EditorGUILayout.Toggle(new GUIContent(name.stringValue, ""), boolValue.boolValue);
                                break;
                            default:
                                break;
                        }
                    }
                }

                EditorGUI.indentLevel = indentOnSectionStart;
            }

            GUILayout.Space(5);

            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
        }

        
        private static string[] GetOptions(SerializedProperty options)
        {
            List<string> list = new List<string>();
            for (int i = 0; i < options.arraySize; i++)
            {
                list.Add(options.GetArrayElementAtIndex(i).stringValue);
            }

            return list.ToArray();
        }

        private static int GetOptionsIndex(SerializedProperty options, string selection)
        {
            for (int i = 0; i < options.arraySize; i++)
            {
                if (options.GetArrayElementAtIndex(i).stringValue == selection)
                {
                    return i;
                }
            }

            return 0;
        }

        private void SetupThemeOptions()
        {
            ProfileItem.ThemeLists lists = ProfileItem.GetThemeTypes();
            themeOptions = lists.Names.ToArray();
            themeTypes = lists.Types.ToArray();
        }

        // redundant method, put in a utils with static methods!!!
        private static int ReverseLookup(string option, string[] options)
        {
            for (int i = 0; i < options.Length; i++)
            {
                if (options[i] == option)
                {
                    return i;
                }
            }

            return 0;
        }

        public static ShaderProperties[] GetShaderProperties(Renderer renderer, ShaderPropertyType[] filter)
        {
            List<ShaderProperties> properties = new List<ShaderProperties>();
            if (renderer != null)
            {
                Material material = ThemeBase.GetValidMaterial(renderer);

                if (material != null)
                {
                    int count = ShaderUtil.GetPropertyCount(material.shader);

                    for (int i = 0; i < count; i++)
                    {
                        string name = ShaderUtil.GetPropertyName(material.shader, i);
                        ShaderPropertyType type = ShaderUtilConvert(ShaderUtil.GetPropertyType(material.shader, i));
                        bool isHidden = ShaderUtil.IsShaderPropertyHidden(material.shader, i);
                        Vector2 range = new Vector2(ShaderUtil.GetRangeLimits(material.shader, i, 1), ShaderUtil.GetRangeLimits(material.shader, i, 2));

                        if (!isHidden && HasShaderPropertyType(filter, type))
                        {
                            properties.Add(new ShaderProperties() { Name = name, Type = type, Range = range });
                        }
                    }
                }
            }
            return properties.ToArray();
        }

        public static ShaderPropertyType ShaderUtilConvert(ShaderUtil.ShaderPropertyType type)
        {
            ShaderPropertyType shaderType;
            switch (type)
            {
                case ShaderUtil.ShaderPropertyType.Color:
                    shaderType = ShaderPropertyType.Color;
                    break;
                case ShaderUtil.ShaderPropertyType.Vector:
                    shaderType = ShaderPropertyType.Vector;
                    break;
                case ShaderUtil.ShaderPropertyType.Float:
                    shaderType = ShaderPropertyType.Float;
                    break;
                case ShaderUtil.ShaderPropertyType.Range:
                    shaderType = ShaderPropertyType.Range;
                    break;
                case ShaderUtil.ShaderPropertyType.TexEnv:
                    shaderType = ShaderPropertyType.TexEnv;
                    break;
                default:
                    shaderType = ShaderPropertyType.None;
                    break;
            }
            return shaderType;
        }

        public static string[] SerializedPropertyToOptions(SerializedProperty arr)
        {
            List<string> list = new List<string>();
            for (int i = 0; i < arr.arraySize; i++)
            {
                list.Add(arr.GetArrayElementAtIndex(i).stringValue);
            }
            return list.ToArray();
        }

        public static bool HasShaderPropertyType(ShaderPropertyType[] filter, ShaderPropertyType type)
        {
            for (int i = 0; i < filter.Length; i++)
            {
                if (filter[i] == type)
                {
                    return true;
                }
            }

            return false;
        }*/
    }
}
