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
        public delegate void OnSprintDelegate(float sprint);
        public delegate void OnLeftClickDelegate(float armWeight);
        public delegate void OnRightClickDelegate(float armWeight);
        public delegate void OnGroundDelegate(bool onFloor); // use to update the on-floor state
        public delegate void OnLookDelegate(Vector2 look);
        public delegate void OnScrollWheelDelegate(Vector2 scroll);
        
        // ------ Delegates ------
        public OnMoveDelegate        OnMoveDelegates;
        public OnJumpDelegate        OnJumpDelegates;
        public OnSprintDelegate      OnSprintDelegates;
        public OnLeftClickDelegate   OnLeftClickDelegates;
        public OnRightClickDelegate  OnRightClickDelegates;
        public OnGroundDelegate      OnGroundDelegates;
        public OnLookDelegate        OnLookDelegates;
        public OnScrollWheelDelegate OnScrollWheelDelegates;
        
        
        // ------ Input System Events ------
        public abstract void OnMove(InputValue value);
        public abstract void OnJump(InputValue value);
        public abstract void OnSprint(InputValue value);
        public abstract void OnLeftClick(InputValue value);
        public abstract void OnRightClick(InputValue value);

        public abstract void OnLook(InputValue value);
        public abstract void OnScrollWheel(InputValue value);

    }
}