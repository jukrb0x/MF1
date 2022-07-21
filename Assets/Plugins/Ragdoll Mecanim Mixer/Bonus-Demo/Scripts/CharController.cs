using UnityEngine;

namespace FightDemo {
    public abstract class CharController : MonoBehaviour {
        public bool isAttacking;
        public abstract void Die();
    }
}