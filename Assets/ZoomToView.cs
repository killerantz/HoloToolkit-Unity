using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomToView : MonoBehaviour
{
    public Transform ObjectToZoom;
    public GameObject ZoomContainerPrefab;
    public float ZoomSpeed = 3;

    private GameObject zoomedObject;
    private Transform objectParent;
    private IMixedRealityPointer currentPointer;
    private Vector3 cachedLocalPosition;
    private Quaternion cachedLocalRotation;

    private ZoomReturn objectManager;
    private GameObject safeArea;
    private bool hadZoom = false;
    private bool isZooming = false;

    public void StartZoom(GameObject interactable)
    {
        if(zoomedObject == null)
        {
            zoomedObject = Instantiate(ZoomContainerPrefab);
            Interactable button = interactable.GetComponent<Interactable>();
            currentPointer = null;
            foreach (IMixedRealityInputSource source in button.PressingInputSources)
            {
                for (int i = 0; i < source.Pointers.Length; i++)
                {
                    if (source.Pointers[i] is PokePointer)
                    {
                        currentPointer = source.Pointers[i];
                        break;
                    }
                    
                }
            }

            if(currentPointer != null)
            {
                zoomedObject.transform.position = currentPointer.Position;
                zoomedObject.transform.rotation = currentPointer.Rotation;
            }
            
            cachedLocalPosition = ObjectToZoom.transform.localPosition;
            cachedLocalRotation = ObjectToZoom.transform.localRotation;
            objectParent = ObjectToZoom.transform.parent;
            ObjectToZoom.parent = zoomedObject.transform;

            ZoomReturn zoom = zoomedObject.GetComponent<ZoomReturn>();
            zoom.ZoomReference = this;
            zoom.EnableCollider(true);
            hadZoom = true;
            isZooming = true;
        }
    }

    public void StopZoom()
    {
        if(zoomedObject != null && !HasCollision())
        {
            ObjectToZoom.transform.parent = objectParent;
            ObjectToZoom.transform.localRotation = cachedLocalRotation;
            ObjectToZoom.transform.localPosition = cachedLocalPosition;
            Destroy(zoomedObject);
            zoomedObject = null;
        }
        else
        {
            ZoomReturn zoom = GetZoom();
            if(zoom != null)
            {
                zoom.EnableInteractable(true);
            }
        }

        isZooming = false;
    }

    private ZoomReturn GetZoom()
    {
        ZoomReturn zoom = null;
        if (zoomedObject != null)
        {
            zoom = zoomedObject.GetComponent<ZoomReturn>();
        }

        return zoom;
    }

    private bool HasCollision()
    {
        bool hasCollision = false;
        ZoomReturn zoom = GetZoom();
        if (zoom != null)
        {
            hasCollision = zoom.HasCollision;
        }

        return hasCollision;
    }

    private void Update()
    {
        float decay = Mathf.Pow(10, -ZoomSpeed);
        float lerpSpeed = 1 - Mathf.Pow(decay, Time.deltaTime);
        
        if (zoomedObject != null && currentPointer != null && isZooming)
        {
            zoomedObject.transform.position = currentPointer.Position;
            zoomedObject.transform.rotation = currentPointer.Rotation;
            
            ObjectToZoom.transform.localPosition = Vector3.Lerp(ObjectToZoom.transform.localPosition, Vector3.zero, lerpSpeed);
            // lerp rotation?
        }
        else
        {
            if (hadZoom && !HasCollision())
            {
                ObjectToZoom.transform.localPosition = Vector3.Lerp(ObjectToZoom.transform.localPosition, cachedLocalPosition, lerpSpeed);
                // Add rotation over time
                if (ObjectToZoom.transform.localPosition == cachedLocalPosition)
                {
                    hadZoom = false;
                }
            }
        }
    }
}
