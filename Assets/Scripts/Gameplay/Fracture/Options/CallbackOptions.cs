using System;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Fracture.Options
{
    [Serializable]
    public class CallbackOptions
    {
        public UnityEvent onCompleted;

        public CallbackOptions()
        {
            this.onCompleted = null;
        }
    }
}
