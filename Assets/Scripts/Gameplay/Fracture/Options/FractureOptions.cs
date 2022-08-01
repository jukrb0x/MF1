using System;
using UnityEngine;

namespace Gameplay.Fracture.Options
{
    public enum TriggerType
    {
        Collision,
    }
    
    [Serializable]
    public class FractureOptions
    {
        [Range(1, 100)]
        public int maxFractures;
        public bool detectFloatingFragment;
        public TriggerType triggerType;
        public float minCollisionForce;
        
        FractureOptions()
        {
            maxFractures = 50;
            detectFloatingFragment = true;
            triggerType = TriggerType.Collision;
            minCollisionForce = 1f;
        }
    }
}
