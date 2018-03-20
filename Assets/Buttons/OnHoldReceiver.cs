using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HoloToolkit.Unity
{
    public class OnHoldReceiver : ReceiverBase
    {
        [InspectorField(Type = InspectorField.FieldTypes.Float, Label = "Hold Time", Tooltip = "The amount of time to press before triggering event")]
        public float HoldTime = 1f;

        private float clickTimer = 0;

        private bool hasDown;
        private State lastState;

        public OnHoldReceiver(UnityEvent ev): base(ev)
        {
            Name = "OnHold";
        }

        public override void OnUpdate(State state)
        {
            bool changed = state != lastState;
            
            if (state == InteractableStates.Press && !hasDown)
            {
                hasDown = true;
                clickTimer = 0;
            }
            else if(state != InteractableStates.Press)
            {
                hasDown = false;
            }

            if (hasDown && clickTimer < HoldTime)
            {
                clickTimer += Time.deltaTime;

                if (clickTimer >= HoldTime)
                {
                    Debug.Log("Hold!!");
                    uEvent.Invoke();
                }
            }
            
            lastState = state;
        }
    }
}
