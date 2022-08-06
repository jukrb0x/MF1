using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gameplay.Fracture.Runtime.Scripts.Projectile
{
    public class ProjectilePool : MonoBehaviour
    {
        public static ProjectilePool PoolInstance;
        private       GameObject     _projectilePrefab;
        public        int            poolSize = 20;

        private List<GameObject> _projectilePool;
        private int              _currentIndex = 0;

        private void Awake()
        {
            PoolInstance = this;
#if UNITY_EDITOR
            _projectilePrefab = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Projectile.prefab", typeof(GameObject)) as GameObject;
#else
            _projectilePrefab = Resources.Load("Assets/Prefabs/Projectile.prefab", typeof(GameObject)) as GameObject;
#endif
            _projectilePool = new List<GameObject>();
            for (var i = 0; i < poolSize; i++)
            {
                var projectile = Instantiate(_projectilePrefab);
                projectile.SetActive(false);
                _projectilePool.Add(projectile);
            }
        }


        public GameObject GetProjectile()
        {
            if (_currentIndex >= _projectilePool.Count)
            {
                _currentIndex = 0;
            }
            var projectile = _projectilePool[_currentIndex];
            _currentIndex++;
            projectile.SetActive(true);
            return projectile;
        }

        // public GameObject GetProjectile()
        // {
        //     foreach (var projectile in _projectilePool)
        //     {
        //         if (!projectile.activeInHierarchy)
        //         {
        //             return projectile;
        //         }
        //     }
        //     return null;
        // }


    }
}
