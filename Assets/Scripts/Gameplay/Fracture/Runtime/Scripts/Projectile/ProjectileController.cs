using System;
using System.Collections;
using UnityEngine;

namespace Gameplay.Fracture.Runtime.Scripts.Projectile
{
    public class ProjectileController : MonoBehaviour
    {
        public float lifeSeconds = 1f;
        public float localScale = 1f;
        private void OnEnable()
        {
            StartCoroutine(AutoDeactivate());
        }
        
        IEnumerator AutoDeactivate()
        {
            yield return new WaitForSeconds(lifeSeconds);
            gameObject.SetActive(false);
        }


    }
}
