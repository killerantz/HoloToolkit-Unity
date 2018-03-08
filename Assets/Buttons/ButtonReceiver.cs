using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Buttons
{
    public enum InspectorTypes {Float, Int, String, Property, Drop, Bool }
    public class InspectorField : Attribute
    {
        public InspectorTypes Type { get; set; }
        public string Label { get; set; }
    }

    [System.Serializable]
    public class ButtonReceiver
    {
        // make public
        [InspectorField(Type = InspectorTypes.Float, Label = "Click Time")]
        public float clickTime = 0.5f;

        [InspectorField(Type = InspectorTypes.Float, Label = "Click Prop")]
        public float clickProp { get; set; }

        private float clickTimer = 0;
        private UnityEvent onClick;

        private bool hasDown;
        private State lastState;

        public ButtonReceiver(UnityEvent ev)
        {
            onClick = ev;
        }

        public void OnUpdate(State state)
        {
            bool changed = state != lastState;

            bool hadDown = hasDown;
            if (lastState == InteractiveStates.Focus && state == InteractiveStates.Press)
            {
                hasDown = true;
            }
            else
            {
                hasDown = false;
            }

            if (hadDown && !hasDown && state == InteractiveStates.Focus && clickTimer < clickTime)
            {
                Debug.Log("CLICKED!!!!!! / " + onClick);
                onClick.Invoke();
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
