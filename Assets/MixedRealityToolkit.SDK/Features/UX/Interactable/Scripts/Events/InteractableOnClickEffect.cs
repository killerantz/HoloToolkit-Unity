// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities.Editor;
using UnityEngine;
using UnityEngine.Events;

namespace Microsoft.MixedReality.Toolkit.UI
{
    /// <summary>
    /// OnClick the prefab will be instatiated at the location of the click event plus the EffectOffset value.
    /// The effect will face the user performing the click.
    /// This is an easy way to spawn objects or add effects.
    /// </summary>
    public class InteractableOnClickEffect : ReceiverBase
    {
        [InspectorField(Label = "Effect Prefab", Tooltip = "The effect prefab, should destroy itself", Type = InspectorField.FieldTypes.GameObject)]
        public GameObject EffectPrefab;

        [InspectorField(Label = "Offset Position", Tooltip = "Spawn the prefab relative to the Interactive position", Type = InspectorField.FieldTypes.Vector3)]
        public Vector3 EffectOffset = Vector3.zero;

        [InspectorField(Label = "Rotation Options", Tooltip = "How this prefab will rotate when initiated", Type = InspectorField.FieldTypes.DropdownInt, Options = new string[] { "None", "Parent Forward", "Camera Facing"})]
        public int RotationOptions = 0;

        [InspectorField(Label = "Rotation Offset", Tooltip = "Based on the rotation option, added rotation", Type = InspectorField.FieldTypes.Vector3)]
        public Vector3 RotationOffset = Vector3.zero;

        public InteractableOnClickEffect(UnityEvent ev): base(ev)
        {
            Name = "OnClick";
            HideUnityEvents = true;
        }

        public override void OnUpdate(InteractableStates state, Interactable source)
        {
            // using onClick
        }

        public override void OnClick(InteractableStates state, Interactable source, IMixedRealityPointer pointer = null)
        {
            if(EffectPrefab != null)
            {
                GameObject effect = GameObject.Instantiate(EffectPrefab);
                effect.transform.position = Host.transform.position + EffectOffset;
                switch (RotationOptions)
                {
                    case 1:
                        effect.transform.rotation = Host.transform.rotation * Quaternion.Euler(RotationOffset);
                        break;
                    case 2:
                        effect.transform.rotation = Quaternion.LookRotation(-(Camera.main.transform.position - effect.transform.position).normalized) * Quaternion.Euler(RotationOffset);
                        break;
                    default:
                        break;
                }
                
            }
        }
    }
}
