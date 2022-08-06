using System;
using Gameplay.Notification;
using Gameplay.Notification.ScriptableObjects;
using UnityEngine;

namespace Gameplay
{
    public class UIManager : MonoBehaviour
    {

        public GameObject wastedUI;
        public GameObject winUI;

        public void ShowWastedUI(bool isShow)
        {
            wastedUI.SetActive(isShow);
        }

        public void ShowWinUI(bool isShow)
        {
            winUI.SetActive(isShow);
        }
        public void NotifyPaused()
        {
            GetComponent<NotificationTriggerController>()?.ManuallyNotify();
        }

        public void HideAllUI()
        {
            ShowWastedUI(false);
            ShowWinUI(false);
        }

        private void Update()
        {
            if (GameManager.Instance)
            {
                var gameState = GameManager.Instance.gameState;
                switch (gameState)
                {
                    case GAME_STATE.PLAYING:
                        HideAllUI();
                        break;
                    case GAME_STATE.PAUSED:
                        NotifyPaused();
                        break;
                    case GAME_STATE.WIN:
                        ShowWinUI(true);
                        break;
                    case GAME_STATE.LOSE:
                        ShowWastedUI(true);
                        break;
                }
            }
        }
    }
}
