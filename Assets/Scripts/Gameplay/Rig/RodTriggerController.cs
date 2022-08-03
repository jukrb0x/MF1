using System;
using UnityEngine;

namespace Gameplay.Rig
{
    public class RodTriggerController : BaseTrigger
    {
        public  Collider detectorOn;
        public  Collider detectorOff;
        private bool     _isTriggerOn;


        private void OnTriggerEnter(Collider other)
        {
            var otherCollider = other.gameObject.GetComponent<Collider>();
            if (otherCollider == detectorOn)
            {
                SetTriggerState(true);
                onTriggerOn?.Invoke();

            }
            else if (otherCollider == detectorOff)
            {
                SetTriggerState(false);
                onTriggerOff?.Invoke();
            }
        }

        private void SetTriggerState(bool state)
        {
            _isTriggerOn = state;
        }

    }
}
