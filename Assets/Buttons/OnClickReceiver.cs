using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HoloToolkit.Unity
{
    public class OnClickReceiver : ReceiverBase
    {
        [InspectorField(Type = InspectorField.FieldTypes.Float, Label = "Click Time", Tooltip = "The press and release should happen within this time")]
        public float ClickTime = 0.5f;

        private float clickTimer = 0;

        private bool hasDown;
        private State lastState;

        public OnClickReceiver(UnityEvent ev): base(ev)
        {
            Name = "OnClick";
        }

        public override void OnUpdate(State state)
        {
            bool changed = state != lastState;
            
            bool hadDown = hasDown;
            hasDown = state == InteractableStates.Press;

            if (hadDown && !hasDown && state == InteractableStates.Focus && clickTimer < ClickTime)
            {
                Debug.Log("Click!");
                uEvent.Invoke();
            }

            if (!hasDown)
            {
                clickTimer = 0;
            }
            else
            {
                clickTimer += Time.deltaTime;
            }

            lastState = state;
        }
    }
}
