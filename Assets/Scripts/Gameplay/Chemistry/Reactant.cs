using System;
using Gameplay.Chemistry.ScriptableObjects;
using UnityEngine;

namespace Gameplay.Chemistry
{
    public class Reactant : MonoBehaviour
    {
        public enum ReactantType
        {
            SimpleElement    = 0,
            CompositeElement = 1
        }

        // property name
        public  string       uniqueName;
        public  ReactantType type;
        private GameObject   _reactantObject;

        private void Start()
        {
            _reactantObject = gameObject;
            // use to identify the unique reactant
            // todo: use hashcode instead of string
            uniqueName = uniqueName.Trim();
            if (uniqueName == "")
            {
                uniqueName = _reactantObject.name;
            }

        }

        public bool CompareReactant(Reactant reactant)
        {
            return reactant.uniqueName == uniqueName;
        }
    }
}
