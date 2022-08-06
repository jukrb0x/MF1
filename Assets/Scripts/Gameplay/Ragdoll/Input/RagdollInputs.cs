using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Ragdoll.Input
{
    /// <summary>
    ///   Handles player inputs (Input System) for the ragdoll.
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class RagdollInputs : InputBase
    {
        public PlayerInput playerInput;
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
        private void Start()
        {
            if(playerInput == null) playerInput = GetComponent<PlayerInput>();
        }
        
        private void OnDisable()
        {
            if (playerInput != null)
                playerInput.enabled = false;
        }

        private void OnEnable()
        {
            if (playerInput != null)
                playerInput.enabled = true;
        }
    }
}
