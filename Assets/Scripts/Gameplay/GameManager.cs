using System;
using Gameplay.Fracture.Runtime.Scripts.Projectile;
using UnityEngine;

namespace Gameplay
{
    public enum GAME_STATE
    {
        PLAYING,
        PAUSED,
        WIN,
        LOSE,
    }

    // control the game flow
    public class GameManager : MonoBehaviour
    {
        private GAME_STATE _gameState;

        private void Start()
        {
            // global projectile pool, for the ray emitter
            var projectilePool = new GameObject("Projectile Pool")
            {
                transform =
                {
                    position = Vector3.zero,
                    localScale = Vector3.one,
                    localRotation = Quaternion.identity
                }
            };
            Instantiate(projectilePool);
            projectilePool.AddComponent<ProjectilePool>();
        }

        private void Update()
        {
            if (_gameState == GAME_STATE.WIN)
                Debug.Log("You Win!");
        }

        public void SetGameState(GAME_STATE gameState)
        {
            _gameState = gameState;
        }
    }
}
