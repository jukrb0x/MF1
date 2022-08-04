using Gameplay.Ragdoll.Core;
using Gameplay.Ragdoll.Input;
using UnityEngine;

namespace Gameplay.Ragdoll
{
    // The Ragdoll System Entry
    // Configure body parts and their connections
    [RequireComponent(typeof(Movement))]
    [RequireComponent(typeof(RagdollPhysics))]
    [RequireComponent(typeof(Body))]
    [RequireComponent(typeof(RagdollAnimation))]
    [RequireComponent(typeof(RagdollInputs))]
    public class Ragdoll : MonoBehaviour
    {

        // Ragdoll Components
        [Header("Ragdoll System")]
        public RagdollInputs    inputs;
        public RagdollAnimation ragdollAnimation;
        public Body             ragdollBody;
        public RagdollPhysics   ragdollPhysics;
        public Movement         ragdollMovement;

        // Ragdoll Internals
        
        private void Awake()
        {
            // _floorCheck = GetComponent<FloorCheck>();
        }
        
        

    }
}
