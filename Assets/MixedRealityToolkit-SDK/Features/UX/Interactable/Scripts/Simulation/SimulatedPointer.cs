// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Core.Definitions.Physics;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.Devices;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.InputSystem;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.InputSystem.Handlers;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.Physics;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.TeleportSystem;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SDK.UX
{

    public class SimulatedPointer : MonoBehaviour, IMixedRealityPointer
    {
        public uint Id = 0;

        public IMixedRealityController Controller
        {
            get
            {
                throw new System.NotImplementedException();
            }

            set
            {
                throw new System.NotImplementedException();
            }
        }

        public uint PointerId => Id;

        public string PointerName { get { return name; } set { name = name; } }

        public IMixedRealityInputSource InputSourceParent => null;

        public IMixedRealityCursor BaseCursor { get { return null; } set { } }
        public ICursorModifier CursorModifier { get { return null; } set { } }
        public IMixedRealityTeleportHotSpot TeleportHotSpot { get { return null; } set { } }

        public bool IsInteractionEnabled => true;

        public bool IsFocusLocked { get { return false; } set { } }
        public float PointerExtent { get { return 0.1f; } set { } }

        public RayStep[] Rays => new RayStep[0];

        public LayerMask[] PrioritizedLayerMasksOverride { get { return new LayerMask[0]; } set { } }
        public IMixedRealityFocusHandler FocusTarget { get { return null; } set { } }
        public IPointerResult Result { get { return null; } set { } }
        public IBaseRayStabilizer RayStabilizer { get { return null; } set { } }
        public RaycastModeType RaycastMode { get { return RaycastModeType.Simple; } set { } }
        public float SphereCastRadius { get { return 0.1f; } set { } }

        public float PointerOrientation => 0.1f;

        public new bool Equals(object x, object y)
        {
            return x == y;
        }

        public int GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }

        public void OnPostRaycast()
        {
            // raycasting ?
        }

        public void OnPreRaycast()
        {
            // before raycasting?
        }

        public bool TryGetPointerPosition(out Vector3 position)
        {
            position = transform.position;
            return true;
        }

        public bool TryGetPointerRotation(out Quaternion rotation)
        {
            rotation = transform.rotation;
            return true;
        }

        public bool TryGetPointingRay(out Ray pointingRay)
        {
            pointingRay = new Ray(transform.position, transform.forward);
            return true;
        }
    }
}
