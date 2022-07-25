using System;
using UnityEngine;

namespace Gameplay.Chemistry
{
    public class Product : MonoBehaviour
    {
        private GameObject productPrefab;
        
        // todo: add triggers...

        private void Start()
        {
            productPrefab = gameObject;
        }
    }
}