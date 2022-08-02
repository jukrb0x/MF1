using System;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;

namespace Gameplay
{
    public class BaseTrigger : MonoBehaviour
    {

        private GameManager _gm;
        public GameObject gameManagerGameObject;

        private void Start()
        {
            if(gameManagerGameObject == null)
            {
                gameManagerGameObject = GameObject.Find("GameManager");
            }
            _gm = gameManagerGameObject.GetComponent<GameManager>();
            if (_gm == null)
            {
                Debug.LogError("GameManager is null");
                throw new Exception("GameManager is null");
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            // TODO: control the game flow
            if (false)
            {
                _gm.SetGameState(GAME_STATE.WIN);
            }
        }
    }
}
