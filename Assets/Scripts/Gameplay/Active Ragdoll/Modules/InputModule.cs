using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ActiveRagdoll
{
    // Author: Sergio Abreu García | https://sergioabreu.me

    /// <summary> Tells the ActiveRagdoll what it should do. Input can be external (like the
    /// one from the player or from another script) and internal (kind of like sensors, such as
    /// detecting if it's on floor). </summary>
    public class InputModule : Module
    {
        // ---------- EXTERNAL INPUT ----------

        public delegate void OnMoveDelegate(Vector2 movement);

        public OnMoveDelegate OnMoveDelegates { get; set; }

        public void OnMove(InputValue value)
        {
            OnMoveDelegates?.Invoke(value.Get<Vector2>());
        }

        public delegate void OnJumpDelegate();
        public OnJumpDelegate OnJumpDelegates { get; set; }
        public void OnJump(InputValue value)
        {
            OnJumpDelegates?.Invoke();
        }

        public delegate void OnLeftArmDelegate(float armWeight);

        public OnLeftArmDelegate OnLeftArmDelegates { get; set; }

        public void OnLeftArm(InputValue value)
        {
            OnLeftArmDelegates?.Invoke(value.Get<float>());
        }

        public delegate void OnRightArmDelegate(float armWeight);

        public OnRightArmDelegate OnRightArmDelegates { get; set; }

        public void OnRightArm(InputValue value)
        {
            OnRightArmDelegates?.Invoke(value.Get<float>());
        }

        // ---------- INTERNAL INPUT ----------

        [Header("--- FLOOR ---")] public float floorDetectionDistance = 0.3f;
        public float maxFloorSlope = 60;

        private bool _isOnFloor = true;

        public bool IsOnFloor
        {
            get { return _isOnFloor; }
        }

        Rigidbody _rightFoot, _leftFoot;


        void Start()
        {
            _rightFoot = _activeRagdoll.GetPhysicalBone(HumanBodyBones.RightFoot).GetComponent<Rigidbody>();
            _leftFoot = _activeRagdoll.GetPhysicalBone(HumanBodyBones.LeftFoot).GetComponent<Rigidbody>();
        }

        void Update()
        {
            UpdateOnFloor();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            Cursor.lockState = hasFocus ? CursorLockMode.Locked : CursorLockMode.None;
        }

        public delegate void onFloorChangedDelegate(bool onFloor);

        public onFloorChangedDelegate OnFloorChangedDelegates { get; set; }

        private void UpdateOnFloor()
        {
            bool lastIsOnFloor = _isOnFloor;

            _isOnFloor = CheckRigidbodyOnFloor(_rightFoot, out Vector3 foo)
                         || CheckRigidbodyOnFloor(_leftFoot, out foo);

            if (_isOnFloor != lastIsOnFloor)
                OnFloorChangedDelegates(_isOnFloor);
        }

        /// <summary>
        /// Checks whether the given rigidbody is on floor
        /// </summary>
        /// <param name="bodyPart">Part of the body to check</param>
        /// <returns> True if the Rigidbody is on floor </returns>
        public bool CheckRigidbodyOnFloor(Rigidbody bodyPart, out Vector3 normal)
        {
            // Raycast
            Ray ray = new Ray(bodyPart.position, Vector3.down);
            bool onFloor = Physics.Raycast(ray, out RaycastHit info, floorDetectionDistance,
                ~(1 << bodyPart.gameObject.layer)); // raycast only ignore self layer

            // Additional checks
            onFloor = onFloor && Vector3.Angle(info.normal, Vector3.up) <= maxFloorSlope;

            if (onFloor && info.collider.gameObject.TryGetComponent<Floor>(out Floor floor))
                onFloor = floor.isFloor;

            normal = info.normal;
            return onFloor;
        }
    }
} // namespace ActiveRagdoll