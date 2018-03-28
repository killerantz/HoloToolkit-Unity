// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloToolkit.Unity
{
    [System.Serializable]
    public class State
    {
        public string Name;
        public int Index;
        public int Bit;

        public override string ToString()
        {
            return Name;
        }

        public int ToInt()
        {
            return Index;
        }

        public int ToBit()
        {
            return Bit;
        }
    }

    public abstract class StateModel
    {
        protected State currentState;

        public StateModel(State defaultState)
        {
            currentState = defaultState;
        }
        
        public virtual void SetSate(State state)
        {
            currentState = state;
        }
        
        public virtual State GetState()
        {
            return currentState;
        }

        public abstract State CompareStates(bool[] states);

        public abstract State[] GetStates();

        protected int GetBit(bool[] states)
        {
            int bit = 0;
            int bitCount = 0;
            for (int i = 0; i < states.Length; i++)
            {
                if (i == 0)
                {
                    bit += 1;
                }
                else
                {
                    bit += bit;
                }

                if (states[i])
                {
                    bitCount += bit;
                }
            }

            return bitCount;
        }
    }
}
