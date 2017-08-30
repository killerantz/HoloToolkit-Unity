// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HoloToolkit.Examples.Prototyping
{
    /// <summary>
    /// Abstract class for accessing and setting color values on a different types of objects
    /// </summary>
    public abstract class ColorObject
    {
        /// <summary>
        /// Settings for the type of color value to access in the shader
        /// </summary>
        public enum ShaderColorTypes { Color, TintColor, RimColor }
        protected string shaderColorType = "_Color";

        /// <summary>
        /// reference to the the host game object
        /// </summary>
        public GameObject GameObjectRef;

        /// <summary>
        /// is the assumed object type valid.
        /// </summary>
        public bool Valid { get { return isValid; } }
        protected bool isValid = false;

        public ColorObject(GameObject gameObject)
        {
            // constructor
            GameObjectRef = gameObject;
        }

        /// <summary>
        /// Sets the color on the object
        /// </summary>
        /// <param name="color">Color: color to set</param>
        public abstract void SetColor(Color color);

        /// <summary>
        /// gets the color on the object
        /// </summary>
        /// <returns></returns>
        public abstract Color GetColor();

        /// <summary>
        /// sets the alpha value of the color
        /// </summary>
        /// <param name="alpha"></param>
        public abstract void SetAlpha(float alpha);

        /// <summary>
        /// get the current alpha value
        /// </summary>
        /// <returns></returns>
        public abstract float GetAlpha();

        /// <summary>
        /// sets the shader color property string using in Material.GetColor() or Material.SetColor()
        /// </summary>
        /// <param name="type">ShaderColorTypes</param>
        public void SetShaderColorType(ShaderColorTypes type)
        {
            shaderColorType = "_" + type.ToString();
        }
    }

    /// <summary>
    /// A version of ColorObject that accesses color properties on UI Text objects
    /// </summary>
    public class TextColorObject : ColorObject
    {
        private Text mText;

        public TextColorObject(GameObject gameObject) : base(gameObject)
        {
            mText = gameObject.GetComponent<Text>();
            isValid = mText != null;
        }

        public override void SetColor(Color color)
        {
            if (mText != null)
            {
                mText.color = color;
            }
        }

        public override Color GetColor()
        {
            if (mText != null)
            {
                return mText.color;
            }
            return Color.black;
        }

        public override void SetAlpha(float alpha)
        {
            if (mText != null)
            {
                Color color = mText.color;
                color.a = alpha;
                mText.color = color;
            }
        }

        public override float GetAlpha()
        {
            if (mText != null)
            {
                Color color = mText.color;
                return color.a;
            }
            return 0;
        }
    }

    /// <summary>
    /// A version of ColorObject that accesses color properties on TextMesh objects
    /// </summary>
    public class TextMeshColorObject : ColorObject
    {
        private TextMesh mTextMesh;
        public TextMeshColorObject(GameObject gameObject) : base(gameObject)
        {
            mTextMesh = gameObject.GetComponent<TextMesh>();
            isValid = mTextMesh != null;
        }

        public override void SetColor(Color color)
        {
            if (mTextMesh != null)
            {
                mTextMesh.color = color;
            }
        }

        public override Color GetColor()
        {
            if (mTextMesh != null)
            {
                return mTextMesh.color;
            }
            return Color.black;
        }

        public override void SetAlpha(float alpha)
        {
            if (mTextMesh != null)
            {
                Color color = mTextMesh.color;
                color.a = alpha;
                mTextMesh.color = color;
            }
        }

        public override float GetAlpha()
        {
            if (mTextMesh != null)
            {
                Color color = mTextMesh.color;
                return color.a;
            }
            return 0;
        }
    }

    /// <summary>
    /// A version of Color Object that accesses color properties on material shaders
    /// </summary>
    public class MaterialColorObject : ColorObject
    {
        private MaterialPropertyBlock materialBlock;

        public MaterialColorObject(GameObject gameObject) : base(gameObject)
        {
            materialBlock = new MaterialPropertyBlock();
            gameObject.GetComponent<Renderer>().GetPropertyBlock(materialBlock);

            isValid = true;
        }

        public override void SetColor(Color color)
        {
            materialBlock.SetColor(shaderColorType, color);
            GameObjectRef.GetComponent<Renderer>().SetPropertyBlock(materialBlock);
        }

        public override Color GetColor()
        {
            GameObjectRef.GetComponent<Renderer>().GetPropertyBlock(materialBlock);
            Color color = materialBlock.GetVector(shaderColorType);
            return color;
        }

        public override void SetAlpha(float alpha)
        {
            Color color = GetColor();
            color.a = alpha;
            SetColor(color);
        }

        public override float GetAlpha()
        {
            Color color = GetColor();
            return color.a;
        }
    }

    /// <summary>
    /// A version of Color Object that accesses color properties of mulitple materials
    /// </summary>
    public class MaterialsColorObject : ColorObject
    {
        private Material[] mMaterials;

        public MaterialsColorObject(GameObject gameObject) : base(gameObject)
        {
            Renderer renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
#if UNITY_EDITOR
                mMaterials = renderer.sharedMaterials;
#else
                mMaterials = renderer.materials;
#endif
            }

            isValid = mMaterials.Length > 1;
        }

        public override void SetColor(Color color)
        {
            if (mMaterials != null)
            {
                for (int i = 0; i < mMaterials.Length; i++)
                {
                    mMaterials[i].SetColor(shaderColorType, color);
                }
            }
        }

        public override Color GetColor()
        {
            if (mMaterials != null)
            {
                return mMaterials[0].GetColor(shaderColorType);
            }
            return Color.black;
        }

        public override void SetAlpha(float alpha)
        {
            if (mMaterials != null)
            {
                for (int i = 0; i < mMaterials.Length; i++)
                {
                    Color color = mMaterials[i].GetColor(shaderColorType);
                    color.a = alpha;
                    mMaterials[i].SetColor(shaderColorType, color);
                }
            }
        }

        public override float GetAlpha()
        {
            if (mMaterials != null)
            {
                Color color = mMaterials[0].GetColor(shaderColorType);
                return color.a;
            }
            return 0;
        }

        public Material[] GetMaterials()
        {
            return mMaterials;
        }

        public Color GetColorByMaterialName(string name)
        {
            if (mMaterials != null)
            {
                for (int i = 0; i < mMaterials.Length; i++)
                {
                    if (TrimName(mMaterials[i].name) == name)
                    {
                        return mMaterials[i].GetColor(shaderColorType);
                    }
                }
            }

            return Color.black;
        }

        public void SetColorByMaterialName(Color color, string name)
        {
            if (mMaterials != null)
            {
                for (int i = 0; i < mMaterials.Length; i++)
                {
                    if (TrimName(mMaterials[i].name) == name)
                    {
                        mMaterials[i].SetColor(shaderColorType, color);
                    }
                }
            }
        }

        public void SetAlphaByMaterialName(float alpha, string name)
        {
            if (mMaterials != null)
            {
                for (int i = 0; i < mMaterials.Length; i++)
                {
                    if (TrimName(mMaterials[i].name) == name)
                    {
                        Color color = mMaterials[i].GetColor(shaderColorType);
                        color.a = alpha;
                        mMaterials[i].SetColor(shaderColorType, color);
                    }
                }
            }
        }

        public float GetAlphaByMaterialName(string name)
        {
            if (mMaterials != null)
            {
                for (int i = 0; i < mMaterials.Length; i++)
                {
                    if (TrimName(mMaterials[i].name) == name)
                    {
                        Color color = mMaterials[i].GetColor(shaderColorType);
                        return color.a;
                    }
                }
            }
            return 0;
        }

        private string TrimName(string name)
        {
            int SpaceIndex = name.IndexOf(" ");
            if (SpaceIndex > -1)
            {
                name = name.Substring(0, SpaceIndex);
            }

            return name;
        }
    }

    /// <summary>
    /// Abstraction layer for accessing colorObjects based on object type
    /// </summary>
    public class ColorAbstraction: ColorObject
    {
        private ColorObject mColorObject;

        public ColorAbstraction(GameObject gameObject, ColorObject.ShaderColorTypes shaderType =  ShaderColorTypes.Color): base(gameObject)
        {
            GameObjectRef = gameObject;
            mColorObject = GetColorObject(gameObject);
            if (mColorObject != null)
            {
                mColorObject.SetShaderColorType(shaderType);
                isValid = true;
            }
            else
            {
                Debug.Log("There are no Color supported components attached to this game object!");
            }
        }

        public override Color GetColor()
        {
            if (mColorObject != null)
            {
                return mColorObject.GetColor();
            }
            else
            {
                Debug.Log("There are no Color supported components attached to this game object! Default color returned!");
            }
            Debug.Log("BLACK!");
            return Color.black;
        }

        public override void SetColor(Color color)
        {
            if (mColorObject != null)
            {
                mColorObject.SetColor(color);
            }
            else
            {
                Debug.Log("There are no Color supported components attached to this game object! No Colors set");
            }
        }

        public override void SetAlpha(float alpha)
        {
            if (mColorObject != null)
            {
                mColorObject.SetAlpha(alpha);
            }
            else
            {
                Debug.Log("There are no Color supported components attached to this game object! No alpha set");
            }
        }

        public override float GetAlpha()
        {
            if (mColorObject != null)
            {
                return mColorObject.GetAlpha();
            }
            else
            {
                Debug.Log("There are no Color supported components attached to this game object! No alpha returned!");
            }
            return 0;
        }

        public void SetColorByMaterialName(Color color, string name)
        {
            MaterialsColorObject colorObject = mColorObject as MaterialsColorObject;
            if (colorObject != null)
            {
                colorObject.SetColorByMaterialName(color, name);
            }
            else
            {
                mColorObject.SetColor(color);
            }
        }

        public Color GetColorByMaterialName(string name)
        {
            MaterialsColorObject colorObject = mColorObject as MaterialsColorObject;
            if (colorObject != null)
            {
                return colorObject.GetColorByMaterialName(name);
            }
            return mColorObject.GetColor();
        }

        public void SetAlphaByMaterialName(float alpha, string name)
        {
            MaterialsColorObject colorObject = mColorObject as MaterialsColorObject;
            if (colorObject != null)
            {
                colorObject.SetAlphaByMaterialName(alpha, name);
            }
            else
            {
                mColorObject.SetAlpha(alpha);
            }
        }

        public float GetAlphaByMaterialName(float alpha, string name)
        {
            MaterialsColorObject colorObject = mColorObject as MaterialsColorObject;
            if (colorObject != null)
            {
                return colorObject.GetAlphaByMaterialName(name);
            }
            else
            {
                return mColorObject.GetAlpha();
            }
        }

        public Material[] GetMaterials()
        {
            MaterialsColorObject colorObject = mColorObject as MaterialsColorObject;
            if (colorObject != null)
            {
                return colorObject.GetMaterials();
            }
            return null;
        }

        public static ColorObject GetColorObject(GameObject gameObject)
        {
            TextColorObject textColor = new TextColorObject(gameObject);
            if (textColor.Valid)
            {
                return textColor;
            }

            TextMeshColorObject textMeshColor = new TextMeshColorObject(gameObject);
            if (textMeshColor.Valid)
            {
                return textMeshColor;
            }

            MaterialsColorObject materialsColor = new MaterialsColorObject(gameObject);
            if (materialsColor.Valid)
            {
                return materialsColor;
            }

            MaterialColorObject materialColor = new MaterialColorObject(gameObject);
            if (materialColor.Valid)
            {
                return materialColor;
            }

            return null;
        }

        public static TextColorObject GetTextColorObject(GameObject gameObject)
        {
            TextColorObject textColor = new TextColorObject(gameObject);
            if (textColor.Valid)
            {
                return textColor;
            }

            return null;
        }

        public static TextMeshColorObject GetTextMeshColorObject(GameObject gameObject)
        {
            TextMeshColorObject textMeshColor = new TextMeshColorObject(gameObject);
            if (textMeshColor.Valid)
            {
                return textMeshColor;
            }

            return null;
        }

        public static MaterialColorObject GetMaterialColorObject(GameObject gameObject, ColorObject.ShaderColorTypes shaderType = ColorObject.ShaderColorTypes.Color)
        {
            MaterialColorObject materialColor = new MaterialColorObject(gameObject);
            if (materialColor.Valid)
            {
                materialColor.SetShaderColorType(shaderType);
                return materialColor;
            }

            return null;
        }

        public static MaterialsColorObject GetMaterialsColorObject(GameObject gameObject, ColorObject.ShaderColorTypes shaderType = ColorObject.ShaderColorTypes.Color)
        {
            MaterialsColorObject materialsColor = new MaterialsColorObject(gameObject);
            if (materialsColor.Valid)
            {
                materialsColor.SetShaderColorType(shaderType);
                return materialsColor;
            }

            return null;
        }
    }
}
