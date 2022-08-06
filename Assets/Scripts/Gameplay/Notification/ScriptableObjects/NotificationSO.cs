using BaseClasses;
using UnityEngine;

namespace Gameplay.Notification.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Notification", menuName = "HUD/Notification", order = 0)]
    public class NotificationSO : BaseScriptableObject
    {
        [TextArea] public string           text;
        public            float            secondsToVanish;
        public            NotificationType notificationType;

    }

    public enum NotificationType
    {
        ThreeRow,
        TwoRow,
    }
}
