using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloToolkit.Unity
{
    [CreateAssetMenu(fileName = "States", menuName = "Interactable/State", order = 1)]
    public class States : ScriptableObject
    {
        public StateModel StateLogic;
        public Type StateType;
        public String[] Options;
        public List<State> StateList;
        public int DefaultIndex = 0;

        public State CompareStates(bool[] states)
        {
            if (StateLogic == null)
            {
                StateLogic = (StateModel)Activator.CreateInstance(StateType, StateList[DefaultIndex]);
            }
            return StateLogic.CompareStates(states);
        }

        public State[] GetStates()
        {
            return StateList.ToArray();
        }

    }
}
