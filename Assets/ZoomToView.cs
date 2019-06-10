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
        }
    }

    public void StopZoom()
    {
        if(zoomedObject != null)
        {
            ObjectToZoom.transform.parent = objectParent;
            //ObjectToZoom.transform.localPosition = cachedLocalPosition;
            ObjectToZoom.transform.localRotation = cachedLocalRotation;
            Destroy(zoomedObject);
            zoomedObject = null;
        }
    }

    private void Update()
    {
        float decay = Mathf.Pow(10, -ZoomSpeed);
        float lerpSpeed = 1 - Mathf.Pow(decay, Time.deltaTime);


        if (zoomedObject != null && currentPointer != null)
        {
            zoomedObject.transform.position = currentPointer.Position;
            zoomedObject.transform.rotation = currentPointer.Rotation;
            

            ObjectToZoom.transform.localPosition = Vector3.Lerp(ObjectToZoom.transform.localPosition, Vector3.zero, lerpSpeed);
        }
        else
        {
            ObjectToZoom.transform.localPosition = Vector3.Lerp(ObjectToZoom.transform.localPosition, cachedLocalPosition, lerpSpeed);
        }
    }
}
