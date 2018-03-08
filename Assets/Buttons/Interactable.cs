using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.State;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloToolkit.Unity
{
    [System.Serializable]
    public enum InteractableStates { Default, Focus, Press, Disabled }

    public class Interactable : MonoBehaviour
    {
        public InteractableStates State;
        public InteractionSourcePressInfo ButtonPressFilter = InteractionSourcePressInfo.Select;
        public bool IsGlobal = false;
        public List<ProfileItem> Profiles;
        public List<InteractableEvents> Events;
        

        //collider checks and other alerts

        // state management

        private void Awake()
        {
            if (Profiles == null)
            {
                Profiles = new List<ProfileItem>();
                ProfileItem item = new ProfileItem();
                item.Themes = new List<Theme>();

                Profiles.Add(item);
            }   
        }
    }
}
