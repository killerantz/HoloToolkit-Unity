using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HoloToolkit.Unity
{
    [System.Serializable]
    public class InteractableEvent
    {
        public string Name;
        public UnityEvent Event;
        public ReceiverBase Receiver;

        public static implicit operator UnityEngine.Object(InteractableEvent v)
        {
            throw new NotImplementedException();
        }
    }
}
