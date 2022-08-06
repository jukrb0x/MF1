using System;
using System.Collections;
using Gameplay.Notification.ScriptableObjects;
using TMPro;
using UnityEngine;

namespace Gameplay.Notification
{
    public class NotificationTriggerController : MonoBehaviour
    {
        private GameObject       _notificationUI;
        public  GameObject       notificationUI3Row;
        public  GameObject       notificationUI2Row;
        private NotificationType _rowType;
        public  NotificationSO   notification;
        private TextMeshProUGUI  _notificationText;
        private float            _staySeconds;
        private bool             _isPlayed;
        public  bool             canReplay = true;

        private void OnValidate()
        {
            if (notificationUI3Row == null)
                notificationUI3Row = GameObject.Find("notification3row").gameObject;
            if (notificationUI2Row == null)
                notificationUI2Row = GameObject.Find("notification2row").gameObject;
        }
        private void Awake()
        {
            if (notificationUI3Row == null)
                notificationUI3Row = GameObject.Find("notification3row").gameObject;
            if (notificationUI2Row == null)
                notificationUI2Row = GameObject.Find("notification2row").gameObject;
        }

        private void Start()
        {
            if (notification)
            {
                _staySeconds = notification.secondsToVanish;
                _rowType = notification.notificationType;
                if (_rowType == NotificationType.ThreeRow)
                {
                    _notificationUI = notificationUI3Row;
                }
                else if (_rowType == NotificationType.TwoRow)
                {
                    _notificationUI = notificationUI2Row;
                }
            }

            if (_notificationUI)
            {
                _notificationText = _notificationUI.GetComponentInChildren<TextMeshProUGUI>();
                _notificationUI.SetActive(false);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (notification == null || _notificationUI == null) return;

            if (other.CompareTag("Player"))
            {
                FillTheText();
                Notify();
            }
        }

        public void ManuallyNotify()
        {
            FillTheText();
            Notify();
            StartCoroutine(ManuallyCountdown());
        }
        private void OnTriggerExit(Collider other)
        {
            if (canReplay) _isPlayed = false;
        }

        private void FillTheText()
        {
            _notificationText.text = notification.text;
        }
        private void Notify()
        {
            if (_isPlayed) return;

            // notificationObj.SetActive(true);
            _notificationUI.SetActive(true);
            _isPlayed = true;
            StartCoroutine(Countdown());
        }

        IEnumerator Countdown()
        {
            var timer = _staySeconds;
            timer -= Time.deltaTime;
            yield return new WaitForSeconds(timer);
            _notificationUI.SetActive(false);
        }

        // make sure the notification can pop up again when manually notify
        IEnumerator ManuallyCountdown()
        {
            float timer = _staySeconds;
            timer -= Time.deltaTime;
            yield return new WaitForSeconds(timer);
            if (canReplay) _isPlayed = false;
        }
    }
}
