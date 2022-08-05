using System;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.RayKit.Options
{
    public enum HitType
    {
        Trigger,
        RaycastHit,
    }

    [Serializable]
    public class HitOptions
    {
        public bool enabled;

        public HitType hitType;

        public GameObject hitTarget;

        public UnityEvent onHit;

        public HitOptions()
        {
            onHit = null;
        }
    }
}
