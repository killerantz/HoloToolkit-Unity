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

    public enum BasicEaseCurves { Linear, EaseIn, EaseOut, EaseInOut}

    /// <summary>
    /// animates the rotation of an object with eases
    /// </summary>
    [ExecuteInEditMode]
    public abstract class TransitionTo<T> : MonoBehaviour, ITransitionTo
    {
        [Tooltip("The object to animate")]
        public GameObject TargetObject;

        [Tooltip("The rotation value to animate to")]
        public T TargetValue;
        
        [Tooltip("The type of ease to apply to the tween")]
        public LerpTypes LerpType;

        [Tooltip("The ease curve to use while animating")]
        public AnimationCurve EaseCurve;

        [Tooltip("Duration of the animation in seconds")]
        public float LerpTime = 1f;

        [Tooltip("auto start? or status")]
        public bool IsRunning = false;

        [Tooltip("animation complete!")]
        public UnityEvent OnComplete;

        public UnityEvent GetOnCompleteEvent()
        {
            return OnComplete;
        }

        // for the interface to enforce a status value
        public bool GetIsRunning()
        {
            return IsRunning;
        }

        // animation ticker
        protected float mLerpTimeCounter = 0;

        // starting/current rotation
        protected T mStartValue;

        // handle on start
        protected bool mInited = false;

        // Lerp time should make sense when in free mode
        // a speed range from the seed to the ratio, slowest to fastest, but reversed for LerpTime.
        protected float FreeTimeRatio = 10;
        protected float FreeTimeRatioSeed = 0.5f;

        protected virtual void Awake()
        {
            if (TargetObject == null)
            {
                TargetObject = this.gameObject;
            }
            
            // set a linear curve by default
            if (EaseCurve == null)
            {
                SetEaseCurve(BasicEaseCurves.Linear);
            }
            else if (EaseCurve.keys.Length < 1)
            {
                SetEaseCurve(BasicEaseCurves.Linear);
            }

            // in the case older scripts are being updated to work with TransitionTo
            if ((int)LerpType > 1)
            {
                UpdateLerpType((int)LerpType);
            }

            mStartValue = GetValue();
            mInited = true;
        }

        /// <summary>
        /// Start the animation
        /// </summary>
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
                    newValue = LerpValues(mStartValue, TargetValue, EaseCurve.Evaluate(percent) * percent);
                    break;
                case LerpTypes.Free:
                    newValue = LerpValues(currentValue, TargetValue, EaseCurve.Evaluate(percent) * percent);
                    break;
                default:
                    break;
            }

            return newValue;
        }
        
        public void SetEaseCurve(BasicEaseCurves curve)
        {
            switch (curve)
            {
                case BasicEaseCurves.Linear:
                    EaseCurve = AnimationCurve.Linear(0, 1, 1, 1);
                    break;
                case BasicEaseCurves.EaseIn:
                    EaseCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1, 2.5f, 0));
                    break;
                case BasicEaseCurves.EaseOut:
                    EaseCurve = new AnimationCurve(new Keyframe(0, 0, 0, 2.5f), new Keyframe(1, 1));
                    break;
                case BasicEaseCurves.EaseInOut:
                    EaseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Animate
        /// </summary>
        private void Update()
        {
#if UNITY_EDITOR
            if(!Application.isPlaying)
                return;
#endif
            
            // manual animation, only runs when auto started or StartRunning() is called
            if (IsRunning && LerpType != LerpTypes.Free)
            {
                // get the time
                mLerpTimeCounter += Time.deltaTime;

                if (mLerpTimeCounter >= LerpTime)
                {
                    mLerpTimeCounter = LerpTime;
                }

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
                
                SetValue(GetNewValue(GetValue(), Mathf.Clamp(Mathf.Pow(FreeTimeRatioSeed, LerpTime) * FreeTimeRatio, FreeTimeRatioSeed, FreeTimeRatio) * Time.deltaTime));
                IsRunning = CompareValues(GetValue(), TargetValue);

                // fire the event if complete
                if (IsRunning != wasRunning && !IsRunning)
                {
                    OnComplete.Invoke();
                }
            }
        }

        protected void UpdateLerpType(int type)
        {
            switch (type)
            {
                case 0:
                    LerpType = LerpTypes.Timed;
                    SetEaseCurve(BasicEaseCurves.Linear);
                    break;
                case 1:
                    LerpType = LerpTypes.Timed;
                    SetEaseCurve(BasicEaseCurves.EaseIn);
                    break;
                case 2:
                    LerpType = LerpTypes.Timed;
                    SetEaseCurve(BasicEaseCurves.EaseOut);
                    break;
                case 3:
                    LerpType = LerpTypes.Timed;
                    SetEaseCurve(BasicEaseCurves.EaseInOut);
                    break;
                case 4:
                    LerpType = LerpTypes.Timed;
                    SetEaseCurve(BasicEaseCurves.Linear);
                    break;
            }
        }
    }
}
