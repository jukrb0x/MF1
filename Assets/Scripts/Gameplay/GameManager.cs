using System;
using Gameplay.Fracture.Runtime.Scripts.Projectile;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
        public static GameManager Instance;
        public        GAME_STATE  gameState;
        private       PlayerInput _playerInput;
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

        private void Start()
        {
            _playerInput = GetComponent<PlayerInput>();
            SetCursorLockState(true);
            ResetGameplay();
        }

        private static void SetCursorLockState(bool isLocked)
        {
            Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isLocked;
        }

        private void ResetGameplay()
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

            // start playing
            SetGameState(GAME_STATE.PLAYING);
            Time.timeScale = 1;
            SwitchPlayerInputActionMap(false);
        }

        private void SetGameState(GAME_STATE gameState)
        {
            this.gameState = gameState;
        }
        private void ShowUI()
        {
            switch (gameState)
            {
                case GAME_STATE.WIN:
                    break;
                case GAME_STATE.LOSE:
                    break;
            }
        }

        // win or lose
        public void GameWin()
        {
            SetGameState(GAME_STATE.WIN);
            Debug.Log("You Win!");
            SwitchPlayerInputActionMap(true);
            Time.timeScale = 0;
            SetCursorLockState(false);
            ShowUI();
        }


        public void GameOver()
        {
            SetGameState(GAME_STATE.LOSE);
            Time.timeScale = 0;
            SwitchPlayerInputActionMap(true);
            Debug.Log("You are wasted.");
            SetCursorLockState(false);
            ShowUI();
        }

        // R to restart the game
        public void RestartGame()
        {
            // reload the scene
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
            ResetGameplay();
        }

        // pause and resume
        public void ResumeGame()
        {
            SetGameState(GAME_STATE.PLAYING);
            SwitchPlayerInputActionMap(false);
            Time.timeScale = 1;
            SetCursorLockState(true);
        }

        public void PauseGame()
        {
            SetGameState(GAME_STATE.PAUSED);
            SwitchPlayerInputActionMap(true);
            Time.timeScale = 0;
            SetCursorLockState(false);
        }

        public void SwitchPlayerInputActionMap(bool showUI)
        {
            if (showUI)
            {
                _playerInput.actions.FindActionMap("UI").Enable();
                _playerInput.actions.FindActionMap("Player").Disable();
            }
            else
            {
                _playerInput.actions.FindActionMap("UI").Disable();
                _playerInput.actions.FindActionMap("Player").Enable();
            }
        }
        // ----- input system events -----
        public void OnRestart(InputValue value)
        {
            if (value.isPressed)
                RestartGame();
        }
        public void OnPauseMenu(InputValue value)
        {
            if (value.isPressed)
            {
                switch (gameState)
                {
                    case GAME_STATE.PLAYING:
                        PauseGame();
                        break;
                    case GAME_STATE.PAUSED:
                        ResumeGame();
                        break;
                }
            }
        }
        // --------------------------------
    }
}
