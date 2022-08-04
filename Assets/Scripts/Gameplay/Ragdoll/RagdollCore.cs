using System;
using UnityEngine;

namespace Gameplay.Ragdoll
{
    // Base class for ragdoll system
    public class RagdollCore : MonoBehaviour
    {

        public Ragdoll ragdoll;


        // todo:
        // 1. binding of animation and bones(physical)
        // 2. self balancing
        // 3. move and grab things
        // 4*. throw things (add a force when throw maybe)

        private void OnValidate()
        {
            if (ragdoll == null)
            {
                if (!TryGetComponent<Ragdoll>(out ragdoll)) Debug.LogWarning("Ragdoll is null.");
            }
        }

    }
}