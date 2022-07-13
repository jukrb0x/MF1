using InputSystem;
using UnityEngine;

namespace StarterAssets
{
    public class UICanvasControllerInput : MonoBehaviour
    {

        [Header("Output")]
        public PlayerInputs playerInputs;

        public void VirtualMoveInput(Vector2 virtualMoveDirection)
        {
            playerInputs.MoveInput(virtualMoveDirection);
        }

        public void VirtualLookInput(Vector2 virtualLookDirection)
        {
            playerInputs.LookInput(virtualLookDirection);
        }

        public void VirtualJumpInput(bool virtualJumpState)
        {
            playerInputs.JumpInput(virtualJumpState);
        }

        public void VirtualSprintInput(bool virtualSprintState)
        {
            playerInputs.SprintInput(virtualSprintState);
        }
        
    }

}
