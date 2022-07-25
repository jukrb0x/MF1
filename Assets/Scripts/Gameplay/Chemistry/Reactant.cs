using System;
using Gameplay.Chemistry.ScriptableObjects;
using UnityEngine;

namespace Gameplay.Chemistry
{
    public class Reactant : MonoBehaviour
    {
        public enum ReactantType
        {
            SimpleElement = 0,
            CompositeElement = 1
        }
        // property name
        public string UniqueName;
        // internal name to identify the reactant

        public ReactantType type;
        private GameObject _reactantObject;

        private void Start()
        {
            _reactantObject = gameObject;
            UniqueName = UniqueName.Trim();
        }
        
        public bool CompareReactant(Reactant reactant)
        {
            return reactant.UniqueName == UniqueName;
        }
    }
}