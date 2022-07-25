using UnityEngine;

namespace Gameplay.Ragdoll
{
    public class Inputs : MonoBehaviour
    {
        private void OnApplicationFocus(bool hasFocus)
        {
            Cursor.lockState = hasFocus ? CursorLockMode.Locked : CursorLockMode.None;
        }
        
        
    }
}