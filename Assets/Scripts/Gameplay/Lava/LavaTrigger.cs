using System;
using UnityEngine;

namespace Gameplay.Lava
{
    public class LavaTrigger : BaseTrigger
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                onTriggerOn?.Invoke();
            }
        }

    }
}
