using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ZoomReturn : MonoBehaviour
{
    public bool HasCollision;
    public ZoomToView ZoomReference;

    private bool hadCollistion;
    private IMixedRealityPointer currentPointer;
    private bool hasPress;

    /// <summary>
    /// Add pause or safe area to manager
    /// when in contact with manager, do something visual
    /// also pause, don't let the original Interactable do anything if interacted with again
    /// Make the placed object interactable so it can be picked up or clicked to return.
    /// Should get picked up and put back down or clicked.
    /// </summary>
    
    public void OnClick()
    {
        hasPress = false;
        EnableInteractable(false);
        ZoomReference.StopZoom();
    }

    public void OnPress(GameObject interactable)
    {
        Interactable button = interactable.GetComponent<Interactable>();
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

        hasPress = true;
    }

    public void OnRelease()
    {
        hasPress = false;
        ZoomReference.StopZoom();
    }

    private void Update()
    {
        if (hasPress)
        {
            transform.position = currentPointer.Position;
            transform.rotation = currentPointer.Rotation;
        }
        
        if (ZoomReference)
        {
            SafeArea safeArea = SafeAreaManager.SafeArea;
            if(safeArea != null)
            {
                Collider myCollider = GetComponent<Collider>();
                Bounds myBounds = myCollider.bounds;
                if (myCollider.enabled)
                {
                    HasCollision = safeArea.CheckCollision(myBounds);
                }
                else
                {
                    HasCollision = false;
                }
            }
            else
            {
                HasCollision = false;
            }
        }
        else
        {
            HasCollision = false;
        }

        if(hadCollistion != HasCollision)
        {

        }

        hadCollistion = HasCollision;
    }

    public void EnableInteractable(bool enable)
    {
        Interactable interactable = GetComponent<Interactable>();
        interactable.enabled = enable;
    }

    public void EnableCollider(bool enable)
    {
        Collider collider = GetComponent<Collider>();
        collider.enabled = enable;
    }
}
