using System;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.RayKit.Options
{
    [Serializable]
    public class HitOptions
    {
        public bool enabled;
        public GameObject hitObject;
        
        public UnityEvent onHit;

        public HitOptions()
        {
            onHit = null;
        }
    }
}
