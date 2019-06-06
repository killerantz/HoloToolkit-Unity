using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactToTouchTracker : MonoBehaviour
{
    public enum TrackerType { All, Position, Rotation, XAxis, YAxis, ZAxis}
    public TouchTracker Tracker;
    public TrackerType TrackingPreference;

    private void Update()
    {
        Quaternion rotation = transform.rotation;
        Vector3 position = transform.position;

        Vector3 worldPosition = Tracker.transform.TransformPoint(Tracker.TouchPosition);

        if (Tracker.HasTouch)
        {
            print(Tracker.DirectionPercentage + " / " + Tracker.PercentageFromCenter);
        }

        switch (TrackingPreference)
        {
            case TrackerType.All:
                position = worldPosition;
                break;
            case TrackerType.Position:
                position = worldPosition;
                break;
            case TrackerType.Rotation:
                rotation = Quaternion.FromToRotation(Vector3.up, worldPosition - Tracker.transform.position);
                break;
            case TrackerType.XAxis:
                position = new Vector3(worldPosition.x, position.y, position.z);
                break;
            case TrackerType.YAxis:
                position = new Vector3(position.x, worldPosition.y, position.z);
                break;
            case TrackerType.ZAxis:
                position = new Vector3(position.x, position.y, worldPosition.z);
                break;
            default:
                break;
        }

        transform.position = position;
        transform.rotation = rotation;
    }
}
