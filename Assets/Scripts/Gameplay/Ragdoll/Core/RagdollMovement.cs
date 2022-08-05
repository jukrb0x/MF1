using System;
using Gameplay.Ragdoll.Utilities;
using UnityEngine;

namespace Gameplay.Ragdoll.Core
{
    // handle the movement inputs and apply to physics component
    public class RagdollMovement : RagdollCore
    {
        public  bool    enable = true;
        private Vector2 _movement;
        private Vector3 _aimAt;

        [Range(1, 5)] public float speed = 1;

        // Ground check
        public bool IsOnGround { get; set; }
        public  float     maxSlopeAngle      = 60;
        private float     raycastMaxDistance = 0.2f;
        private Rigidbody _footLeft, _footRight;

        protected override void Start()
        {
            base.Start();
            // init the feet
            _footLeft = ragdoll.ragdollBody.GetPhysicalBone(HumanBodyBones.RightFoot).GetComponent<Rigidbody>();
            _footRight = ragdoll.ragdollBody.GetPhysicalBone(HumanBodyBones.LeftFoot).GetComponent<Rigidbody>();
        }

        protected override void OnInputDelegate()
        {
            base.OnInputDelegate();
            // input events delegates
            ragdoll.inputs.OnMoveDelegates += MovementInput;
            ragdoll.inputs.OnJumpDelegates += JumpInput;
            ragdoll.inputs.OnGroundDelegates += UpdateRagdollOnGround;
        }

        private void Update()
        {
            _aimAt = ragdoll.ragdollCamera.cameraObject.transform.forward;
            ragdoll.ragdollAnimation.AimDirection = _aimAt;
            UpdateMovement();
            UpdateFootOnGround();
        }

        private void UpdateMovement()
        {
            if (_movement == Vector2.zero || !enable)
            {
                ragdoll.ragdollAnimation.Animator.SetBool("moving", false);
                return;
            }

            // animator controls the movement
            // forward is set to the physical and update in FixedUpdate
            ragdoll.ragdollAnimation.Animator.SetBool("moving", true);
            ragdoll.ragdollAnimation.Animator.SetFloat("speed", _movement.magnitude * speed);

            float angleOffset = Vector2.SignedAngle(_movement, Vector2.up);
            Vector3 targetForward =
                Quaternion.AngleAxis(angleOffset, Vector3.up) * MathHelper.GetProjectionOnGround(_aimAt);
            ragdoll.ragdollPhysics.TargetDirection = targetForward;
        }


        // Input values
        private void MovementInput(Vector2 movement)
        {
            _movement = movement;
        }
        private void JumpInput()
        {
            ragdoll.ragdollPhysics.JumpState = true;
        }

        // Ground check
        public bool CheckRigidbodyOnGround(Rigidbody rb)
        {
            Ray ray = new Ray(rb.position, Vector3.down);
            bool onGround = UnityEngine.Physics.Raycast(ray, out RaycastHit hit, raycastMaxDistance,
                ~(1 << rb.gameObject.layer)); // raycast only ignore self layer

            // slope checks
            onGround = onGround && Vector3.Angle(hit.normal, Vector3.up) <= maxSlopeAngle;

            return onGround;
        }

        private void UpdateFootOnGround()
        {
            bool isOnGroundLast = IsOnGround;
            IsOnGround = CheckRigidbodyOnGround(_footLeft) || CheckRigidbodyOnGround(_footRight);
            if (isOnGroundLast != IsOnGround) ragdoll.inputs.OnGroundDelegates(IsOnGround);
        }

        // put this into onGround delegates
        private void UpdateRagdollOnGround(bool onGround)
        {
            if (onGround)
            {
                ragdoll.ragdollPhysics.SetBalanceMode(RagdollPhysics.BalanceMode.StabilizerJoint);
                enable = true;
                ragdoll.ragdollBody.GetBodyPart("Head Neck")?.SetStrengthScale(1);
                ragdoll.ragdollBody.GetBodyPart("Right Leg")?.SetStrengthScale(1);
                ragdoll.ragdollBody.GetBodyPart("Left Leg")?.SetStrengthScale(1);
                ragdoll.ragdollAnimation.PlayAnimation("Idle");
            }
            else // in the air
            {
                ragdoll.ragdollPhysics.SetBalanceMode(RagdollPhysics.BalanceMode.ManualTorque);
                enable = false;
                ragdoll.ragdollBody.GetBodyPart("Head Neck")?.SetStrengthScale(0.1f);
                ragdoll.ragdollBody.GetBodyPart("Right Leg")?.SetStrengthScale(0.05f);
                ragdoll.ragdollBody.GetBodyPart("Left Leg")?.SetStrengthScale(0.05f);
                ragdoll.ragdollAnimation.PlayAnimation("InTheAir");
            }
        }
    }
}
