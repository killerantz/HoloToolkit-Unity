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
    public class InteractableGestureManipulator : MonoBehaviour
    {
        /// <summary>
        /// Filters how gesture data is processed.
        /// Raw: no extra processing is done, just current gesture information compared to start gesture information.
        /// Camera: compares gestures to the facing camera direction, processes data based on camera's right vector, good for billboarded UI.
        /// Aligned: compares the gesture to the Alignment Vector in world space, for instance (1, 0, 0) will compute distance and percentage of the gesture moving along the x axis.
        /// CameraAligned: Rotates the Alignment Vector based on camera direction, so x and always left or right and z is alway forward
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

        /// <summary>
        /// Processed gesture data to drive object manipulation
        /// </summary>
        public struct GestureData
        {
            // handle pointer type, if pointer has an angle?????
            public Vector3 AlignmentVector;
            public Vector3 Direction;
            public Vector3 StartDirection;
            public float Distance;
            public float Percentage;
            public float MaxDistance;
            public bool FlipDirecationOnCameraForward;
            public Vector3 OriginPoint;
            public Vector3 CurrentPoint;

            public GestureData(Vector3 alignmentVector, float maxDistance, bool flipDirectionOnCameraForward)
            {
                AlignmentVector = alignmentVector;
                MaxDistance = maxDistance;
                FlipDirecationOnCameraForward = flipDirectionOnCameraForward;
                Direction = new Vector3();
                Distance = 0;
                Percentage = 0;
                OriginPoint = new Vector3();
                CurrentPoint = new Vector3();
                StartDirection = new Vector3();
            }
        }

        /// <summary>
        /// a relative distance for the gesture, used to scale gesture values based on world space
        /// If set to 0.3 and a user moves a gesture 0.3 meters from the origin point, we can calculate that the users has met the max distance or 100%
        /// </summary>
        [Tooltip("The distance we expect a gesture to encompass")]
        public float MaxGestureDistance = 1.5f;

        /// <summary>
        /// How gesture data is processed based on user's world forward direction, Example: positive z means x is to the right, when z is negitive, x is to the left.
        /// </summary>
        [Tooltip("Flips gesture data when a user faces -z to handle world space forward issues")]
        public bool FlipDirectionOnCameraForward = false;

        /// <summary>
        /// How the gesture should be processed and changes the types of properties presented in the custom inspector
        /// </summary>
        [Tooltip("The type of gesture data processing")]
        public GestureDataType DataProcessingType = GestureDataType.Raw;// conditional based on selection (custom inspector)

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
        /// Camera reference
        /// </summary>
        protected Camera MainCamera { get { return Camera.main; } }

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

        // values for internal processing during each gesture update
        protected bool gestureRunning;
        protected Vector3[] gesturePoints;
        protected Vector3 calculatedDirection = Vector3.zero;
        protected float averageDistance = 0;
        
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
        protected virtual void GestureStarting()
        {
            // do something on start
        }

        /// <summary>
        /// An easy way to check state, when the gesture updates
        /// </summary>
        protected virtual void GestureUpdating()
        {
            // do something on update
        }

        /// <summary>
        /// An easy way to check state, when the gesture ends
        /// </summary>
        protected virtual void GestureStopping()
        {
            // do something on stopping
        }

        #endregion Virtual Functions

        #region Gesture Update
        /// <summary>
        /// a gesture is in progress
        /// </summary>
        /// <param name="pointerData"></param>
        /// <param name="status"></param>
        public virtual void OnUpdate(Interactable.PointerData[] pointerData, GestureStatusEnum status)
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
                    for (int i = 0; i < pointerData.Length; i++)
                    {
                        InputData data = new InputData();
                        data.Pointer = pointerData[i].Pointer;
                        data.GestureData = new GestureData();
                        data.StartProperties = CreatePointerProperties(pointerData[i].Pointer, MainCamera.transform);
                        data.CurrentProperties = CreatePointerProperties(pointerData[i].Pointer, MainCamera.transform);
                        data = ProcessGestureUpdate(data, i);
                        inputData.Add(data);
                    }
                    PostGestureUpdate();

                    GestureStarting();
                    gestureRunning = true;
                    break;
                case GestureStatusEnum.Update:
                    if (inputData.Count < pointerData.Length)
                    {
                        // add and order input data
                        AddPointerData(pointerData);
                    }
                    else if(inputData.Count > pointerData.Length)
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

        protected virtual void PreGestureUpdate()
        {
            // two handed gestures
            gesturePoints = new Vector3[4];

            calculatedDirection = Vector3.zero;
            averageDistance = 0;
        }

        protected virtual InputData ProcessGestureUpdate(InputData data, int index)
        {
            PointerProperties currentProperties = CreatePointerProperties(data.Pointer, MainCamera.transform);
            PointerProperties startProperties = data.StartProperties;
            data.GestureData = CreateGestureData(DataProcessingType, startProperties.Position, currentProperties.Position, startProperties.CamDirection, MainCamera.transform.right, AlignmentVector, MaxGestureDistance, FlipDirectionOnCameraForward, ClampGesturePercentage);

            if (index < 2)
            {
                gesturePoints[index * 2] = startProperties.Position;
                gesturePoints[index * 2 + 1] = currentProperties.Position;
            }

            calculatedDirection = calculatedDirection + data.GestureData.Direction;
            averageDistance = averageDistance + data.GestureData.Distance;

            return data;
        }

        protected virtual void PostGestureUpdate()
        {
            averageDistance = averageDistance / 2;
            Vector3 startDirection = gesturePoints[2] - gesturePoints[0];
            Vector3 directionVector = gesturePoints[3] - gesturePoints[1];
            Vector3 startHeadRay = inputData[0].StartProperties.CamDirection;

            TwoSouceGestureData = CreateGestureData(DataProcessingType, startDirection, directionVector, startHeadRay, MainCamera.transform.right, AlignmentVector, MaxGestureDistance, FlipDirectionOnCameraForward, ClampGesturePercentage);
            
            float currentPercentage = TwoSouceGestureData.Percentage;
            float currentDistatnce = TwoSouceGestureData.Distance;
            Vector3 currentPosition = inputData[0].CurrentProperties.Position;

            GestureData data = TwoSouceGestureData;
            data.StartDirection = startDirection;
            data.Direction = directionVector;
            data.Percentage = currentPercentage;
            data.Distance = currentDistatnce;
            data.CurrentPoint = currentPosition;

            CurrentGestureData = data;
        }

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


        public static GestureData CreateGestureData(GestureDataType gestureData, Vector3 start, Vector3 current, Vector3 startHeadRay, Vector3 right, Vector3 alignmentVector, float maxDistance, bool flipDirecationOnCameraForward, bool clampPercentage)
        {
            GestureData data = new GestureData(alignmentVector, maxDistance, flipDirecationOnCameraForward);

            data.Direction = current - start;
            bool flipDirection = Vector3.Dot(Vector3.forward, startHeadRay) < 0 && flipDirecationOnCameraForward;

            if (flipDirection)
            {
                data.Direction = -data.Direction;
            }
            switch (gestureData)
            {
                case GestureDataType.Raw:
                    data.Distance = data.Direction.magnitude;
                    break;
                case GestureDataType.Camera:
                    data.Distance = Vector3.Dot(data.Direction, right);
                    break;
                case GestureDataType.Aligned:
                    data.Distance = Vector3.Dot(data.Direction, alignmentVector);
                    break;
                case GestureDataType.CameraAligned:
                    data.Distance = GetDistanceFromCameraAligned(data.Direction, alignmentVector, Camera.main.transform);
                    break;
                default:
                    break;
            }

            data.Percentage = Mathf.Min(Mathf.Abs(data.Distance) / maxDistance, 1);
            if (!clampPercentage)
            {
                data.Percentage = Mathf.Abs(data.Distance) / maxDistance;
            }

            data.OriginPoint = start;
            data.CurrentPoint = current;

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

        /// <summary>
        /// Get the current camera's world matrix in world space
        /// </summary>
        /// <returns>Matix4x4</returns>
        public Matrix4x4 GetCameraMatrix()
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

            CurrentGestureData = CreateGestureData(DataProcessingType, startPosition, newPosition, Vector3.forward, Vector3.right, AlignmentVector, MaxGestureDistance, FlipDirectionOnCameraForward, ClampGesturePercentage);
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
                CurrentGestureData = CreateGestureData(DataProcessingType, Vector3.zero, gesturePosition, Vector3.forward, Vector3.right, AlignmentVector, MaxGestureDistance, FlipDirectionOnCameraForward, ClampGesturePercentage);
            }

            // we have not updates for a while, so kill the gesture if running
            if (Time.realtimeSinceStartup - lastGestureTime > Time.deltaTime * 3 && gestureRunning)
            {
                // stop the gesture
                stopGesture = true;
            }
        }

        /*
        

        /// <summary>
        /// Is a gesture in progress?
        /// </summary>
        protected bool GestureStarted = false;

        /// <summary>
        /// The current gesture position from the gesture origin (delta)
        /// Some of the settings above help to determine if this value is positive or negative
        /// so we can easily know which direction the gesture is moving (left vs. right, up vs. down, forward vs. backward).
        /// </summary>
        protected float CurrentDistance;

        /// <summary>
        /// The percentage of gesture distance compared to the MaxGestureDistance
        /// </summary>
        protected float CurrentPercentage;

        /// <summary>
        /// animation time for a keyword triggered gesture
        /// </summary>
        protected float KeywordGestureTime = 0.5f;

        /// <summary>
        /// animation counter for a keyword triggered gesture
        /// </summary>
        protected float KeywordGestureTimeCounter = 0.5f;

        /// <summary>
        /// animation direction for a keyword triggered gesture
        /// </summary>
        protected Vector3 KeywordGestureVector;

        /// <summary>
        /// keep track of gesture count so we see when it changes
        /// </summary>
        protected int gestureCount = 0;
        
        /// <summary>
        /// A list of hands and their directions, distances and percentages, index 2 is a delta between the two
        /// </summary>
        protected GestureInteractiveData[] HandsData = new GestureInteractiveData[3];

        /// <summary>
        /// Gesture information for a single hand, easier to pass around
        /// </summary>
        protected GestureInteractiveData OneHandData;

        /// <summary>
        /// The delta between the two hands
        /// </summary>
        protected GestureInteractiveData TwoHandData;

        // set the default gesture state
        virtual protected void Awake()
        {
            GestureState = GestureInteractive.GestureManipulationState.None;
        }

        /// <summary>
        /// Gesture updates called by GestureInteractive
        /// </summary>
        /// <param name="startGesturePosition">The gesture origin position</param>
        /// <param name="currentGesturePosition">the current gesture position</param>
        /// <param name="startHeadOrigin">the origin of the camera when the gesture started</param>
        /// <param name="startHeadRay">the camera forward when the gesture started</param>
        /// <param name="gestureState">curent gesture state</param>
        public virtual void ManipulationUpdate(Vector3 startGesturePosition, Vector3 currentGesturePosition, Vector3 startHeadOrigin, Vector3 startHeadRay, GestureInteractive.GestureManipulationState gestureState)
        {
            if (gestureState == GestureInteractive.GestureManipulationState.Start || (!GestureStarted && gestureState != GestureInteractive.GestureManipulationState.Start))
            {
                CameraMatrix = GetCameraMatrix();
                StartGesturePosition = startGesturePosition;
                CurrentGesturePosition = startGesturePosition;
                StartHeadPosition = startHeadOrigin;
                StartHeadRay = startHeadRay;
                GestureStarted = true;
                GestureStarting();
            }
            else
            {
                CurrentGesturePosition = currentGesturePosition;
                GestureUpdating();
            }

            UpdateGesture();
            calculatedDirection = DirectionVector;
            averageDistance = CurrentDistance;

            if (gestureState == GestureInteractive.GestureManipulationState.None || gestureState == GestureInteractive.GestureManipulationState.Lost)
            {
                GestureStarted = false;
                GestureStopping();
            }

            GestureState = gestureState;
        }

        /// <summary>
        /// takes an array of gestureHandData
        /// </summary>
        /// <param name="data"></param>
        public virtual void ManipulationUpdate(GestureHandData[] data, GestureInteractive.GestureHandLimits limits)
        {
            // change in data count
            if (gestureCount != data.Length)
            {
                gestureCount = data.Length;
            }

            if (data.Length == 1 && limits == GestureInteractive.GestureHandLimits.One)
            {
                ManipulationUpdate(data[0].StartGesturePosition, data[0].CurrentGesturePosition, data[0].StartHeadOrigin, data[0].StartHeadRay, data[0].State);
            }
            else if (data.Length == 2 || limits == GestureInteractive.GestureHandLimits.None)
            {
                // two handed gestures
                Vector3[] points = new Vector3[4];
                bool stopping = false;

                calculatedDirection = Vector3.zero;
                averageDistance = 0;

                for (int i = 0; i < data.Length; i++)
                {
                    if (i < 2)
                    {
                        HandsData[i] = CreateGestureData(GestureData, data[i].StartGesturePosition, data[i].CurrentGesturePosition, MainCamera.transform.right, data[i].StartHeadRay, AlignmentVector, MaxGestureDistance, FlipDirecationOnCameraForward, ClampPercentage);

                        points[i * 2] = data[i].StartGesturePosition;
                        points[i * 2 + 1] = data[i].CurrentGesturePosition;
                        calculatedDirection = calculatedDirection + HandsData[i].Direction;
                        averageDistance = averageDistance + HandsData[i].Distance;

                        if (data[i].State == GestureInteractive.GestureManipulationState.Lost || data[i].State == GestureInteractive.GestureManipulationState.None)
                        {
                            stopping = true;
                        }
                    }
                }

                averageDistance = averageDistance / 2;
                StartDirectionVector = points[2] - points[0];
                DirectionVector = points[3] - points[1];

                TwoHandData = CreateGestureData(GestureData, StartDirectionVector, DirectionVector, MainCamera.transform.right, data[0].StartHeadRay, AlignmentVector, MaxGestureDistance, FlipDirecationOnCameraForward, ClampPercentage);

                HandsData[2] = TwoHandData;

                CurrentPercentage = TwoHandData.Percentage;
                CurrentDistance = TwoHandData.Distance;

                CurrentGesturePosition = data[0].CurrentGesturePosition;

                GestureStarted = true;

                if (GestureState == GestureInteractive.GestureManipulationState.None || GestureState == GestureInteractive.GestureManipulationState.Lost)
                {
                    GestureStarted = true;
                    CameraMatrix = GetCameraMatrix();
                    StartGesturePosition = data[0].StartGesturePosition;
                    StartHeadPosition = data[0].StartHeadOrigin;
                    StartHeadRay = data[0].StartHeadRay;
                    GestureStarting();
                    GestureState = GestureInteractive.GestureManipulationState.Start;
                }
                else
                {
                    GestureStarted = false;

                    if (!stopping)
                    {
                        GestureState = GestureInteractive.GestureManipulationState.Update;
                        GestureUpdating();
                    }
                    else
                    {
                        GestureState = GestureInteractive.GestureManipulationState.None;
                        GestureStopping();
                    }
                }
            }

            lastGestureTime = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// An easy way to check state, when the gesture starts
        /// </summary>
        protected virtual void GestureStarting()
        {
            // do something on start
        }

        /// <summary>
        /// An easy way to check state, when the gesture updates
        /// </summary>
        protected virtual void GestureUpdating()
        {
            // do something on update
        }

        /// <summary>
        /// An easy way to check state, when the gesture ends
        /// </summary>
        protected virtual void GestureStopping()
        {
            // do something on stopping
        }

        public virtual void Tap()
        {
            // do something on Tap
        }

        /// <summary>
        /// Returns a data set about the current gesture information compared to a specific vector.
        /// For instance, to compare if the gesture is moving vertically or horizontally,
        /// create two isntances of this data set and compare the distance for each.
        /// If the vertical percentage is greater than the horizontal percentage then the gesture is moving vertically.
        /// </summary>
        /// <param name="alignmentVector"></param>
        /// <param name="maxDistance"></param>
        /// <param name="flipDirecationOnCameraForward"></param>
        /// <returns></returns>
        public GestureInteractiveData GetGestureData(Vector3 alignmentVector, float maxDistance, bool flipDirecationOnCameraForward)
        {
            GestureInteractiveData data = new GestureInteractiveData(alignmentVector, maxDistance, flipDirecationOnCameraForward);

            data.Direction = DirectionVector;
            bool flipDirection = Vector3.Dot(Vector3.forward, StartHeadRay) < 0 && flipDirecationOnCameraForward;

            if (flipDirection)
            {
                data.Direction = -data.Direction;
            }
            switch (GestureData)
            {
                case GestureDataType.Raw:
                    data.Distance = data.Direction.magnitude;
                    break;
                case GestureDataType.Camera:
                    data.Distance = Vector3.Dot(data.Direction, MainCamera.transform.right);
                    break;
                case GestureDataType.Aligned:
                    data.Distance = Vector3.Dot(data.Direction, alignmentVector);
                    break;
                case GestureDataType.CameraAligned:
                    data.Distance = GetDistanceByAlignmentVector(alignmentVector);
                    break;
                default:
                    break;
            }

            data.Percentage = Mathf.Min(Mathf.Abs(data.Distance) / maxDistance, 1);
            if (!ClampPercentage)
            {
                data.Percentage = Mathf.Abs(data.Distance) / maxDistance;
            }

            return data;
        }

        

        

        /// <summary>
        /// Rotates the gesture vector around a pivot point based on the camera matrix
        /// </summary>
        /// <param name="direction">Current gesture position</param>
        /// <param name="orientation">Gesture origin position</param>
        /// <returns></returns>
        public Vector3 WorldForwardVector(Vector3 direction, Vector3 orientation, bool flipY = false, bool flipZ = false)
        {
            Matrix4x4 cameraWorld = GetCameraMatrix(); //CameraMatrix

            // create a new vector from the raw gesture data
            Vector3 rawVector = direction - orientation;

            // flip z index?
            if (flipZ)
            {
                rawVector.z = -rawVector.z;
            }

            if (flipY)
            {
                rawVector.y = -rawVector.y;
            }

            Vector3 newDirection = cameraWorld.MultiplyVector(rawVector);

            // replace the y
            newDirection.y = rawVector.y;

            return newDirection;
        }

        /// <summary>
        /// A way to programatically override a gesture, used for keywork gestures.
        /// </summary>
        /// <param name="gestureVector"></param>
        public void SetGestureVector(Vector3 gestureVector, float percent)
        {
            if (GestureStarted)
            {
                return;
            }

            KeywordGestureVector = gestureVector;
            KeywordGestureTimeCounter = 0;
            CameraMatrix = GetCameraMatrix();
            StartGesturePosition = gestureVector * (MaxGestureDistance * percent) * (KeywordGestureTimeCounter / KeywordGestureTime);
            StartHeadPosition = new Vector3();
            StartHeadRay = Vector3.forward;

            CurrentGesturePosition = gestureVector * (MaxGestureDistance * percent) * (KeywordGestureTimeCounter / KeywordGestureTime);

            ManipulationUpdate(StartGesturePosition, Vector3.up, StartHeadPosition, StartHeadRay, GestureInteractive.GestureManipulationState.Start);
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

        /// <summary>
        /// Aligns a gesture to the camera direction
        /// </summary>
        /// <param name="directionVector"></param>
        /// <param name="flipX"></param>
        /// <param name="flipY"></param>
        /// <returns></returns>
        protected Vector3 DirectionVectorToZPlane(Vector3 directionVector, bool flipX, bool flipY)
        {
            // set the direciton based on camera and gesture updates
            float cameraDirectionX = flipX ? -directionVector.x : directionVector.x;
            float cameraDirectionY = flipY ? -directionVector.y : directionVector.y;
            return MainCamera.transform.forward * cameraDirectionY + MainCamera.transform.right * cameraDirectionX;
        }

        /// <summary>
        /// Rotates a gesture based on camera forward.
        /// Helps to take a vector in world space and rotate it to Vector3.forward
        /// </summary>
        /// <param name="gesturePosition"></param>
        /// <returns></returns>
        public Vector3 GesturePosition(Vector3 gesturePosition)
        {
            // rotate the screen space mouse position to world space, based on the camera direciton and compress pixels to world 
            // get current angle from forward - returns an absolute value of 0 - 180
            float angleDiff = Vector3.Angle(Vector3.forward, MainCamera.transform.forward);
            // make sure angle works 360 degrees, find the left or right side
            float dot = Vector3.Dot(Vector3.right, MainCamera.transform.forward);
            // if dot is positive, camera is pointing right of Vector3.forward or rotating clockwise
            if (dot < 0)
                angleDiff = -angleDiff;
            // rotate the world space converted mouse vector on the Y axis
            return RotateVectorOnY(gesturePosition, Vector3.zero, -angleDiff);
        }

        /// <summary>
        /// Rotates a vector on the y axis
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="pivot"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        private Vector3 RotateVectorOnY(Vector3 vector, Vector3 pivot, float angle)
        {
            return Quaternion.Euler(0, angle, 0) * (vector - pivot) + pivot;
        }

        /// <summary>
        /// Flips the direction based on the direction of the camera and the direction of the control.
        /// Used when FlipDirecationOnCameraForward is true to correct the calculations based on if the user
        /// is facing the control or not, compared to if the user if facing forward or not.
        /// </summary>
        /// <param name="toFlip"></param>
        /// <param name="controlPosition"></param>
        /// <param name="controlForward"></param>
        /// <returns></returns>
        protected float FlipDistanceOnFacingControl(float toFlip, Vector3 controlPosition, Vector3 controlForward)
        {
            Vector3 cameraRay = StartHeadPosition - controlPosition;
            bool facingForward = Vector3.Dot(MainCamera.transform.forward, StartHeadRay) >= 0;
            bool facingControl = Vector3.Dot(cameraRay, controlForward) >= 0;

            if (!facingForward)
            {
                // then the direction was flipped
                // facing back
                if (facingControl)
                {
                    toFlip = -toFlip;
                }
            }
            else
            {
                if (!facingControl)
                {
                    toFlip = -toFlip;
                }
            }

            return toFlip;
        }

        

        /// <summary>
        /// Local way of getting the distance of a gesture based on the camera direction compared to a world vector
        /// </summary>
        /// <param name="alignment"></param>
        /// <returns></returns>
        protected float GetDistanceByAlignmentVector(Vector3 alignment)
        {
            return CurrentDistance = GetDistanceFromCameraAligned(DirectionVector, alignment, MainCamera.transform);
        }
*/
    }
}
