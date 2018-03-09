using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloToolkit.Unity
{
    public class InteractableStates : StateModel
    {
        public static State Default = new State() { Index = 0, Bit = 0, Name = "Default" };
        public static State Focus = new State() { Index = 1, Bit = 1, Name = "Focus" };
        public static State Press = new State() { Index = 2, Bit = 2, Name = "Press" };
        public static State Disabled = new State() { Index = 3, Bit = 4, Name = "Disabled" };

        public override State CompareStates(bool[] states)
        {
            int bit = GetBit(states);

            if (bit >= Disabled.Bit)
            {
                return Disabled;
            }

            if (bit == (Focus.Bit | Press.Bit))
            {
                return Press;
            }

            if (bit == Focus.Bit)
            {
                return Focus;
            }

            return Default;
        }

        public override State[] GetStates()
        {
            return new State[] { Default, Focus, Press, Disabled };
        }
    }
}
