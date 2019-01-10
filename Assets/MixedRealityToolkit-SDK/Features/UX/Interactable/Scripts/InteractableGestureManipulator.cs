// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Core.Interfaces.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SDK.UX
{
    /// <summary>
    /// processes pointer information and converts it to values that can be used to maniplutate objects based on simple gesture input
    /// </summary>
    public abstract class InteractableGestureManipulator : MonoBehaviour
    {
        /// <summary>
        /// Filters how gesture data is processed.
        /// Raw: no extra processing is done, just current gesture information compared to start gesture information.
        /// Camera: compares gestures to the facing camera direction, processes data based on camera's right vector, good for billboarded UI.
        /// Aligned: compares the gesture to the Alignment Vector in world space, for instance (1, 0, 0) will compute distance and percentage of the gesture moving along the x axis.
        /// CameraAligned: Rotates the Alignment Vector based on camera direction, so x and always left or right and z is always forward
        /// </summary>
        public enum GestureDataType { Raw, Camera, Aligned, CameraAligned }

        /// <summary>
        /// Changes how data is process based on the amount of pointers
        /// The interaction could change as a source is added or removed or limited to a specific amount of sources
        /// </summary>
        public enum GestureSourceLimits { One, Two, None }

        /// <summary>
        /// Basic gesture states
        /// </summary>
        public enum GestureStatusEnum {None, Start, Update }

        /// <summary>
        /// cached pointer information containing start values
        /// </summary>
        public struct InputData
        {
            public IMixedRealityPointer Pointer;
            public GestureData GestureData;
            public PointerProperties StartProperties;
            public PointerProperties CurrentProperties;
        }

        public struct PointerProperties
        {
            public Quaternion Rotation;
            public Vector3 Position;
            public Ray Ray;
            public Vector3 CamPosition;
            public Vector3 CamDirection;
        }

        public struct GestureDataOptions
        {
            public GestureDataType DataType;
            public Vector3 AlignmentVector;
            public bool FlipDirecationOnCameraForward;
            public float MaxDistance;
            public bool ClampPercentage;

            public GestureDataOptions(GestureDataType dataType, Vector3 alignmentVector, float maxDistance, bool flipDirectionOnCameraForward, bool clampPercentage)
            {
                DataType = dataType;
                AlignmentVector = alignmentVector;
                FlipDirecationOnCameraForward = flipDirectionOnCameraForward;
                MaxDistance = maxDistance;
                ClampPercentage = clampPercentage;
            }
        }

        public struct GestureDataInputValues
        {
            // extra values
            public Vector3 StartPoint;
            public Vector3 CurrentPoint;
            public Quaternion StartRotation;
            public Quaternion CurrentRotation;

            public GestureDataInputValues(Vector3 point, Quaternion rotation)
            {
                StartPoint = point;
                StartRotation = rotation;
                CurrentPoint = point;
                CurrentRotation = rotation;
            }

            public GestureDataInputValues(Vector3 startPoint, Quaternion startRotation, Vector3 currentPoint, Quaternion currentRotation)
            {
                StartPoint = startPoint;
                StartRotation = startRotation;
                CurrentPoint = currentPoint;
                CurrentRotation = currentRotation;
            }

            public void Update(Vector3 point, Quaternion rotation)
            {
                CurrentPoint = point;
                CurrentRotation = rotation;
            }
        }

        /// <summary>
        /// Processed gesture data to drive object manipulation
        /// </summary>
        public struct GestureData
        {
            public Vector3 Direction;
            public float Distance;
            public float Percentage;
            public Vector3 HeadRay;
            public Vector3 CameraRight;
            public Vector3 StartDirection;

            // start and current values as sent in during creation
            public Vector3 Start;
            public Vector3 Currrent;

            public Vector3 CompoundDirection;
            public float CompoundDistance;
            
            public GestureDataOptions Options;
            public GestureDataInputValues[] InputValues;

            /// <summary>
            /// Create a Gesture Data
            /// </summary>
            /// <param name="start"></param>
            /// <param name="current"></param>
            /// <param name="startHeadRay"></param>
            /// <param name="cameraRight"></param>
            /// <param name="options"></param>
            /// <param name="values"></param>
            public GestureData(Vector3 start, Vector3 current, Vector3 startHeadRay, Vector3 cameraRight, GestureDataOptions options)
            {
                // options and extras
                Options = options;
                InputValues = new GestureDataInputValues[0];
                HeadRay = startHeadRay;
                CameraRight = cameraRight;

                // core processed values
                Direction = current - start;
                Distance = Direction.magnitude;
                StartDirection = Direction;
                Percentage = 0;

                // cached start and current
                Start = start;
                Currrent = current;
                
                // used for two handed gestures
                CompoundDistance = 0;
                CompoundDirection = Vector3.zero;

                Update(current, startHeadRay, cameraRight);
            }

            /// <summary>
            /// Update the values in the gesture
            /// </summary>
            /// <param name="current">Current gesture position or vector</param>
            /// <param name="startHeadRay">Use the starting head ray for stability</param>
            /// <param name="cameraRight">Camera right</param>
            public void Update(Vector3 current, Vector3 startHeadRay, Vector3 cameraRight)
            {
                Direction = current - Start;
                Distance = Direction.magnitude;

                bool flipDirection = Vector3.Dot(Vector3.forward, startHeadRay) < 0 && Options.FlipDirecationOnCameraForward;

                if (flipDirection)
                {
                    Direction = -Direction;
                }
                Vector3 rotated = Options.AlignmentVector;

                switch (Options.DataType)
                {
                    case GestureDataType.Raw:
                        // raw data - distance already set
                        break;
                    case GestureDataType.Camera:
                        Distance = Vector3.Dot(Direction, cameraRight);
                        rotated = RotateToCamera(Options.AlignmentVector, MainCamera.transform);
                        Direction = Vector3.Scale(Direction, rotated);
                        break;
                    case GestureDataType.Aligned:
                        Distance = Vector3.Dot(Direction, Options.AlignmentVector);
                        Direction = Vector3.Scale(Direction, Options.AlignmentVector);
                        break;
                    case GestureDataType.CameraAligned:
                        Distance = GetDistanceFromCameraAligned(Direction, Options.AlignmentVector, Camera.main.transform);
                        rotated = RotateToCamera(Options.AlignmentVector, MainCamera.transform);
                        Direction = Vector3.Scale(Direction, rotated);
                        break;
                    default:
                        break;
                }

                Percentage = Mathf.Min(Mathf.Abs(Distance) / Options.MaxDistance, 1);
                if (!Options.ClampPercentage)
                {
                    Percentage = Mathf.Abs(Distance) / Options.MaxDistance;
                }

                Currrent = current;
            }

            public void InsertInputValues(Vector3 startPoint, Quaternion startRotation)
            {
                GestureDataInputValues values = new GestureDataInputValues(startPoint, startRotation);
                InputValues = new GestureDataInputValues[] { values };
            }

            public void InsertInputValues(Vector3 startPoint, Quaternion startRotation, Vector3 currentPoint, Quaternion currentRotation)
            {
                GestureDataInputValues values = new GestureDataInputValues(startPoint, startRotation, currentPoint, currentRotation);
                InputValues = new GestureDataInputValues[] { values };
            }

            public void InsertInputValues(GestureDataInputValues values)
            {
                InputValues = new GestureDataInputValues[] { values };
            }

            public void InsertInputValues(GestureDataInputValues[] values)
            {
                InputValues = values;
            }

            public void UpdateInputValues(Vector3 currentPoint, Quaternion currentRotation)
            {
                if(InputValues.Length > 0)
                {
                    InputValues[0].CurrentPoint = currentPoint;
                    InputValues[0].CurrentRotation = currentRotation;
                }
                else
                {
                    InsertInputValues(currentPoint, currentRotation);
                }
            }

            public void UpdateInputValues(GestureDataInputValues values)
            {
                if (InputValues.Length > 0)
                {
                    InputValues[0].CurrentPoint = values.CurrentPoint;
                    InputValues[0].CurrentRotation = values.CurrentRotation;
                }
                else
                {
                    InsertInputValues(values);
                }
            }

            public void UpdateInputValues(GestureDataInputValues[] values)
            {
                if (values.Length > 0 && values.Length <= values.Length)
                {
                    for (int i = 0; i < InputValues.Length; i++)
                    {
                        InputValues[i].CurrentPoint = values[i].CurrentPoint;
                        InputValues[i].CurrentRotation = values[i].CurrentRotation;
                    }
                }
                else
                {
                    InputValues = values;
                }
            }

            public void ResetCompoundValues()
            {
                CompoundDirection = Vector3.zero;
                CompoundDistance = 0;
            }

            public void AddCompountValues(Vector3 direction, float distance)
            {
                CompoundDirection = CompoundDirection + direction;
                CompoundDistance += distance;
            }

            public void SetCompoundValues(Vector3 direction, float distance)
            {
                CompoundDirection = direction;
                CompoundDistance += distance;
            }
        }
        
        /// <summary>
        /// Camera reference
        /// </summary>
        public static Camera MainCamera { get { return Camera.main; } }

        /// <summary>
        /// a relative distance for the gesture, used to scale gesture values based on world space
        /// If set to 0.3 and a user moves a gesture 0.3 meters from the origin point, we can calculate that the users has met the max distance or 100%
        /// </summary>
        [Tooltip("The distance we expect a gesture to encompass")]
        public float MaxGestureDistance = 0.5f;

        /// <summary>
        /// How gesture data is processed based on user's world forward direction, Example: positive z means x is to the right, when z is negitive, x is to the left.
        /// </summary>
        [Tooltip("Flips gesture data when a user faces -z to handle world space forward issues")]
        public bool FlipDirectionOnCameraForward = false;

        /// <summary>
        /// How the gesture should be processed and changes the types of properties presented in the custom inspector
        /// </summary>
        [Tooltip("The type of gesture data processing")]
        public GestureDataType GestureProcessingType = GestureDataType.Raw;// conditional based on selection (custom inspector)

        /// <summary>
        /// A vector direction to mapp the gesture to or restrict gesture values along an axis
        /// if the alignment vector is to the right, and the user moves the gesture to the right,
        /// verses moving a gesture vertically or forward.
        /// </summary>
        [Tooltip("A vector to compare a gesture to or restrict gesture movement to this axis")]
        public Vector3 AlignmentVector = Vector3.one; // handles raw gesture information

        /// <summary>
        /// The gesture limits that this manipulator will process, can be independent of the GestureReceiver
        /// </summary>
        [Tooltip("Input amount to expect, one or two hands/pointers or both")]
        public GestureSourceLimits GestureInputLimit = GestureSourceLimits.One;

        /// <summary>
        /// Typically percentages are 0-1, but some cases we may want to use max gesture distance is a sensitivity setting.
        /// </summary>
        [Tooltip("Force Percent value to always be 0-1")]
        public bool ClampGesturePercentage = true;

        /// <summary>
        /// cached pointer values
        /// </summary>
        protected List<InputData> inputData = new List<InputData>();

        /// <summary>
        /// The last time a gesture update occurred
        /// </summary>
        protected float lastGestureTime;
        
        /// <summary>
        /// Orientation based on the user facing direction.
        /// Can be used when translating a gesture to an object based on the camera direction and the objects direction.
        /// </summary>
        protected Matrix4x4 CameraMatrix;
        
        /// <summary>
        /// Current Gesture Data
        /// </summary>
        public GestureData CurrentGestureData { get; private set; }

        /// <summary>
        /// first two pointers processed to create a single gesture
        /// </summary>
        public GestureData TwoSouceGestureData { get; private set; }

        /// <summary>
        /// Is the gesture manipulator running?
        /// </summary>
        public bool IsRunning { get { return gestureRunning; } }

        // values for internal processing during each gesture update
        protected bool gestureRunning;
        protected List<GestureDataInputValues> inputValues;
        protected Vector3 compoundDirection = Vector3.zero;
        protected float compoundDistance = 0;
        protected int dataCount = 0;
        
        // override gesture updates if gesture has not updated for a while
        protected bool stopGesture = false;

        // automate gestures
        protected float KeywordGestureTimeCounter = 0.5f;
        protected float KeywordGestureTime = 0.5f;
        protected Vector3 KeywordGestureVector;

        #region Virtual Functions
        /// <summary>
        /// An easy way to check state, when the gesture starts
        /// </summary>
        protected abstract void GestureStarting();

        /// <summary>
        /// An easy way to check state, when the gesture updates
        /// </summary>
        protected abstract void GestureUpdating();

        /// <summary>
        /// An easy way to check state, when the gesture ends
        /// </summary>
        protected abstract void GestureStopping();

        #endregion Virtual Functions

        #region Gesture Update

        /// <summary>
        /// a gesture is in progress
        /// </summary>
        /// <param name="pointerData"></param>
        /// <param name="status"></param>
        public virtual void OnUpdate(Interactable.PointerData[] pointerData, GestureStatusEnum status)
        {
            if (pointerData.Length != dataCount && pointerData.Length > 0)
            {
                status = GestureStatusEnum.Start;
            }

            dataCount = pointerData.Length;

            // filter input count based on input limit, I know we do this already in the gestur Receiver,
            // but UI feedback may require different settings for feedback during the same gesture
            if ((dataCount == 1 && GestureInputLimit == GestureSourceLimits.One) || dataCount > 1 || GestureInputLimit == GestureSourceLimits.None)
            {
                if (stopGesture)
                {
                    status = GestureStatusEnum.None;
                    stopGesture = false;
                }

                switch (status)
                {
                    case GestureStatusEnum.None:
                        PreGestureUpdate();
                        for (int i = 0; i < inputData.Count; i++)
                        {
                            inputData[i] = ProcessGestureUpdate(inputData[i], i);
                        }
                        PostGestureUpdate();
                        if (gestureRunning)
                        {
                            GestureStopping();
                            gestureRunning = false;
                        }
                        break;
                    case GestureStatusEnum.Start:
                        CameraMatrix = GetCameraMatrix();
                        inputData = new List<InputData>();
                        PreGestureUpdate(true);
                        for (int i = 0; i < pointerData.Length; i++)
                        {
                            InputData data = new InputData();
                            data.Pointer = pointerData[i].Pointer;
                            data.StartProperties = CreatePointerProperties(pointerData[i].Pointer, MainCamera.transform);
                            data = ProcessGestureUpdate(data, i, true);
                            inputData.Add(data);
                        }
                        PostGestureUpdate(true);

                        GestureStarting();
                        gestureRunning = true;
                        break;
                    case GestureStatusEnum.Update:
                        if (inputData.Count < pointerData.Length)
                        {
                            // add and order input data
                            AddPointerData(pointerData);
                        }
                        else if (inputData.Count > pointerData.Length)
                        {
                            // remove and order input data
                            RemovePointerData(pointerData);
                        }
                        PreGestureUpdate();
                        for (int i = 0; i < inputData.Count; i++)
                        {
                            inputData[i] = ProcessGestureUpdate(inputData[i], i);
                        }
                        PostGestureUpdate();
                        gestureRunning = true;
                        GestureUpdating();
                        break;
                    default:
                        break;
                }

                lastGestureTime = Time.realtimeSinceStartup;
            }
        }

        /// <summary>
        /// setup for a gesture update
        /// </summary>
        protected virtual void PreGestureUpdate(bool start = false)
        {
            inputValues = new List<GestureDataInputValues>();
            compoundDirection = Vector3.zero;
            compoundDistance = 0;
        }

        /// <summary>
        /// Gesture processing the update
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected virtual InputData ProcessGestureUpdate(InputData data, int index, bool start = false)
        {
            PointerProperties currentProperties = CreatePointerProperties(data.Pointer, MainCamera.transform);
            PointerProperties startProperties = data.StartProperties;
            GestureDataInputValues values = new GestureDataInputValues(startProperties.Position, startProperties.Rotation, currentProperties.Position, currentProperties.Rotation);
            if (start)
            {
                data.GestureData = CreateGestureData(GestureProcessingType, startProperties.Position, currentProperties.Position, startProperties.CamDirection, MainCamera.transform.right, AlignmentVector, MaxGestureDistance, FlipDirectionOnCameraForward, ClampGesturePercentage);
            }
            else
            {
                data.GestureData.Update(currentProperties.Position, startProperties.CamDirection, MainCamera.transform.right);
            }

            compoundDirection = compoundDirection + data.GestureData.Direction;
            compoundDistance = compoundDistance + data.GestureData.Distance;

            data.GestureData.InsertInputValues(values);
            data.GestureData.SetCompoundValues(compoundDirection, compoundDistance);
            data.CurrentProperties = currentProperties;
            data.StartProperties = startProperties;

            inputValues.Add(values);

            return data;
        }

        /// <summary>
        /// Gesture update is finished
        /// </summary>
        protected virtual void PostGestureUpdate(bool start = false)
        {
            if (inputData.Count > 0)
            {
                // two hands
                Vector3 startValue = inputValues[0].StartPoint;
                Vector3 currentValue = inputValues[0].CurrentPoint;

                //inputValues
                if (inputData.Count > 1)
                {
                    startValue = inputValues[1] .StartPoint- inputValues[0].StartPoint;
                    currentValue = inputValues[1].CurrentPoint - inputValues[0].CurrentPoint;
                }

                Vector3 startHeadRay = inputData[0].StartProperties.CamDirection;
                CurrentGestureData = inputData[0].GestureData;
                if (start)
                {
                    TwoSouceGestureData = CreateGestureData(GestureProcessingType, startValue, currentValue, startHeadRay, MainCamera.transform.right, AlignmentVector, MaxGestureDistance, FlipDirectionOnCameraForward, ClampGesturePercentage);
                }
                else
                {
                    TwoSouceGestureData.Update(currentValue, startHeadRay, MainCamera.transform.right);
                }

                TwoSouceGestureData.InsertInputValues(inputValues.ToArray());
                TwoSouceGestureData.SetCompoundValues(compoundDirection, compoundDistance);
            }
        }

        /// <summary>
        /// Create the data set of pointer properties from an iMixedRealityPointer
        /// </summary>
        /// <param name="pointer"></param>
        /// <param name="camTransform"></param>
        /// <returns></returns>
        protected PointerProperties CreatePointerProperties(IMixedRealityPointer pointer, Transform camTransform)
        {
            PointerProperties props = new PointerProperties();

            props.CamDirection = camTransform.forward;
            props.CamPosition = camTransform.position;
            props.Position = getPointerPosition(pointer);
            props.Rotation = getPointerRotation(pointer);
            props.Ray = getPointerRay(pointer);

            return props;
        }

        /// <summary>
        /// Get the current position of the pointer
        /// </summary>
        /// <param name="pointer"></param>
        /// <returns></returns>
        protected Vector3 getPointerPosition(IMixedRealityPointer pointer)
        {
            Vector3 position = Vector3.zero;
            pointer.TryGetPointerPosition(out position);
            return position;
        }

        /// <summary>
        /// get the current rotation of the pointer
        /// </summary>
        /// <param name="pointer"></param>
        /// <returns></returns>
        protected Quaternion getPointerRotation(IMixedRealityPointer pointer)
        {
            Quaternion rotation = Quaternion.identity;
            pointer.TryGetPointerRotation(out rotation);
            return rotation;
        }

        /// <summary>
        /// get the current ray of the pointer
        /// </summary>
        /// <param name="pointer"></param>
        /// <returns></returns>
        protected Ray getPointerRay(IMixedRealityPointer pointer)
        {
            Ray ray = new Ray();
            pointer.TryGetPointingRay(out ray);
            return ray;
        }

        /// <summary>
        /// Pointer data has changed, we need more data in the list
        /// </summary>
        /// <param name="pointerData"></param>
        protected void AddPointerData(Interactable.PointerData[] pointerData)
        {
            List<InputData> list = new List<InputData>();
            for (int i = 0; i < pointerData.Length; i++)
            {
                bool hasData = false;
                for (int j = 0; j < inputData.Count; j++)
                {
                    if (inputData[j].Pointer.PointerId == pointerData[i].Pointer.PointerId)
                    {
                        list.Add(inputData[j]);
                        hasData = true;
                        break;
                    }
                }

                if (!hasData)
                {
                    InputData data = new InputData();
                    data.Pointer = pointerData[i].Pointer;
                    data.GestureData = new GestureData();
                    PointerProperties props = new PointerProperties();
                    props.CamDirection = MainCamera.transform.forward;
                    props.CamPosition = MainCamera.transform.position;
                    props.Position = getPointerPosition(pointerData[i].Pointer);
                    props.Rotation = getPointerRotation(pointerData[i].Pointer);
                    props.Ray = getPointerRay(pointerData[i].Pointer);
                    data.StartProperties = props;
                    data.CurrentProperties = props;
                    list.Add(data);
                }
            }
            inputData = list;
        }

        /// <summary>
        /// a gesture input source has been removed, so remove the remaining data
        /// </summary>
        /// <param name="pointerData"></param>
        protected void RemovePointerData(Interactable.PointerData[] pointerData)
        {
            List<InputData> list = new List<InputData>();

            for (int i = 0; i < pointerData.Length; i++)
            {
                for (int j = 0; j < inputData.Count; j++)
                {
                    if (inputData[j].Pointer.PointerId == pointerData[i].Pointer.PointerId)
                    {
                        list.Add(inputData[j]);
                        break;
                    }
                }
            }

            inputData = list;
        }

        #endregion Gesture Update

        /// <summary>
        /// Creates a Gesture data model and processes the gesture to return values for direction, distance and percent to drive UI
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="start"></param>
        /// <param name="current"></param>
        /// <param name="startHeadRay"></param>
        /// <param name="right"></param>
        /// <param name="alignmentVector"></param>
        /// <param name="maxDistance"></param>
        /// <param name="flipDirecationOnCameraForward"></param>
        /// <param name="clampPercentage"></param>
        /// <returns></returns>
        public static GestureData CreateGestureData(GestureDataType dataType, Vector3 start, Vector3 current, Vector3 startHeadRay, Vector3 right, Vector3 alignmentVector, float maxDistance, bool flipDirecationOnCameraForward, bool clampPercentage)
        {
            GestureDataOptions options = new GestureDataOptions(dataType, alignmentVector, maxDistance, flipDirecationOnCameraForward, clampPercentage);

            GestureData data = new GestureData(start, current, startHeadRay, right, options);
            
            return data;
        }

        #region Helper Functions

        /// <summary>
        /// Get the distance of a gesture based on the camera direction compared to a world vector
        /// </summary>
        /// <param name="direciton"></param>
        /// <param name="alignment"></param>
        /// <param name="cameraTransform"></param>
        /// <returns></returns>
        public static float GetDistanceFromCameraAligned(Vector3 direciton, Vector3 alignment, Transform cameraTransform)
        {
            if (alignment.x > 0 && alignment.x > alignment.y && alignment.x > alignment.z)
            {
                return Vector3.Dot(direciton, cameraTransform.right);
            }

            if (alignment.x < 0 && alignment.x < alignment.y && alignment.x < alignment.z)
            {
                return Vector3.Dot(direciton, -cameraTransform.right);
            }

            if (alignment.y > 0 && alignment.y > alignment.x && alignment.y > alignment.z)
            {
                return Vector3.Dot(direciton, cameraTransform.up);
            }

            if (alignment.y < 0 && alignment.y < alignment.x && alignment.y < alignment.z)
            {
                return Vector3.Dot(direciton, -cameraTransform.up);
            }

            if (alignment.z > 0 && alignment.z > alignment.y && alignment.z > alignment.x)
            {
                return Vector3.Dot(direciton, cameraTransform.forward);
            }

            if (alignment.z < 0 && alignment.z < alignment.y && alignment.z < alignment.x)
            {
                return Vector3.Dot(direciton, -cameraTransform.forward);
            }

            return Vector3.Dot(direciton, cameraTransform.right);
        }

        public static Vector3 RotateToCamera(Vector3 vectorToRotate, Transform cameraTransform)
        {
            Quaternion cameraOrientation = Quaternion.LookRotation(cameraTransform.forward, cameraTransform.up);

            Quaternion axisOrientation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            Quaternion axisDifference = axisOrientation * Quaternion.Inverse(cameraOrientation);
            Vector3 rotatedVector = Quaternion.Inverse(axisDifference) * vectorToRotate;
            return rotatedVector;
        }

        /// <summary>
        /// Get the current camera's world matrix in world space
        /// </summary>
        /// <returns>Matix4x4</returns>
        public static Matrix4x4 GetCameraMatrix()
        {
            // get the preferred body direciton
            Vector3 up = Vector3.up;
            Vector3 forward = MainCamera.transform.forward;
            // protecting from a weird cross value
            if (Vector3.Angle(up, forward) < 10)
            {
                up = MainCamera.transform.up;
            }
            Vector3 right = Vector3.Cross(up, forward);
            right.Normalize();

            // build a matrix based on body/camera direction
            Matrix4x4 cameraWorld = new Matrix4x4();
            cameraWorld.SetColumn(0, right);
            cameraWorld.SetColumn(1, up);
            cameraWorld.SetColumn(2, forward);
            cameraWorld.SetColumn(3, new Vector4(0, 0, 0, 1));

            return cameraWorld;
        }

        public static Quaternion GetRelativeRotation(Vector3 point1, Vector3 point2)
        {
            Vector3 direction = point1 - point2;
            Vector3 forward = Vector3.Cross(direction.normalized, Vector3.up);
            Vector3 up = Vector3.Cross(forward, direction.normalized);
            return Quaternion.LookRotation(forward, up);
        }

        #endregion Helper Functions

        #region Automation
        /// <summary>
        /// A way to programatically override a gesture, used for keywork gestures.
        /// </summary>
        /// <param name="gestureVector"></param>
        public void SetGestureVector(Vector3 gestureVector, float percent)
        {
            if (gestureRunning)
            {
                return;
            }

            KeywordGestureVector = gestureVector;
            KeywordGestureTimeCounter = 0;
            CameraMatrix = GetCameraMatrix();

            Vector3 startPosition = gestureVector * (MaxGestureDistance * percent) * (KeywordGestureTimeCounter / KeywordGestureTime);
            Vector3 newPosition = startPosition;

            GestureDataInputValues values = new GestureDataInputValues(startPosition, Quaternion.identity, newPosition, Quaternion.identity);
            CurrentGestureData = CreateGestureData(GestureProcessingType, startPosition, newPosition, Vector3.forward, Vector3.right, AlignmentVector, MaxGestureDistance, FlipDirectionOnCameraForward, ClampGesturePercentage);
            CurrentGestureData.InsertInputValues(values);
        }

        /// <summary>
        /// a place holder function for taking value and settings a gesture direction.
        /// Used by the keywork gesture system so that we can have multiple keywords for a single control.
        /// For instance: forward/backward or Min/Center/Max
        /// </summary>
        /// <param name="gestureValue"></param>
        public virtual void SetGestureValue(float gestureValue)
        {
            // override to convert keywork index to vectors.
            SetGestureVector(AlignmentVector, gestureValue);
        }
        #endregion Automation

        protected virtual void Update()
        {
            if (KeywordGestureTimeCounter < 0)
            {
                if (gestureRunning)
                {
                    KeywordGestureTimeCounter = KeywordGestureTime;
                    return;
                }

                KeywordGestureTimeCounter += Time.deltaTime;
                if (KeywordGestureTimeCounter > KeywordGestureTime)
                {
                    KeywordGestureTimeCounter = KeywordGestureTime;
                }

                Vector3 gesturePosition = KeywordGestureVector * MaxGestureDistance * (KeywordGestureTimeCounter / KeywordGestureTime);
                GestureDataInputValues values = new GestureDataInputValues(Vector3.zero, Quaternion.identity, gesturePosition, Quaternion.identity);
                CurrentGestureData = CreateGestureData(GestureProcessingType, Vector3.zero, gesturePosition, Vector3.forward, Vector3.right, AlignmentVector, MaxGestureDistance, FlipDirectionOnCameraForward, ClampGesturePercentage);
                CurrentGestureData.InsertInputValues(values);
            }

            // we have not updates for a while, so kill the gesture if running
            if (Time.realtimeSinceStartup - lastGestureTime > Time.deltaTime * 3 && gestureRunning)
            {
                // stop the gesture
                stopGesture = true;
            }
        }
    }
}
