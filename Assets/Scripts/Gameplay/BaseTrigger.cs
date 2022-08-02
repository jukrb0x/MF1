using System;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;

namespace Gameplay
{
    public class BaseTrigger : MonoBehaviour
    {

        private GameManager _gm;

        private void Start()
        {
            _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        private void OnCollisionEnter(Collision other)
        {
            if (false) // TODO
            {
                _gm.SetGameState(GAME_STATE.WIN);
            }
        }
    }
}
