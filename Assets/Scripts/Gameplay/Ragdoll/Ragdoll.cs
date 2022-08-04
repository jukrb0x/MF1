using Gameplay.Ragdoll.Core;
using Gameplay.Ragdoll.Input;
using UnityEngine;

namespace Gameplay.Ragdoll
{
    // The Ragdoll System Entry
    // Configure body parts and their connections
    [RequireComponent(typeof(RagdollMovement))]
    [RequireComponent(typeof(RagdollPhysics))]
    [RequireComponent(typeof(RagdollBody))]
    [RequireComponent(typeof(RagdollAnimation))]
    [RequireComponent(typeof(RagdollInputs))]
    public class Ragdoll : MonoBehaviour
    {

        // Ragdoll Components
        [Header("Ragdoll System")]
        public RagdollInputs    inputs;
        public RagdollAnimation ragdollAnimation;
        public RagdollBody             ragdollBody;
        public RagdollPhysics   ragdollPhysics;
        public RagdollMovement         ragdollMovement;

        // Ragdoll Internals
        
        private void Awake()
        {
            // _floorCheck = GetComponent<FloorCheck>();
        }
        
        

    }
}
