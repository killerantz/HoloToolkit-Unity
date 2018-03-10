using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloToolkit.Unity
{
    [System.Serializable]

    public class Interactable : MonoBehaviour
    {
        public bool Enabled;
        public InteractableStates State;
        public InteractionSourcePressInfo ButtonPressFilter = InteractionSourcePressInfo.Select;
        public bool IsGlobal = false;
        public int Dimensions = 1;
        public string VoiceCommand = "";
        public bool RequiresGaze = true;
        public List<ProfileItem> Profiles = new List<ProfileItem>();
        public List<InteractableEvent> Events = new List<InteractableEvent>();
        public List<ReceiverBase> Receivers = new List<ReceiverBase>();
        
        //collider checks and other alerts

        // state management

    }
}
