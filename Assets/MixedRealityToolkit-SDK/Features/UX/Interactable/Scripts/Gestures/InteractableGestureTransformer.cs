// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Core.Interfaces.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SDK.UX
{
    public class InteractableGestureTransformer : InteractableGestureManipulator
    {
        //TODO: make sure movement sticks to alignment, especially the projection
        // TODO: add clipping to max distance or max Rotation?
        // TODO: make sure receiver handles multiple gesture Manipulators! - had to fix references in code for the list.
        // what are axis constraints????
        // force gesture input limit!!!!
        
        /// <summary>
        /// Should the transform snap back when the gesture is released? > 0 will animate the transform back to start
        /// </summary>
        [Tooltip("Should the gesture snap back when released?")]
        public float SnapBackTime = 0;

        [Tooltip("Scale the transform maniplulation by this factor")]
        public float TransformMultiplier = 1;

        // Position
        public bool Position = true;
        public bool ProjectPosition = false;
        public bool FaceOnMove = false;

        public float Smoothness = 5.0f;

        // Rotation
        public bool Rotate = false;

        // Scale
        public bool Scale = false;
        public bool UniformScale = true;
        public Vector3 AxisConstraints = Vector3.one;
        
        protected float snapBackCount = 0;
        protected int inputCount = 0;

        // cached transform values
        protected Quaternion currentRotation = Quaternion.identity;
        protected Vector3 currentPostion = Vector3.zero;
        protected Vector3 currentScale = Vector3.one;
        protected Quaternion startRotation = Quaternion.identity;
        protected Vector3 startPosition = Vector3.zero;
        protected Vector3 startScale = Vector3.one;
        
        // solvers
        protected GestureDataPositionSolver positionSolver;
        protected GestureDataRotationSolver rotationSolver;
        protected GestureDataScaleSolver scaleSolver;

        // data
        protected TransformSolver.TransformData positionData;
        protected TransformSolver.TransformData rotationData;
        protected TransformSolver.TransformData scaleData;

        protected void Awake()
        {
            snapBackCount = SnapBackTime;
            startRotation = transform.rotation;
            startPosition = transform.position;
            startScale = transform.localScale;
        }
        
        protected override void GestureStarting()
        {
            positionSolver = new GestureDataPositionSolver(TransformMultiplier, ProjectPosition);
            scaleSolver = new GestureDataScaleSolver(TransformMultiplier, UniformScale);
            rotationSolver = new GestureDataRotationSolver(TransformMultiplier);

            GestureData data = CurrentGestureData;

            if (inputData.Count > 0)
            {
                data = TwoSouceGestureData;
            }

            if (Position)
            {
                positionSolver.TransformMultiplier = TransformMultiplier;
                positionData = positionSolver.SetupTarget(data, transform, FaceOnMove);
            }

            if (Scale)
            {
                scaleSolver.TransformMultiplier = TransformMultiplier;
                scaleData = scaleSolver.SetupTarget(data, transform);
            }

            if (Rotate)
            {
                rotationSolver.TransformMultiplier = TransformMultiplier;
                rotationData = rotationSolver.SetupTarget(data, transform);
            }

            CalculateGesture();
        }

        protected void CalculateGesture()
        {
            if(inputCount != inputData.Count)
            {
                if (inputData.Count < 1)
                {
                    inputCount = inputData.Count;
                    GestureStopping();
                }
                else
                {
                    inputCount = inputData.Count;
                    GestureStarting();
                }
            }

            GestureData data = CurrentGestureData;
            
            if (inputData.Count > 1)
            {
                data = TwoSouceGestureData;
            }
            
            Quaternion newRotation = startRotation;

            float decay = Mathf.Pow(10, -Smoothness);
            float smoothing = 1 - Mathf.Pow(decay, Time.deltaTime);

            if (Position)
            {
                positionData = positionSolver.UpdateTarget(data, transform);
                currentPostion = positionData.Position;
                transform.position = Vector3.Lerp(transform.position, currentPostion, smoothing);
                newRotation = positionData.Rotation;
            }

            if (Scale)
            {
                scaleData = scaleSolver.UpdateTarget(data, transform);
                currentScale = scaleData.Scale;
                transform.localScale = Vector3.Lerp(transform.localScale, currentScale, smoothing);
            }

            if (Rotate)
            {
                rotationData = rotationSolver.UpdateTarget(data, transform);
                currentRotation = rotationData.Rotation;
                transform.rotation = Quaternion.Lerp(transform.rotation, currentRotation * newRotation, smoothing);
            }
            else
            {
                currentRotation = newRotation;
                transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, smoothing);
            }
         }

        /// <summary>
        /// Animate the dot snapping back to the center point on release
        /// </summary>
        /// <param name="percent"></param>
        protected void TickerUpdate(float percent)
        {
            currentRotation = Quaternion.Lerp(currentRotation, startRotation, percent);
            currentPostion = Vector3.Lerp(currentPostion, startPosition, percent);
            currentScale = Vector3.Lerp(currentScale, startScale, percent);

            transform.rotation = currentRotation;
            transform.localScale = currentScale;
            transform.position = currentPostion;
        }

        /// <summary>
        /// Handle automation
        /// </summary>
        protected override void Update()
        {
            base.Update();

            if (snapBackCount < SnapBackTime && !gestureRunning)
            {
                snapBackCount += Time.deltaTime;
                if (snapBackCount > SnapBackTime)
                {
                    snapBackCount = SnapBackTime;
                }

                TickerUpdate(snapBackCount / SnapBackTime);
            }
        }

        protected override void GestureStopping()
        {
            snapBackCount = 0;
        }

        protected override void GestureUpdating()
        {
            CalculateGesture();
        }
    }
}
