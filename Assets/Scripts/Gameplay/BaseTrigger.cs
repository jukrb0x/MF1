using System;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay
{
    public class BaseTrigger : MonoBehaviour
    {

        // private GameObject  _gameManagerGameObject;
        // public  GameManager gameManager;
        public  UnityEvent  onTriggerOn;
        public  UnityEvent  onTriggerOff;

        private void Start()
        {
            // GameManager is now called by static instance
            
            // if (gameManager == null)
            // {
            //     _gameManagerGameObject = GameObject.Find("GameManager");
            //     if (_gameManagerGameObject != null)
            //     {
            //         gameManager = _gameManagerGameObject.GetComponent<GameManager>();
            //         if (gameManager == null)
            //         {
            //             Debug.LogError("GameManager Component not found");
            //         }
            //     }
            //     else
            //     {
            //         Debug.LogError("GameManager Game Object not found");
            //     }
            // }
        }

    }
}
