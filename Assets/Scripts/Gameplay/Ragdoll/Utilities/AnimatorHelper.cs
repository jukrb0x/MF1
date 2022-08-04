// this script is originated by Sergio Abreu García https://sergioabreu.me
using UnityEngine;

namespace Gameplay.Ragdoll.Utilities
{
    // control Animator and IK components

    /// <summary>
    ///     Helper class that contains a lot of necessary functionality to control the animator,
    ///     and more especifically the IK.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimatorHelper : MonoBehaviour
    {
        // ----- Look -----
        public enum LookMode
        {
            // default value is chosen in the order of enum if not explicit specified
            POINT,
            TARGET
        }

        private Animator _animator;

        // Values used for animating the transition between animation & IK
        private float _currentLeftArmIKWeight, _currentRightArmIKWeight;
        private float _currentLeftLegIKWeight, _currentRightLegIKWeight;

        // ----- IK Targets -----
        private Transform _targetsParent;

        public LookMode CurrentLookMode { get; private set; }
        public Transform LookTarget { get; private set; }
        public Vector3 LookPoint { get; private set; }
        public Transform LeftHandTarget { get; private set; }
        public Transform RightHandTarget { get; private set; }
        public Transform LeftHandHint { get; private set; }
        public Transform RightHandHint { get; private set; }
        public Transform LeftFootTarget { get; private set; }
        public Transform RightFootTarget { get; private set; }

        /// <summary> How much influence the IK will have over the animation </summary>
        public float LookIKWeight { get; set; }

        public float LeftArmIKWeight { get; set; }
        public float RightArmIKWeight { get; set; }
        public float LeftLegIKWeight { get; set; }
        public float RightLegIKWeight { get; set; }

        /// <summary> How much the chest IK will influence the animation at its maximum </summary>
        public float ChestMaxLookWeight { get; set; } = 0.5f;

        /// <summary> Smooths the transition between IK an animation to avoid snapping </summary>
        public bool SmoothIKTransitions { get; set; } = true;

        public float IKTransitionsSpeed { get; set; } = 10;

        private void Awake()
        {
            _animator = GetComponent<Animator>();

            // create IK targets 
            _targetsParent = new GameObject("IK Targets").transform;
            _targetsParent.parent = transform.parent;

            LeftHandTarget = new GameObject("LeftHandTarget").transform;
            RightHandTarget = new GameObject("RightHandTarget").transform;
            LeftHandTarget.parent = _targetsParent;
            RightHandTarget.parent = _targetsParent;

            LeftHandHint = new GameObject("LeftHandHint").transform;
            RightHandHint = new GameObject("RightHandHint").transform;
            LeftHandHint.parent = _targetsParent;
            RightHandHint.parent = _targetsParent;

            LeftFootTarget = new GameObject("LeftFootTarget").transform;
            RightFootTarget = new GameObject("RightFootTarget").transform;
            LeftFootTarget.parent = _targetsParent;
            RightFootTarget.parent = _targetsParent;
        }

        private void Update()
        {
            UpdateIKTransitions();
        }

        private void OnAnimatorIK(int layerIndex)
        {
            // Look
            _animator.SetLookAtWeight(LookIKWeight, (LeftArmIKWeight + RightArmIKWeight) / 2 * ChestMaxLookWeight, 1,
                1, 0);

            var lookPos = Vector3.zero;
            if (CurrentLookMode == LookMode.TARGET) lookPos = LookTarget.position;
            if (CurrentLookMode == LookMode.POINT) lookPos = LookPoint;

            _animator.SetLookAtPosition(lookPos); // head look-at IK

            // Left arm
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, _currentLeftArmIKWeight);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, _currentLeftArmIKWeight);
            _animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, LeftArmIKWeight);

            _animator.SetIKPosition(AvatarIKGoal.LeftHand, LeftHandTarget.position);
            _animator.SetIKRotation(AvatarIKGoal.LeftHand, LeftHandTarget.rotation);
            _animator.SetIKHintPosition(AvatarIKHint.LeftElbow, LeftHandHint.position);

            // Right arm
            _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, _currentRightArmIKWeight);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, _currentRightArmIKWeight);
            _animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, RightArmIKWeight);

            _animator.SetIKPosition(AvatarIKGoal.RightHand, RightHandTarget.position);
            _animator.SetIKRotation(AvatarIKGoal.RightHand, RightHandTarget.rotation);
            _animator.SetIKHintPosition(AvatarIKHint.RightElbow, RightHandHint.position);

            // Left leg
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, _currentLeftLegIKWeight);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, _currentLeftLegIKWeight);

            _animator.SetIKPosition(AvatarIKGoal.LeftFoot, LeftFootTarget.position);
            _animator.SetIKRotation(AvatarIKGoal.LeftFoot, LeftFootTarget.rotation);

            // Right leg
            _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, _currentRightLegIKWeight);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, _currentRightLegIKWeight);

            _animator.SetIKPosition(AvatarIKGoal.RightFoot, RightFootTarget.position);
            _animator.SetIKRotation(AvatarIKGoal.RightFoot, RightFootTarget.rotation);
        }

        private void UpdateIKTransitions()
        {
            if (SmoothIKTransitions)
            {
                _currentLeftArmIKWeight = Mathf.Lerp(_currentLeftArmIKWeight, LeftArmIKWeight,
                    Time.deltaTime * IKTransitionsSpeed);
                _currentLeftLegIKWeight = Mathf.Lerp(_currentLeftLegIKWeight, LeftLegIKWeight,
                    Time.deltaTime * IKTransitionsSpeed);
                _currentRightArmIKWeight = Mathf.Lerp(_currentRightArmIKWeight, RightArmIKWeight,
                    Time.deltaTime * IKTransitionsSpeed);
                _currentRightLegIKWeight = Mathf.Lerp(_currentRightLegIKWeight, RightLegIKWeight,
                    Time.deltaTime * IKTransitionsSpeed);
            }
            else
            {
                _currentLeftArmIKWeight = LeftArmIKWeight;
                _currentLeftLegIKWeight = LeftLegIKWeight;
                _currentRightArmIKWeight = RightArmIKWeight;
                _currentRightLegIKWeight = RightLegIKWeight;
            }
        }

        public void LookAtTarget(Transform target)
        {
            LookTarget = target;
            CurrentLookMode = LookMode.TARGET;
        }

        public void LookAtPoint(Vector3 point)
        {
            LookPoint = point;
            CurrentLookMode = LookMode.POINT;
        }
    }
}
