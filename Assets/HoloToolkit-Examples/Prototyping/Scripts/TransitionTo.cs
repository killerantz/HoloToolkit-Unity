// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HoloToolkit.Examples.Prototyping
{
    /// <summary>
    /// ease types
    ///     Linear: steady progress
    ///     EaseIn: ramp up in speed
    ///     EaseOut: ramp down in speed
    ///     EaseInOut: ramp up then down in speed
    ///     Free: super ease - just updates as the TargetValue changes
    /// </summary>
    [System.Serializable]
    public enum LerpTypes { Timed, Free }

    /// <summary>
    /// animates the rotation of an object with eases
    /// </summary>
    public abstract class TransitionTo<T> : MonoBehaviour, ITransitionTo
    {
        [Tooltip("The object to animate")]
        public GameObject TargetObject;

        [Tooltip("The rotation value to animate to")]
        public T TargetValue;
        
        [Tooltip("The type of ease to apply to the tween")]
        public LerpTypes LerpType;

        public AnimationCurve EaseCurve;

        [Tooltip("Duration of the animation in seconds")]
        public float LerpTime = 1f;

        [Tooltip("auto start? or status")]
        public bool IsRunning = false;

        public bool GetIsRunning()
        {
            return IsRunning;
        }

        [Tooltip("animation complete!")]
        public UnityEvent OnComplete;

        public UnityEvent GetOnCompleteEvent()
        {
            return OnComplete;
        }

        // animation ticker
        protected float mLerpTimeCounter;

        // starting/current rotation
        protected T mStartValue;

        protected bool mInited = false;

        protected virtual void Awake()
        {
            if (TargetObject == null)
            {
                TargetObject = this.gameObject;
            }
            
            if (EaseCurve.keys.Length < 1)
            {
                //default, linear
                EaseCurve = AnimationCurve.Linear(0, 1, 1, 1);

                // could be old setting from previous version
                if ((int)LerpType > 1)
                {
                    UpgradeOldEaseSettings();
                }
            }

            mInited = true;

            mStartValue = GetValue();
        }

        public virtual void StartRunning()
        {
            if (TargetObject == null)
            {
                TargetObject = this.gameObject;
            }

            mStartValue = GetValue();
            mLerpTimeCounter = 0;
            IsRunning = true;
        }

        /// <summary>
        /// Set the rotation to the cached starting value
        /// </summary>
        public virtual void ResetTransform()
        {
            if (TargetObject == null)
            {
                TargetObject = this.gameObject;
            }

            if (!mInited) return;

            SetValue(mStartValue);
            IsRunning = false;
            mLerpTimeCounter = 0;
        }

        /// <summary>
        /// reverse the rotation - go back
        /// </summary>
        public void Reverse()
        {
            
            if (TargetObject == null)
            {
                TargetObject = this.gameObject;
            }

            if (!mInited) return;

            TargetValue = mStartValue;
            mStartValue = TargetValue;
            mLerpTimeCounter = 0;
            IsRunning = true;
        }

        /// <summary>
        /// Stop the animation
        /// </summary>
        public void StopRunning()
        {
            IsRunning = false;
        }

        // get the current value
        public abstract T GetValue();

        // set the value
        public abstract void SetValue(T value);

        // compare values
        public abstract bool CompareValues(T value1, T value2);

        // lerp values
        public abstract T LerpValues(T startValue, T targetValue, float percent);

        /// <summary>
        /// Calculate the new rotation based on time and ease settings
        /// </summary>
        /// <param name="currentValue"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        private T GetNewValue(T currentValue, float percent)
        {
           T newValue = GetValue();
            switch (LerpType)
            {
                case LerpTypes.Timed:
                    newValue = LerpValues(mStartValue, TargetValue, percent * EaseCurve.Evaluate(percent));
                    break;
                case LerpTypes.Free:
                    newValue = LerpValues(currentValue, TargetValue, percent * EaseCurve.Evaluate(percent));
                    break;
                default:
                    break;
            }

            return newValue;
        }
        
        /// <summary>
        /// Animate
        /// </summary>
        private void Update()
        {
            // manual animation, only runs when auto started or StartRunning() is called
            if (IsRunning && LerpType != LerpTypes.Free)
            {
                // get the time
                mLerpTimeCounter += Time.deltaTime;
                float percent = mLerpTimeCounter / LerpTime;

                // set the rotation
                SetValue(GetNewValue(GetValue(), percent));

                // fire the event if complete
                if (percent >= 1)
                {
                    IsRunning = false;
                    OnComplete.Invoke();
                }
            }
            else if (LerpType == LerpTypes.Free) // is always running, just waiting for the TargetValue to change
            {
                bool wasRunning = IsRunning;
                
                SetValue(GetNewValue(GetValue(), LerpTime * Time.deltaTime));
                IsRunning = CompareValues(GetValue(), TargetValue);

                // fire the event if complete
                if (IsRunning != wasRunning && !IsRunning)
                {
                    OnComplete.Invoke();
                }
            }
        }

        private void UpgradeOldEaseSettings()
        {
            int type = (int)LerpType;

            switch (type)
            {
                case 0:
                    LerpType = LerpTypes.Timed;
                    break;
                case 1:
                    LerpType = LerpTypes.Timed;
                    // ease in
                    EaseCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1, 2, 0));
                    break;
                case 2:
                    LerpType = LerpTypes.Timed;

                    // ease out
                    EaseCurve = new AnimationCurve(new Keyframe(0, 0, 0, 2), new Keyframe(1, 1));
                    break;
                case 3:
                    LerpType = LerpTypes.Timed;

                    // ease inout
                    EaseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
                    break;
                case 4:
                    LerpType = LerpTypes.Free;
                    break;
            }
        }
    }
}
