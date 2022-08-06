using UnityEngine;

namespace Gameplay.Treasure
{
    public class TreasureTrigger : BaseTrigger
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                GameManager.Instance.GameWin();
            }
        }

    }
}
