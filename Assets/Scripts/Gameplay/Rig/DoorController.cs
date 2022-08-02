using System;
using System.Collections;
using UnityEngine;

namespace Gameplay.Rig
{

    public enum AUTODOOR_TYPE
    {
        AUTO_CLOSE,
        AUTO_OPEN
    }

    public class DoorController : MonoBehaviour
    {
        private static readonly int Open = Animator.StringToHash("open");
        private                 int _timer;

        public Animator doorAnimator;
        [Header("Automation")]
        public bool isAutoDoor;
        public          AUTODOOR_TYPE autoDoorType;
        [Min(0)] public int           automationDelay;

        DoorController() => automationDelay = 1;

        public void OpenDoor()
        {
            doorAnimator.SetBool(Open, true);
            if (isAutoDoor) _timer = 0;
        }

        public void CloseDoor()
        {
            doorAnimator.SetBool(Open, false);
            if (isAutoDoor) _timer = 0;
        }

        private void Start()
        {
            if (doorAnimator == null) gameObject.GetComponent<Animator>();
            StartCoroutine(StartTimer());
        }

        private void Update()
        {
            // door automation will check every automationDelay seconds
            if (isAutoDoor && _timer >= automationDelay)
            {
                if (autoDoorType == AUTODOOR_TYPE.AUTO_CLOSE)
                {
                    CloseDoor();
                }
                else if (autoDoorType == AUTODOOR_TYPE.AUTO_OPEN)
                {
                    OpenDoor();
                }
            }
        }

        IEnumerator StartTimer()
        {
            while (_timer >= 0)
            {
                yield return new WaitForSeconds(1f);
                _timer++;
            }

        }
    }
}
