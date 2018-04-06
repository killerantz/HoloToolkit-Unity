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
        public List<State> StateList;
        public int DefaultIndex = 0;
        public Type StateType;
        public string[] StateOptions;
        public Type[] StateTypes;
        public string StateLogicName = "InteractableStates";

        //!!! finish making states work, they shoulg initiate the type and run the logic during play mode.

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

        public void SetupStateOptions()
        {
            List<Type> stateTypes = new List<Type>();
            List<string> names = new List<string>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsSubclassOf(typeof(StateModel)))
                    {
                        stateTypes.Add(type);
                        names.Add(type.Name);
                    }
                }
            }

            StateOptions = names.ToArray();
            StateTypes = stateTypes.ToArray();
        }

        // redundant method, put in a utils with static methods!!!
        public static int ReverseLookup(string option, string[] options)
        {
            for (int i = 0; i < options.Length; i++)
            {
                if (options[i] == option)
                {
                    return i;
                }
            }

            return 0;
        }

    }
}
