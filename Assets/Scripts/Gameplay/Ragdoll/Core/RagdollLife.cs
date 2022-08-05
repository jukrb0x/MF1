using System;
using UnityEngine;

namespace Gameplay.Ragdoll.Core
{
    /// <summary>
    ///  the ragdoll will be killed when in KillY
    /// </summary>
    public class RagdollLife : MonoBehaviour
    {
        private GameObject _stabilizer;
        public bool isWasted;


        private void KillRagdoll()
        {
            
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Kill Y
        }



    }
}
