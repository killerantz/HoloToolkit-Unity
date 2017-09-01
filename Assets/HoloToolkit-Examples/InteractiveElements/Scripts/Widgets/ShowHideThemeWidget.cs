// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Examples.Prototyping;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloToolkit.Examples.InteractiveElements
{
    /// <summary>
    /// a widget to show or hide objects based on a theme and state
    /// </summary>
    public class ShowHideThemeWidget : InteractiveThemeWidget
    {

        public GameObject[] ObjectList;

        private BoolInteractiveTheme mTheme;

        public override void SetTheme()
        {
            mTheme = GetBoolTheme(ThemeTag);
        }

        public override void SetState(Interactive.ButtonStateEnum state)
        {
            if (mTheme != null)
            {
                for (int i = 0; i < ObjectList.Length; i++)
                {
                    TransitionToFade transition = ObjectList[i].GetComponent<TransitionToFade>();
                    if (transition != null)
                    {
                        transition.TargetValue = mTheme.GetThemeValue(state) ? 1 : 0;
                        transition.Run();
                    }
                    else
                    {
                        ObjectList[i].SetActive(mTheme.GetThemeValue(state));
                    }
                }
            }
        }
    }
}
