using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HoloToolkit.Unity
{
    public class OnHoldReceiver : ReceiverBase
    {
        [InspectorField(Type = InspectorField.FieldTypes.Float, Label = "Click Time")]
        public float clickTime = 0.5f;

        [InspectorField(Type = InspectorField.FieldTypes.Float, Label = "Click Prop")]
        public float clickProp { get; set; }

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

            bool hadDown = hasDown;
            if (lastState == InteractableStates.Focus && state == InteractableStates.Press)
            {
                hasDown = true;
            }
            else
            {
                hasDown = false;
            }

            if (hadDown && !hasDown && state == InteractableStates.Focus && clickTimer < clickTime)
            {
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
