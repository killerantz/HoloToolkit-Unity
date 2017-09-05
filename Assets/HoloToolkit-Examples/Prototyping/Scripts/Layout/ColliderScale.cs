// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloToolkit.Examples.Prototyping
{
    [ExecuteInEditMode]
    public class ColliderScale : MonoBehaviour
    {
        [Tooltip("the object to copy the scale from")]
        public Transform CopyFrom;

        [Tooltip("the percentage amounts to offset the scale")]
        public Vector3 ScaleFactor = Vector3.one;

        [Tooltip("should this only run in Edit mode, to avoid updating as items move?")]
        public bool OnlyInEditMode;

        private Collider copyTo;

        private void Awake()
        {
            copyTo = GetComponent<Collider>();
        }

        private void SetScale()
        {
            if (copyTo != null && CopyFrom != null)
            {
                BoxCollider box = copyTo as BoxCollider;
                if (box != null)
                {
                    box.size = Vector3.Scale(CopyFrom.transform.localScale, ScaleFactor);
                    return;
                }

                CapsuleCollider capsule = copyTo as CapsuleCollider;
                if (capsule != null)
                {
                    capsule.radius = CopyFrom.transform.localScale.x * ScaleFactor.x;
                    capsule.height = CopyFrom.transform.localScale.y * ScaleFactor.y;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
#if UNITY_EDITOR
            if ((Application.isPlaying && !OnlyInEditMode) || (!Application.isPlaying))
            {
                SetScale();
            }
#else
                SetScale();
#endif
        }
    }
}
