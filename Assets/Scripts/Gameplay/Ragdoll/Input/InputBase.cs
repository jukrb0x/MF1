using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Ragdoll.Input
{
    public abstract class InputBase : MonoBehaviour
    {
        private void OnApplicationFocus(bool hasFocus)
        {
            Cursor.lockState = hasFocus ? CursorLockMode.Locked : CursorLockMode.None;
        }

        // ------ Delegate Definition ------
        public delegate void OnMoveDelegate(Vector2 move);
        public delegate void OnJumpDelegate();
        public delegate void OnLeftClickDelegate(float armWeight);
        public delegate void OnRightClickDelegate(float armWeight);
        public delegate void OnGroundDelegate(bool onFloor); // use to update the on-floor state
        
        // ------ Delegates ------
        public OnMoveDelegate       OnMoveDelegates;
        public OnJumpDelegate       OnJumpDelegates;
        public OnLeftClickDelegate  OnLeftClickDelegates;
        public OnRightClickDelegate OnRightClickDelegates;
        public OnGroundDelegate      OnGroundDelegates;
        
        // ------ Input System Events ------
        public abstract void OnMove(InputValue value);
        public abstract void OnJump(InputValue value);
        public abstract void OnLeftClick(InputValue value);
        public abstract void OnRightClick(InputValue value);
        
    }
}