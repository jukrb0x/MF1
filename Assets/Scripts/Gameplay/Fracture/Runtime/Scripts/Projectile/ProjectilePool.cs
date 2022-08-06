using System;
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
        public        GameObject     projectilePrefab;
        public        int            poolSize = 20;

        private List<GameObject> _projectilePool;
        private int              _currentIndex = 0;

        private void Awake()
        {
            PoolInstance = this;
            // if (projectilePrefab == null)
            // {
            //     // workaround
            //     while(projectilePrefab == null) 
            //         projectilePrefab = ProjectileSample.Instance.projectilePrefab;
            //     projectilePrefab.AddComponent<ProjectileController>();
            // }
            _projectilePool = new List<GameObject>();
            for (var i = 0; i < poolSize; i++)
            {
                if (projectilePrefab != null)
                {
                    var projectile = Instantiate(projectilePrefab);
                    projectile.SetActive(false);
                    _projectilePool.Add(projectile);
                }
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
