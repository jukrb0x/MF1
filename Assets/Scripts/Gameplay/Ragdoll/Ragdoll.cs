using Gameplay.Ragdoll.Input;
using UnityEngine;

namespace Gameplay.Ragdoll
{
    // The Ragdoll System Entry
    // Configure body parts and their connections
    [RequireComponent(typeof(RagdollManager))]
    public class Ragdoll : MonoBehaviour
    {

        public RagdollInputs inputs;
        // exposing apis
        [Header("Common Variables")]
        public float movementSpeed;



        private void Awake()
        {
            // _floorCheck = GetComponent<FloorCheck>();
        }


    }
}
