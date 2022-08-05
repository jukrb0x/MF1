using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Ragdoll.Input
{
    /// <summary>
    ///   Handles player inputs (Input System) for the ragdoll.
    /// </summary>
    public class RagdollInputs : InputBase
    {
        // ------ Input System Events ------
        public override void OnMove(InputValue value)
        {
            OnMoveDelegates?.Invoke(value.Get<Vector2>());
        }
        public override void OnJump(InputValue value)
        {
            OnJumpDelegates?.Invoke();
        }
        public override void OnLeftClick(InputValue value)
        {
            OnLeftClickDelegates?.Invoke(value.Get<float>());

        }
        public override void OnRightClick(InputValue value)
        {
            OnRightClickDelegates?.Invoke(value.Get<float>());
        }
        public override void OnLook(InputValue value)
        {
            OnLookDelegates?.Invoke(value.Get<Vector2>());
        }
        public override void OnScrollWheel(InputValue value)
        {
           OnScrollWheelDelegates?.Invoke(value.Get<Vector2>());
        }
        
        public override void OnSprint(InputValue value)
        {
            OnSprintDelegates?.Invoke(value.Get<float>());
        }
        // ----------------------------------
        private void OnApplicationFocus(bool hasFocus)
        {
            // todo: gm can control this, pause menu...
            Cursor.lockState = hasFocus ? CursorLockMode.Locked : CursorLockMode.None;
        }
        

        private void Start()
        {
        }

        private void Update()
        {
        }
        
    }
}
