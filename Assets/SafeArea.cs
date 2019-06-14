using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SafeArea : MonoBehaviour
{
    private void OnEnable()
    {
        SafeAreaManager.SafeArea = this;
    }

    public bool CheckCollision(Bounds bounds)
    {
        Bounds myBounds = GetComponent<Collider>().bounds;
        return myBounds.Intersects(bounds);
    }
}
