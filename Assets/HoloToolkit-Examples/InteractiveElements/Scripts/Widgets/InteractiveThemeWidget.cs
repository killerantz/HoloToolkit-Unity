// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;						
using UnityEngine;

namespace HoloToolkit.Examples.InteractiveElements
{
    /// <summary>
    /// A version of InteractiveWidget that uses an InteractiveTheme to define each state
    /// </summary>
    public abstract class InteractiveThemeWidget : InteractiveWidget
    {
        [Tooltip("A tag for finding the theme in the scene")]
        public string ThemeTag = "defaultTheme";

        // checks if the theme has changed since the last SetState was called.
        protected bool mThemeUpdated;

        protected string mCheckThemeTag = "";

        /// <summary>
        /// Sets the themes based on the Theme Tags
        /// </summary>
        public abstract void SetTheme();

        /// <summary>
        /// If the themes have changed since the last SetState was called, update the widget
        /// </summary>
        public void RefreshIfNeeded()
        {
            if (mThemeUpdated)
            {
                SetState(State);
            }
        }

        /// <summary>
        /// Sets the state of the widget
        /// </summary>
        /// <param name="state"></param>
        public override void SetState(Interactive.ButtonStateEnum state)
        {
            base.SetState(state);
            mThemeUpdated = false;
        }

        /// <summary>
        /// get a new theme if themeTag has changed.
        /// </summary>
        protected override void Update()
        {
            base.Update();
            
            if (!mCheckThemeTag.Equals(ThemeTag) && InteractiveHost != null)
            {
                SetTheme();
                RefreshIfNeeded();
                mCheckThemeTag = ThemeTag;
            }
        }

        /// <summary>
        /// Find a ColorInteractiveTheme by tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public ColorInteractiveTheme GetColorTheme(string tag)
        {
            ColorInteractiveTheme[] colorThemes;
            ColorInteractiveTheme theme = null;

            if (InteractiveHost != null)
            {
                // search locally
                colorThemes = InteractiveHost.GetComponentsInChildren<ColorInteractiveTheme>();
                theme = FindColorTheme(colorThemes, tag);
            }
            else
            {
                colorThemes = GetComponentsInParent<ColorInteractiveTheme>();
                theme = FindColorTheme(colorThemes, tag);
            }

            // search globally
            if (theme == null)
            {
                colorThemes = FindObjectsOfType<ColorInteractiveTheme>();
                theme = FindColorTheme(colorThemes, tag);
            }

            if (!mThemeUpdated) mThemeUpdated = theme != null;

            return theme;
        }

