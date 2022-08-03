using System;
using UnityEngine;

namespace Gameplay.Ragdoll
{
    public class RagdollCore : MonoBehaviour
    {

        public RagdollInputs inputs;
        // todo:
        // 1. binding of animation and bones(physical)
        // 2. self balancing
        // 3. move and grab things
        // 4*. throw things (add a force when throw maybe)

        private void Start()
        {
            inputs.OnFloorDelegates += OnFloorExecutor;

        }

        private void OnFloorExecutor(bool isOnFloor)
        { 
            //todo buffer
        }

    }
}
