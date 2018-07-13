﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloToolkit.Unity
{
    public class InteractableStates : StateModel
    {
        public enum InteractableStateEnum {
            /// <summary>
            /// Default state, nothing happening
            /// </summary>
            Default,
            /// <summary>
            /// Looking at object
            /// </summary>
            Focus,
            /// <summary>
            /// Looking at object and finger down
            /// </summary>
            Pressed,
            /// <summary>
            /// Looking at and finger up
            /// </summary>
            Targeted,
            /// <summary>
            /// Not looking at it and finger is up
            /// </summary>
            Interactive,
            /// <summary>
            /// Looking at button finger down
            /// </summary> 
            ObservationTargeted,
            /// <summary>
            /// Not looking at it and finger down
            /// </summary>
            Observation,
            /// <summary>
            /// Button in a disabled state
            /// </summary>
            Disabled,
            /// <summary>
            /// Button was clicked already
            /// </summary>
            Visited,
            /// <summary>
            /// Botton is toggled state, on/off
            /// </summary>
            Toggled,
            /// <summary>
            /// Gesture is happening, Move
            /// </summary>
            Gesture,
            /// <summary>
            /// Gesture has reached it's max movement
            /// </summary>
            GestureMax,
            /// <summary>
            /// There is a collision
            /// </summary>
            Collition,
            /// <summary>
            /// Custom placeholder for anything
            /// </summary>
            Custom
        }

        protected new State[] allStates = new State[14]
        {
            new State(){ Index = 0, Name = "Default", ActiveIndex = -1, Bit = 0, Value = 0},
            new State(){ Index = 1, Name = "Focus", ActiveIndex = -1, Bit = 0, Value = 0},
            new State(){ Index = 2, Name = "Pressed", ActiveIndex = -1, Bit = 0, Value = 0},
            new State(){ Index = 3, Name = "Targeted", ActiveIndex = -1, Bit = 0, Value = 0},
            new State(){ Index = 4, Name = "Interactive", ActiveIndex = -1, Bit = 0, Value = 0},
            new State(){ Index = 5, Name = "ObservationTargeted", ActiveIndex = -1, Bit = 0, Value = 0},
            new State(){ Index = 6, Name = "Observation", ActiveIndex = -1, Bit = 0, Value = 0},
            new State(){ Index = 7, Name = "Disabled", ActiveIndex = -1, Bit = 0, Value = 0},
            new State(){ Index = 8, Name = "Visited", ActiveIndex = -1, Bit = 0, Value = 0},
            new State(){ Index = 9, Name = "Toggled", ActiveIndex = -1, Bit = 0, Value = 0},
            new State(){ Index = 10, Name = "Gesture", ActiveIndex = -1, Bit = 0, Value = 0},
            new State(){ Index = 11, Name = "GestureMax", ActiveIndex = -1, Bit = 0, Value = 0},
            new State(){ Index = 12, Name = "Collition", ActiveIndex = -1, Bit = 0, Value = 0},
            new State(){ Index = 13, Name = "Custom", ActiveIndex = -1, Bit = 0, Value = 0}
        };

        public InteractableStates(State defaultState) : base(defaultState)
        {
            base.allStates = allStates;
        }

        public virtual void SetStateOn(InteractableStateEnum state)
        {
            SetStateOn((int)state);
        }

        public virtual void SetStateOff(InteractableStateEnum state)
        {
            SetStateOff((int)state);
        }

        public virtual void SetStateValue(InteractableStateEnum state, int value)
        {
            SetStateValue((int)state, value);
        }

        public State GetState(InteractableStateEnum state)
        {
            return GetState((int)state);
        }

        public override State CompareStates()
        {
            int bit = GetBit();

            currentState = stateList[0];

            for (int i = stateList.Count-1; i > -1; i--)
            {
                if (bit >= stateList[i].Bit)
                {
                    currentState = stateList[i];
                    break;
                    //return currentState;
                }
            }

            return currentState;
        }
        
        public override State[] GetStates()
        {
            return stateList.ToArray();
        }
    }
}
