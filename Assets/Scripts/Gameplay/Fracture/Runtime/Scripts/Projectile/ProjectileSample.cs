using UnityEngine;

namespace Gameplay.Fracture.Runtime.Scripts.Projectile
{
    public class ProjectileSample : MonoBehaviour
    {
        
        public static ProjectileSample Instance;
        public GameObject projectilePrefab;
        
        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
           
        }
    }
}
