using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NearInteractionTouchable)),RequireComponent(typeof(BoxCollider))]
public class TouchTracker : MonoBehaviour, IMixedRealityTouchHandler
{
    // check when touch enters bounds
    // track distance from front to back
    // track distance from left to right
    // track rotation from center

    public struct TouchData
    {
        public Vector3 StartData;
        public Vector3 InputData;

        public TouchData(Vector3 start)
        {
            StartData = start;
            InputData = start;
        }

        public Vector3 Direction
        {
            get {
                return InputData - StartData;
            }
        }

        public TouchData Update(Vector3 inputData)
        {
            TouchData newData = new TouchData(StartData);
            newData.InputData = inputData;
            return newData;
        }
    }

    public bool HasTouch => touchPoints.Count > 0;
    private Dictionary<IMixedRealityController, TouchData> touchPoints = new Dictionary<IMixedRealityController, TouchData>();

    public Vector3 TouchPosition;
    public Vector3 DirectionPercentage;
    public Vector3 PercentageFromCenter;

    private void OnDisable()
    {
        touchPoints.Clear();
    }

    private void Update()
    {
        // get collider
        BoxCollider box = GetComponent<BoxCollider>();

        // get NearInteractionTouchable
        NearInteractionTouchable touch = GetComponent<NearInteractionTouchable>();

        if (HasTouch)
        {
            // calculate values from touchPoints
            foreach (var touchEntry in touchPoints)
            {
                TouchData data = touchEntry.Value;

                Vector3 scaleDirection = transform.InverseTransformDirection(data.Direction);
                Vector3 scalePoint = transform.InverseTransformPoint(data.InputData);
                TouchPosition = scalePoint;
                PercentageFromCenter = new Vector3((scalePoint.x / box.size.x)*2, (scalePoint.y / box.size.y)*2, (scalePoint.z / box.size.z)*2);
                DirectionPercentage = new Vector3(DirectionValue(scalePoint.x, box.size.x), DirectionValue(scalePoint.y, box.size.y), DirectionValue(scalePoint.z, box.size.z));
            }
        }
    }

    private float DirectionValue(float input, float size)
    {
        return (input + size * 0.5f) / size;
    }

    public void OnTouchCompleted(HandTrackingInputEventData eventData)
    {
        if (touchPoints.ContainsKey(eventData.Controller))
        {
            touchPoints.Remove(eventData.Controller);
            eventData.Use();
        }
    }

    public void OnTouchStarted(HandTrackingInputEventData eventData)
    {
        if (touchPoints.ContainsKey(eventData.Controller))
        {
            return;
        }
        
        touchPoints.Add(eventData.Controller, new TouchData(eventData.InputData));
        eventData.Use();
    }

    public void OnTouchUpdated(HandTrackingInputEventData eventData)
    {
        if (touchPoints.ContainsKey(eventData.Controller))
        {
            TouchData data = touchPoints[eventData.Controller];
            touchPoints[eventData.Controller] = data.Update(eventData.InputData);

            eventData.Use();
        }
    }
}
