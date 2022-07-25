using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Chemistry.ScriptableObjects;
using UnityEngine;

namespace Gameplay.Chemistry
{
    public class Reactor : MonoBehaviour
    {
        public ReactionSO reaction;
        private List<Reactant> reactants;
        private List<Product> products;
        public Transform productLocation;
        private int _coolDownTime;

        private void Start()
        {
            reactants = new List<Reactant>();
            products = new List<Product>();
            if (productLocation == null)
            {
                Transform[] t = transform.GetComponentsInChildren<Transform>();
                foreach (var i in t)
                {
                    if (i.gameObject.transform != null && i.gameObject.name == "ProductLocation")
                    {
                        productLocation = i.gameObject.transform;
                    }
                }
                
            }
            InvokeRepeating("CollDown", 1, 1);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("ChemicalObject")) return;
            // var reactant = other.GetComponent<Reactant>();
            other.TryGetComponent(out Reactant reactant);
            if (reactant == null) return;
            reactants.Add(reactant);
            // enough reactants to react
            if (reactants.Count == reaction.reactants.Count)
            {
                TryReact();
            }
        }

        private void OnTriggerExit(Collider other)
        { 
            
            if (!other.CompareTag("ChemicalObject")) return;
            other.TryGetComponent(out Reactant reactant);
            if (reactant != null && reactants.Contains(reactant))
            {
                Debug.Log("removing reactant");
                reactants.Remove(reactant);
            }
        }

        private void TryReact()
        {
            // compare with reaction rules
            List<string> reactionNamesByRule = reaction.reactants.Select(reactant => reactant.UniqueName).ToList();
            List<string> reactantNames = reactants.Select(reactant => reactant.UniqueName).ToList();
            // if all reactants are present in the reactor, ignore the order
            // SequenceEqual requires the order of items
            if (Enumerable.SequenceEqual(reactionNamesByRule.OrderBy(e => e), reactantNames.OrderBy(e => e)))
            {
                if(_coolDownTime > 0)
                {
                    return;
                }
                React();
            }
        }

        private void React()
        {
            Debug.Log("Reaction!");
            _coolDownTime = 10;
            // products = new List<Product>();
            // foreach (var product in reaction.products)
            // {
            //     var newProduct = Instantiate(product, transform.position, Quaternion.identity);
            //     newProduct.transform.parent = transform;
            //     products.Add(newProduct.GetComponent<Product>());
            // }
            
            foreach (var reactant in reactants)
            {
                Destroy(reactant.gameObject);
            }
            reactants.Clear();

            foreach (Product prod in reaction.products)
            {
                var o = productLocation;
                var newProd = Instantiate(prod, o.position, o.rotation);
            }
            
            
        }

        private void CollDown()
        {
            if(_coolDownTime > 0)
            {
                _coolDownTime--;
            }
        }
        
        
    }
}