        // compare theme tags
        private ColorInteractiveTheme FindColorTheme(ColorInteractiveTheme[] colorThemes, string tag)
        {
            for (int i = 0; i < colorThemes.Length; ++i)
            {
                if (colorThemes[i].Tag == tag)
                {
                    return colorThemes[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Find a Vector3InteractiveTheme by tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public Vector3InteractiveTheme GetVector3Theme(string tag)
        {
            Vector3InteractiveTheme[] vector3Themes;
            Vector3InteractiveTheme theme = null;

            if (InteractiveHost != null)
            {
                // search locally
                vector3Themes = InteractiveHost.GetComponentsInChildren<Vector3InteractiveTheme>();
                theme = FindVector3Theme(vector3Themes, tag);
            }
            else
            {
                vector3Themes = GetComponentsInParent<Vector3InteractiveTheme>();
                theme = FindVector3Theme(vector3Themes, tag);
            }

            // search globally
            if (theme == null)
            {
                vector3Themes = FindObjectsOfType<Vector3InteractiveTheme>();
                theme = FindVector3Theme(vector3Themes, tag);
            }

            if (!mThemeUpdated) mThemeUpdated = theme != null;

            return theme;
        }

        // compare theme tags
        public Vector3InteractiveTheme FindVector3Theme(Vector3InteractiveTheme[] vector3Themes, string tag)
        {
            for (int i = 0; i < vector3Themes.Length; ++i)
            {
                if (vector3Themes[i].Tag == tag)
                {
                    return vector3Themes[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Find a TextureInteractiveTheme by tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public TextureInteractiveTheme GetTextureTheme(string tag)
        {
            TextureInteractiveTheme[] textureThemes;
            TextureInteractiveTheme theme = null;
            
            if (InteractiveHost != null)
            {
                // search locally
                textureThemes = InteractiveHost.GetComponentsInChildren<TextureInteractiveTheme>();
                theme = FindTextureTheme(textureThemes, tag);
            }
            else
            {
                textureThemes = GetComponentsInParent<TextureInteractiveTheme>();
                theme = FindTextureTheme(textureThemes, tag);
            }

            // search globally
            if (theme == null)
            {
                textureThemes = FindObjectsOfType<TextureInteractiveTheme>();
                theme = FindTextureTheme(textureThemes, tag);
            }

            if (!mThemeUpdated) mThemeUpdated = theme != null;

            return theme;
        }

        // compare theme tags
        public TextureInteractiveTheme FindTextureTheme(TextureInteractiveTheme[] textureThemes, string tag)
        {
            for (int i = 0; i < textureThemes.Length; ++i)
            {
                if (textureThemes[i].Tag == tag)
                {
                    return textureThemes[i];
                }
            }

            return null;
        }
		/// <summary>
        /// Find a QuaternionInteractiveTheme by tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public QuaternionInteractiveTheme GetQuaternionTheme(string tag)
        {
            // search locally
            QuaternionInteractiveTheme[] quaternionTheme;
            QuaternionInteractiveTheme theme = null;

            if (InteractiveHost != null)
            {
                // search locally
                quaternionTheme = InteractiveHost.GetComponentsInChildren<QuaternionInteractiveTheme>();
                theme = FindQuaternionTheme(quaternionTheme, tag);
            }
            else
            {
                quaternionTheme = GetComponentsInParent<QuaternionInteractiveTheme>();
                theme = FindQuaternionTheme(quaternionTheme, tag);
            }

            // search globally
            if (theme == null)
            {
                quaternionTheme = FindObjectsOfType<QuaternionInteractiveTheme>();
                theme = FindQuaternionTheme(quaternionTheme, tag);
            }

            return theme;
        }

        // compare theme tags
        public QuaternionInteractiveTheme FindQuaternionTheme(QuaternionInteractiveTheme[] quaternionThemes, string tag)
        {
            for (int i = 0; i < quaternionThemes.Length; ++i)
            {
                if (quaternionThemes[i].Tag == tag)
                {
                    return quaternionThemes[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Find a StringInteractiveTheme by tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public StringInteractiveTheme GetStringTheme(string tag)
        {
            // search locally
            StringInteractiveTheme[] stringThemes;
            StringInteractiveTheme theme = null;

            if (InteractiveHost != null)
            {
                // search locally
                stringThemes = InteractiveHost.GetComponentsInChildren<StringInteractiveTheme>();
                theme = FindStringTheme(stringThemes, tag);
            }
            else
            {
                stringThemes = GetComponentsInParent<StringInteractiveTheme>();
                theme = FindStringTheme(stringThemes, tag);
            }


            // search globally
            if (theme == null)
            {
                stringThemes = FindObjectsOfType<StringInteractiveTheme>();
                theme = FindStringTheme(stringThemes, tag);
            }

            return theme;
        }

        // compare theme tags
        public StringInteractiveTheme FindStringTheme(StringInteractiveTheme[] stringThemes, string tag)
        {
            for (int i = 0; i < stringThemes.Length; ++i)
            {
                if (stringThemes[i].Tag == tag)
                {
                    return stringThemes[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Find a BoolInteractiveTheme by tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public BoolInteractiveTheme GetBoolTheme(string tag)
        {
            // search locally
            BoolInteractiveTheme[] boolThemes;
            BoolInteractiveTheme theme = null;

            if (InteractiveHost != null)
            {
                // search locally
                boolThemes = InteractiveHost.GetComponentsInChildren<BoolInteractiveTheme>();
                theme = FindBoolTheme(boolThemes, tag);
            }
            else
            {
                boolThemes = GetComponentsInParent<BoolInteractiveTheme>();
                theme = FindBoolTheme(boolThemes, tag);
            }


            // search globally
            if (theme == null)
            {
                boolThemes = FindObjectsOfType<BoolInteractiveTheme>();
                theme = FindBoolTheme(boolThemes, tag);
            }

            return theme;
        }

        // compare theme tags
        public BoolInteractiveTheme FindBoolTheme(BoolInteractiveTheme[] boolThemes, string tag)
        {
            for (int i = 0; i < boolThemes.Length; ++i)
            {
                if (boolThemes[i].Tag == tag)
                {
                    return boolThemes[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Find a AudioInteractiveTheme by tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public AudioInteractiveTheme GetAudioTheme(string tag)
        {
            // search locally
            AudioInteractiveTheme[] audioThemes;
            AudioInteractiveTheme theme = null;

            if (InteractiveHost != null)
            {
                // search locally
                audioThemes = InteractiveHost.GetComponentsInChildren<AudioInteractiveTheme>();
                theme = FindAudioTheme(audioThemes, tag);
            }
            else
            {
                audioThemes = GetComponentsInParent<AudioInteractiveTheme>();
                theme = FindAudioTheme(audioThemes, tag);
            }


            // search globally
            if (theme == null)
            {
                audioThemes = FindObjectsOfType<AudioInteractiveTheme>();
                theme = FindAudioTheme(audioThemes, tag);
            }

            return theme;
        }

        // compare theme tags
        public AudioInteractiveTheme FindAudioTheme(AudioInteractiveTheme[] audioThemes, string tag)
        {
            for (int i = 0; i < audioThemes.Length; ++i)
            {
                if (audioThemes[i].Tag == tag)
                {
                    return audioThemes[i];
                }
            }

            return null;
        }

        public virtual List<string> GetTags()
        {
            List<string> tags = new List<string>();
            Type type = this.GetType();
            FieldInfo[] info = (FieldInfo[])type.GetFields();

            for (int i = 0; i < info.Length; i++)
            {
                if (info[i].FieldType == typeof(string))
                {
                    string tag = (string)info[i].GetValue(this);
                    if (tag != "")
                    {
                        tags.Add(tag);
                    }
                }
            }

            return tags;
        }			 
    }
}
