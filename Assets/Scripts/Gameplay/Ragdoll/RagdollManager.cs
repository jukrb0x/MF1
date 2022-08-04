using Gameplay.Ragdoll.Core;
using Gameplay.Ragdoll.Input;
using UnityEngine;

namespace Gameplay.Ragdoll
{
    [RequireComponent(typeof(Body))]
    [RequireComponent(typeof(RagdollInputs))]
    public class RagdollManager : MonoBehaviour
    {
        // binding core components here
        public RagdollInputs         ragdollInput;
        public RagdollAnimation ragdollAnimation;
        public Body             ragdollBody;
            
        private Ground      _ground;

        
        private void Awake()
        {
            // _floorCheck = GetComponent<FloorCheck>();
        }


    }
}
