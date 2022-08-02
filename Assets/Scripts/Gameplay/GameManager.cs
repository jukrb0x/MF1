using System;
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
