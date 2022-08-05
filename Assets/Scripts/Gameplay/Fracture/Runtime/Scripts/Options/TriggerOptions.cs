using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gameplay.Fracture.Runtime.Scripts.Options
{
    public enum TriggerType
    {
        Collision,
        RaycastHit,
        Trigger,
        Keyboard
    }

    [Serializable]
    public class TriggerOptions
    {
        [Tooltip("The type of input that triggers the fracture.")]
        public TriggerType triggerType;

        [Tooltip("Minimum contact collision force required to cause the object to fracture.")]
        public float minimumCollisionForce;

        [Tooltip("If true, only objects with the tags 'Allowed Tags' list will trigger a collision.")]
        public bool filterCollisionsByTag;

        [Tooltip("If 'Filter Collisions By Tag' is set to true, only objects with the tags in this list will trigger the fracture.")]
        public List<string> triggerAllowedTags;

        [Tooltip("If the trigger type is Keyboard, this is the key code that will trigger a fracture when pressed.")]
        public KeyCode triggerKey;

        // raycast hit
        public float  maximumHealth;
        public float  rayForce = 15f; // todo: currently no usage
        public bool burstOnHit = true;

        public TriggerOptions()
        {
            triggerType = TriggerType.Collision;
            minimumCollisionForce = 0f;
            filterCollisionsByTag = false;
            triggerAllowedTags = new List<string>();
            triggerKey = KeyCode.None;
            maximumHealth = 5f;
        }

        /// <summary>
        /// Returns true if the specified tag is allowed to trigger the fracture
        /// </summary>
        /// <param name="tag">The tag to check</param>
        /// <returns></returns>
        public bool IsTagAllowed(string tag)
        {
            return triggerAllowedTags.Contains(tag);
        }
    }
}
