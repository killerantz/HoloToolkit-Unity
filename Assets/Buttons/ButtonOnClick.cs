using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Buttons
{
    public class ButtonOnClick : MonoBehaviour, IInputClickHandler
    {
        public UnityEvent OnClick;

        public void OnInputClicked(InputClickedEventData eventData)
        {
            print("ON CLICK EVENT!!! ??????");
            OnClick.Invoke();
        }
    }
}
