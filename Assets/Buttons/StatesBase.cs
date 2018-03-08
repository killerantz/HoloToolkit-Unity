using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buttons
{
    public class State
    {
        public int Index;
        public int Bit;
        public string Name;

        public override string ToString()
        {
            return Name;
        }

        public int ToInt()
        {
            return Index;
        }
    }

    public abstract class StatesBase
    {
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

        public abstract State GetState(bool[] states);
        
        public abstract State[] GetStates();
    }
}
