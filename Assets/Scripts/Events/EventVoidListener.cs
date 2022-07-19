using System;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Events
{
    public class EventVoidListener : MonoBehaviour
    {

        public UnityEvent OnEventRaised;

        private void OnEnable()
        {
            
        }
    }
}