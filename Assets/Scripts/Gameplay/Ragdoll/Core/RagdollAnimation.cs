using System;
using Gameplay.Ragdoll.Utilities;
using UnityEngine;

namespace Gameplay.Ragdoll.Core
{
    public class RagdollAnimation : RagdollCore
    {

        // Inverse Kinematics
        public bool enableIK = true;

        // Body
        private Quaternion[]        _initialJointsRotation;
        private ConfigurableJoint[] _joints;
        private Transform[]         _animatedBones;
        private AnimatorHelper      _animatorHelper;

        [NonSerialized] public Animator Animator;

        // Define the rotation range in which the target direction influences arm movement.
        public float minTargetDirAngle = -30, maxTargetDirAngle = 60;


        [Space(10)]
        // The limits of the arms direction. How far down/up should they be able to point?
        public float minArmsAngle = -70, maxArmsAngle = 100;
        // the limits of the look direction. How far down/up should the character be able to look?
        public float minLookAngle = -50, maxLookAngle = 60;

        [Space(10)]
        // The vertical offset of the look direction in reference to the target direction.
        public float lookAngleOffset;
        // The vertical offset of the arms direction in reference to the target direction.
        public float armsAngleOffset;
        // Defines the orientation of the hands
        public float handsRotationOffset = 0;

        [Space(10)]
        // How far apart to place the arms
        public float armsHorizontalSeparation = 0.75f;

        [Tooltip("The distance from the body to the hands in relation to how high/low they are. " +
                 "Allows to create more realistic movement patterns.")]
        public AnimationCurve armsDistance;

        public Vector3 AimDirection { get; set; }
        private Vector3   _armsDir,   _lookDir, _targetDir2D;
        private Transform _animTorso, _chest;
        private float     _targetDirVerticalPercent;


        private void Start()
        {

            // input event delegates
            ragdoll.inputs.OnLeftClickDelegates += UseLeftArm;
            ragdoll.inputs.OnRightClickDelegates += UseRightArm;

            // binding
            _joints = ragdoll.ragdollBody.physicalJoints;
            _animatedBones = ragdoll.ragdollBody.animatedBones;
            _animatorHelper = ragdoll.ragdollBody.animatorHelper;
            Animator = ragdoll.ragdollBody.animatedAnimator;

            _initialJointsRotation = new Quaternion[_joints.Length];
            for (int i = 0; i < _joints.Length; i++)
            {
                _initialJointsRotation[i] = _joints[i].transform.localRotation;
            }
        }

        private void FixedUpdate()
        {
            UpdatePhysicalJoints();
        }

        private void UpdatePhysicalJoints()
        {
            for (int i = 0; i < _joints.Length; i++)
            {
                ConfigurableJointExtensions.SetTargetRotationLocal(_joints[i], _animatedBones[i + 1].localRotation, _initialJointsRotation[i]);
            }
        }


        // todo: code review
        private void UpdateIK()
        {
            if (!enableIK)
            {
                _animatorHelper.LeftArmIKWeight = 0;
                _animatorHelper.RightArmIKWeight = 0;
                _animatorHelper.LookIKWeight = 0;
                return;
            }
            _animatorHelper.LookIKWeight = 1;

            AimDirection = AimDirection;
            _animTorso = ragdoll.ragdollBody.animatedTorso;
            _chest = ragdoll.ragdollBody.GetAnimatedBone(HumanBodyBones.Spine);
            ReflectBackwards();
            _targetDir2D = MathHelper.GetProjectionOnGround(AimDirection);
            CalculateVerticalPercent();

            UpdateLookIK();
            UpdateArmsIK();
        }

        /// <summary> Reflect the direction when looking backwards, avoids neck-breaking twists </summary>
        private void ReflectBackwards()
        {
            bool lookingBackwards = Vector3.Angle(AimDirection, _animTorso.forward) > 90;
            if (lookingBackwards) AimDirection = Vector3.Reflect(AimDirection, _animTorso.forward);
        }

        /// <summary> Calculate the vertical inlinacion percentage of the target direction
        /// (how much it is looking up) </summary>
        private void CalculateVerticalPercent()
        {
            float directionAngle = Vector3.Angle(AimDirection, Vector3.up);
            directionAngle -= 90;
            _targetDirVerticalPercent = 1 - Mathf.Clamp01((directionAngle - minTargetDirAngle) / Mathf.Abs(maxTargetDirAngle - minTargetDirAngle));
        }

        // turn around the head to the look-at point
        private void UpdateLookIK()
        {
            float lookVerticalAngle = _targetDirVerticalPercent * Mathf.Abs(maxLookAngle - minLookAngle) + minLookAngle;
            lookVerticalAngle += lookAngleOffset;
            _lookDir = Quaternion.AngleAxis(-lookVerticalAngle, _animTorso.right) * _targetDir2D;

            Vector3 lookPoint = ragdoll.ragdollBody.GetAnimatedBone(HumanBodyBones.Head).position + _lookDir;
            _animatorHelper.LookAtPoint(lookPoint);
        }

        private void UpdateArmsIK()
        {
            float armsVerticalAngle = _targetDirVerticalPercent * Mathf.Abs(maxArmsAngle - minArmsAngle) + minArmsAngle;
            armsVerticalAngle += armsAngleOffset;
            _armsDir = Quaternion.AngleAxis(-armsVerticalAngle, _animTorso.right) * _targetDir2D;

            float currentArmsDistance = armsDistance.Evaluate(_targetDirVerticalPercent);

            Vector3 armsMiddleTarget = _chest.position + _armsDir * currentArmsDistance;
            Vector3 upRef = Vector3.Cross(_armsDir, _animTorso.right).normalized;
            Vector3 armsHorizontalVec = Vector3.Cross(_armsDir, upRef).normalized;
            Quaternion handsRot = _armsDir != Vector3.zero ? Quaternion.LookRotation(_armsDir, upRef)
                : Quaternion.identity;

            _animatorHelper.LeftHandTarget.position = armsMiddleTarget + armsHorizontalVec * armsHorizontalSeparation / 2;
            _animatorHelper.RightHandTarget.position = armsMiddleTarget - armsHorizontalVec * armsHorizontalSeparation / 2;
            _animatorHelper.LeftHandTarget.rotation = handsRot * Quaternion.Euler(0, 0, 90 - handsRotationOffset);
            _animatorHelper.RightHandTarget.rotation = handsRot * Quaternion.Euler(0, 0, -90 + handsRotationOffset);

            var armsUpVec = Vector3.Cross(_armsDir, _animTorso.right).normalized;
            _animatorHelper.LeftHandHint.position = armsMiddleTarget + armsHorizontalVec * armsHorizontalSeparation - armsUpVec;
            _animatorHelper.RightHandHint.position = armsMiddleTarget - armsHorizontalVec * armsHorizontalSeparation - armsUpVec;
        }

        /// Animator Player, the speed only affects the speed of the animation, not the actual movement speed.
        public void PlayAnimation(string animation, float speed = 1)
        {
            Animator.Play(animation);
            Animator.SetFloat("speed", speed);
        }

        // todo: use xxx arm only works when IK is enable
        public void UseLeftArm(float weight)
        {
            if (!enableIK)
                return;

            _animatorHelper.LeftArmIKWeight = weight;
        }

        public void UseRightArm(float weight)
        {
            if (!enableIK)
                return;

            _animatorHelper.RightArmIKWeight = weight;
        }

    }
}
