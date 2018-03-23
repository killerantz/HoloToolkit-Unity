using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloToolkit.Unity
{
    public enum ThemePropertyValueTypes { Float, Int, Color, ShaderFloat, shaderRange, Vector2, Vector3, Vector4, Quaternion, Texture, Material, AudioClip, Animaiton, GameObject, String, Bool }

    [System.Serializable]
    public struct ThemePropertyValue
    {
        public string Name;
        public string String;
        public bool Bool;
        public int Int;
        public float Float;
        public Texture Texture;
        public Material Material;
        public GameObject GameObject;
        public Vector2 Vector2;
        public Vector3 Vector3;
        public Vector4 Vector4;
        public Color Color;
        public Quaternion Quaternion;
        public AudioClip AudioClip;
        public Animation Animation;
    }

    public enum ShaderPropertyType { Color, Float, Range, TexEnv, Vector, None }

    [System.Serializable]
    public struct ShaderProperties
    {
        public string Name;
        public ShaderPropertyType Type;
        public Vector2 Range;
    }

    [System.Serializable]
    public struct ThemeProperty
    {
        public string Name;
        public ThemePropertyValueTypes Type;
        public List<ThemePropertyValue> Values;
        public ThemePropertyValue StartValue;
        public int PropId;
        public List<ShaderProperties> ShaderOptions;
        public List<string> ShaderOptionNames;
    }

    [System.Serializable]
    public class EaseSettings
    {
        public enum BasicEaseCurves { Linear, EaseIn, EaseOut, EaseInOut }
        public bool EaseValues = false;
        public AnimationCurve Curve = AnimationCurve.Linear(0, 1, 1, 1);
        public float LerpTime = 0.5f;
        private float timer = 0;

        public void OnUpdate()
        {
            if (timer < LerpTime)
            {
                timer = Mathf.Min(timer + Time.deltaTime, LerpTime);
            }
        }

        public void Start()
        {
            timer = 0;
            if (!EaseValues)
            {
                timer = LerpTime;
            }
        }

        public bool IsPlaying()
        {
            return timer < LerpTime;
        }

        public float GetLinear()
        {
            return timer / LerpTime;
        }

        public float GetCurved()
        {
            return IsLinear() ? GetLinear() : Curve.Evaluate(GetLinear());
        }

        protected bool IsLinear()
        {
            if (Curve.keys.Length > 1)
            {
                return (Curve.keys[0].value == 1 && Curve.keys[1].value == 1);
            }

            return false;
        }

        public void SetCurve(BasicEaseCurves curve)
        {
            AnimationCurve animation = AnimationCurve.Linear(0, 1, 1, 1);
            switch (curve)
            {
                case BasicEaseCurves.EaseIn:
                    animation = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1, 2.5f, 0));
                    break;
                case BasicEaseCurves.EaseOut:
                    animation = new AnimationCurve(new Keyframe(0, 0, 0, 2.5f), new Keyframe(1, 1));
                    break;
                case BasicEaseCurves.EaseInOut:
                    animation = AnimationCurve.EaseInOut(0, 0, 1, 1);
                    break;
                default:
                    break;
            }

            Curve = animation;
        }
    }
    
    public abstract class ThemeBase
    {
        public Type[] Types;
        public string Name = "Base Theme";
        public List<ThemeProperty> ThemeProperties = new List<ThemeProperty>();
        public GameObject Host;
        public EaseSettings Ease;

        private int lastState = -1;

        public abstract void SetValue(ThemeProperty property, int index, float percentage);

        public abstract ThemePropertyValue GetProperty(ThemeProperty property);

        protected float LerpFloat(float s, float e, float t)
        {
            return (e - s) * t + s;
        }

        protected int LerpInt(int s, int e, float t)
        {
            return Mathf.RoundToInt((e - s) * t) + s;
        }

        public virtual void OnUpdate(int state)
        {
            if(state != lastState)
            {
                for (int i = 0; i < ThemeProperties.Count; i++)
                {
                    ThemeProperty current = ThemeProperties[i];
                    current.StartValue = GetProperty(current);
                    Ease.Start();
                    SetValue(current, state, Ease.GetCurved());
                    ThemeProperties[i] = current;
                }
            }
            else if(Ease.EaseValues && Ease.IsPlaying())
            {
                Ease.OnUpdate();
                
                for (int i = 0; i < ThemeProperties.Count; i++)
                {
                    ThemeProperty current = ThemeProperties[i];
                    SetValue(current, state, Ease.GetCurved());
                }
            }

            lastState = state;
        }

        public static MaterialPropertyBlock GetMaterialPropertyBlock(GameObject gameObject, string colorId, string propId)
        {
            MaterialPropertyBlock materialBlock = new MaterialPropertyBlock();
            Renderer renderer = gameObject.GetComponent<Renderer>();
            renderer.GetPropertyBlock(materialBlock);

            Color color = Color.white;
            float prop = 0;
            if (renderer != null)
            {
                Material material = GetValidMaterial(renderer);
                if (material != null)
                {
                    if (!String.IsNullOrEmpty(colorId))
                    {
                        color = material.GetVector(colorId);
                        materialBlock.SetColor(colorId, color);
                    }

                    if (!String.IsNullOrEmpty(propId))
                    {
                        prop = material.GetFloat(propId);
                        materialBlock.SetFloat(propId, prop);
                    }
                }
            }

            gameObject.GetComponent<Renderer>().SetPropertyBlock(materialBlock);

            return materialBlock;
        }

        public static Material GetValidMaterial(Renderer renderer)
        {
            Material material = null;

            if (renderer != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    material = renderer.sharedMaterial;
                }
                else
                {
                    material = renderer.material;
                }
#else
                material = renderer.material;
#endif
            }
            return material;
        }

    }
}
