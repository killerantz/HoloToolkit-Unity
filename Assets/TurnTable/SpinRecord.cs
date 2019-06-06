using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SpinRecord : MonoBehaviour
{
    public TouchTracker Tracker;
    public float AnimationSpeed = 3;
    public float ScratchSpeed = 3;
    public Vector3 RotationClamp = Vector3.one;
    public Vector3 RotationAxis = Vector3.up;
    public bool Playing = true;

    private AudioSource track;

    private bool hasTouch = false;
    private Quaternion startRotation;
    private float currentAngle = 0;
    private Vector3 startDirection = Vector3.zero;
    private Vector3 startPosition;
    private Vector3 lastDirection = Vector3.zero;
    private float lastPlaySpeed = 0;

    private void Update()
    {
        if (!Playing)
        {
            return;
        }
        Quaternion rotation = Quaternion.identity;

        if (Tracker.HasTouch)
        {
            Vector3 worldPosition = Tracker.transform.TransformPoint(Tracker.TouchPosition);
            
            if (!hasTouch)
            {
                startRotation = transform.rotation;
                startDirection = Vector3.Scale(RotationClamp, worldPosition - Tracker.transform.position);
                lastDirection = startDirection;
                
                if (worldPosition != Tracker.transform.position)
                {
                    hasTouch = true;
                }
            }
            else
            {
                Vector3 clampedDirection = Vector3.Scale(RotationClamp, worldPosition - Tracker.transform.position);
                Quaternion scratch = Quaternion.FromToRotation(startDirection, clampedDirection);
                rotation = startRotation * scratch;
                
                transform.rotation = rotation;
                
                float newPlaySpeed = Vector3.Angle(lastDirection, clampedDirection) / ScratchSpeed;
                float spinDirection = GetDot(lastDirection, clampedDirection);

                if (spinDirection < 0 || (lastPlaySpeed < 0 && spinDirection < 0.0001f))
                {
                    if (lastPlaySpeed < 0)
                    {
                        lastPlaySpeed = newPlaySpeed;
                    }
                }
                else
                {
                    if (newPlaySpeed > 0)
                    {
                        newPlaySpeed = -newPlaySpeed;
                    }

                    if (lastPlaySpeed > 0)
                    {
                        lastPlaySpeed = newPlaySpeed;
                    }
                }

                float decay = Mathf.Pow(10, -ScratchSpeed);
                float lerpSpeed = 1 - Mathf.Pow(decay, Time.deltaTime);
                float playSpeed = Mathf.Lerp(lastPlaySpeed, newPlaySpeed, lerpSpeed);
                
                track = GetComponent<AudioSource>();
                track.pitch = Mathf.Clamp(playSpeed, -3, 3);
                
                Vector3.Angle(clampedDirection, lastDirection);
                lastDirection = clampedDirection;
                lastPlaySpeed = playSpeed;

            }
        }
        else
        {
            if (hasTouch)
            {
                currentAngle = transform.rotation.eulerAngles.magnitude;
            }
            else
            {
                currentAngle += -AnimationSpeed * Time.deltaTime;
            }

            rotation = Quaternion.AngleAxis(currentAngle, RotationAxis);

            hasTouch = false;

            track = GetComponent<AudioSource>();
            track.pitch = 1;
            transform.rotation = rotation;
        }
    }

    private float GetDot(Vector3 start, Vector3 current)
    {
        Quaternion quad90 = Quaternion.AngleAxis(90, RotationAxis);
        
        Vector3 dotDirection = quad90 * start;
        return Vector3.Dot(dotDirection, current);
    }
}
