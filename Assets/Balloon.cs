using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balloon : MonoBehaviour
{
    public Vector3 ForceDirectoin = Vector3.up;
    public Transform ReferenceHeight;
    public float Velocity = 3;
    public bool SmoothDamp = true;

    public float Radius = 0.1f;
    public float Power = 0.5f;

    private float lerpSpeed;
    private Vector3 currentVelocity = Vector3.zero;

    private void Start()
    {
        if (ReferenceHeight == null)
        {
            ReferenceHeight = Camera.main.transform;
        }

        
    }

    /// <summary>
    /// Gage distance
    /// FilterDirection
    /// Apply counter forces to dampen
    /// ease back to zero before reaching desination
    /// calculate velocity and time to target
    /// should not need max force, should figure it out on our own,
    /// but may need some parameters
    /// </summary>

    // Update is called once per frame
    void FixedUpdate()
    {
        float decay = Mathf.Pow(10, -Velocity);
        lerpSpeed = 1 - Mathf.Pow(decay, Time.deltaTime);

        ExplosiveForce();
        //ApplyForce();
    }

    private void ExplosiveForce()
    {
        Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, Radius);
        print(colliders.Length);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null)
            {
                print(rb.name + " / " + rb.velocity + " / " + rb.velocity.x);
                rb.AddExplosionForce(Power, explosionPos, Radius, 3.0F);
            }
        }
    }

    private void ApplyForce()
    {
        Rigidbody rigidbody = GetComponent<Rigidbody>();

        // distance
        Vector3 distance = Vector3.Scale(transform.position - ReferenceHeight.position, ForceDirectoin);

        // time
        Vector3 time = distance / lerpSpeed;

        // counter force divide by time
        Vector3 counterForce = Vector3.zero;
        if (SmoothDamp)
        {
            counterForce = -Vector3.SmoothDamp(distance, Vector3.zero, ref currentVelocity, lerpSpeed);
        }
        else
        {
            counterForce = -Vector3.Lerp(distance, Vector3.zero, lerpSpeed) / time.magnitude;
        }

        rigidbody.AddForce(counterForce);
    }
}
