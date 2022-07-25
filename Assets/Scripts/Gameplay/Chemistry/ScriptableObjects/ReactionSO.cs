using System.Collections.Generic;
using BaseClasses;
using UnityEngine;

namespace Gameplay.Chemistry.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Reaction", menuName = "Chemistry/Reaction", order = 0)]
    // this scriptable object defines the rule of the reaction
    public class ReactionSO : BaseScriptableObject
    {
    
        public List<Reactant> reactants;

        public List<Reactant> Reactants
        {
            get
            {
                reactants ??= new List<Reactant>();
                return null;
            }
        }
        

        public List<Product> products;
        
        public List<Product> Products
        {
            get
            {
                if (products == null)
                {
                    products = new List<Product>();
                }
                return products;
            }
        }
        
    }
}