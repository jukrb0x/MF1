using System;
using UnityEngine;

namespace Gameplay.Ragdoll.Core
{
    public class RagdollPhysics : RagdollCore
    {
        // ----- physics -----
        [Header("Physics")]
        public float customTorsoAngularDrag = 0.05f;

        // ----- stabilizer -----
        public enum BalanceMode
        {
            UprightTorque, // default
            ManualTorque,
            StabilizerJoint,
            FreezeRotations,
            None,
        }

        public  BalanceMode       balanceMode = BalanceMode.StabilizerJoint;
        public GameObject        stabilizer;
        private Rigidbody         _stabilizerRigidbody;
        private ConfigurableJoint _stabilizerJoint;
        [SerializeField] [Header("Stabilizer")]
        private JointDriveConfig _stabilizerJointDriveConfig;
        public JointDriveConfig StabilizerJointDriveConfig
        {
            get
            {
                return _stabilizerJointDriveConfig;
            }
            set
            {
                if (_stabilizerJoint != null)
                    _stabilizerJoint.angularXDrive = _stabilizerJoint.angularYZDrive = (JointDrive)value;
            }
        }
        private Vector2 _torqueInput;

        // upright torque
        [Header("Upright Torque")] public float uprightTorque = 10000;

        [Tooltip("Defines how much torque percent is applied given the inclination angle percent [0, 1]")]
        public AnimationCurve uprightTorqueFunction;
        public float rotationTorque = 500;
        // manual torque
        [Header("Manual Torque")] public float manualTorque      = 500;
        public                           float maxManualRotSpeed = 5;


        // ----- rotation -----
        [Header("Freeze rotation")]
        public float freezeRotationSpeed = 5;
        private Quaternion _targetRotation;
        public Vector3 TargetDirection { get; set; }

        // ----- jumping -----
        [Header("Jump")]
        public float jumpForce = 20000;
        private bool _jumping;
        public bool JumpState
        {
            get
            {
                return _jumping;
            }
            set
            {
                _jumping = value;
            }
        }
        protected override void Start()
        {
            base.Start();
            // physics system starts
            UpdateTargetRotation();
            InitStabilizer();
            StartBalance();
        }

        protected override void OnInputDelegate()
        {
            base.OnInputDelegate();
            ragdoll.inputs.OnMoveDelegates += ManualTorqueInput;
        }

        // stabilizer is used to stable the ragdoll in upright position
        private void InitStabilizer()
        {
            stabilizer = new GameObject("Stabilizer", typeof(Rigidbody), typeof(ConfigurableJoint));
            stabilizer.transform.parent = ragdoll.ragdollBody.physicalTorso.transform.parent;
            stabilizer.transform.rotation = ragdoll.ragdollBody.physicalTorso.rotation;

            _stabilizerJoint = stabilizer.GetComponent<ConfigurableJoint>();
            _stabilizerRigidbody = stabilizer.GetComponent<Rigidbody>();
            _stabilizerRigidbody.isKinematic = true;

            var joint = stabilizer.GetComponent<ConfigurableJoint>();
            joint.connectedBody = ragdoll.ragdollBody.physicalTorso;
        }

        private void FixedUpdate()
        {
            UpdateTargetRotation();
            ApplyCustomDrag();
            // wondering if this is the best place to put jump(), maybe call if stabilizer joint
            if (_jumping && ragdoll.ragdollMovement.IsOnGround) Jump();

            // generally, only stabilizer joint and manual torque are used
            switch (balanceMode)
            {
                case BalanceMode.UprightTorque:
                    var up = ragdoll.ragdollBody.physicalTorso.transform.up;
                    var balancePercent = Vector3.Angle(up,
                        Vector3.up) / 180;
                    balancePercent = uprightTorqueFunction.Evaluate(balancePercent);
                    var rot = Quaternion.FromToRotation(up,
                        Vector3.up).normalized;

                    ragdoll.ragdollBody.physicalTorso.AddTorque(new Vector3(rot.x, rot.y, rot.z) * (uprightTorque * balancePercent));

                    var directionAnglePercent = Vector3.SignedAngle(ragdoll.ragdollBody.physicalTorso.transform.forward,
                        TargetDirection, Vector3.up) / 180;
                    ragdoll.ragdollBody.physicalTorso.AddRelativeTorque(0, directionAnglePercent * rotationTorque, 0);
                    break;

                case BalanceMode.FreezeRotations:
                    var smoothedRot = Quaternion.Lerp(ragdoll.ragdollBody.physicalTorso.rotation,
                        _targetRotation, Time.fixedDeltaTime * freezeRotationSpeed);
                    ragdoll.ragdollBody.physicalTorso.MoveRotation(smoothedRot);

                    break;

                case BalanceMode.StabilizerJoint:
                    // useless for game
                    _stabilizerRigidbody.MovePosition(ragdoll.ragdollBody.physicalTorso.position);
                    _stabilizerRigidbody.MoveRotation(_targetRotation);

                    break;

                case BalanceMode.ManualTorque:
                    if (ragdoll.ragdollBody.physicalTorso.angularVelocity.magnitude < maxManualRotSpeed)
                    {
                        Vector2 force = _torqueInput * manualTorque;
                        ragdoll.ragdollBody.physicalTorso.AddRelativeTorque(force.y, 0, force.x);
                    }

                    break;
            }

        }

        // Player input actions
        public void ManualTorqueInput(Vector2 torqueInput)
        {
            _torqueInput = torqueInput;
        }
        private void Jump()
        {
            // fixme: current ragdoll in the air only roll forward based on in direction of screen
            //        the problem seems to be in the animation
            var up = new Vector3(0, 1, 0);
            var f = up * jumpForce;
            // _activeRagdoll.PhysicalTorso.AddForce(f);
            AddForce(f);
            JumpState = false;
        }

        public enum ForceCoordinate
        {
            World,
            Local
        }

        /// add force to the physical torso
        public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force,
                             ForceCoordinate coordinate = ForceCoordinate.World)
        {
            if (coordinate == ForceCoordinate.World)
                ragdoll.ragdollBody.physicalTorso.AddForce(force, mode);
            if (coordinate == ForceCoordinate.Local)
                ragdoll.ragdollBody.physicalTorso.AddRelativeForce(force, mode);
        }

        // Utilities
        private void UpdateTargetRotation()
        {
            if (TargetDirection != Vector3.zero)
                _targetRotation = Quaternion.LookRotation(TargetDirection, Vector3.up);
            else
                _targetRotation = Quaternion.identity;
        }

        private void ApplyCustomDrag()
        {
            var angVel = ragdoll.ragdollBody.physicalTorso.angularVelocity;
            angVel -= (Mathf.Pow(angVel.magnitude, 2) * customTorsoAngularDrag) * angVel.normalized;
            ragdoll.ragdollBody.physicalTorso.angularVelocity = angVel;
        }

        // balance control
        public void SetBalanceMode(BalanceMode mode)
        {
            StopBalance();
            balanceMode = mode;
            StartBalance();
        }

        private void StopBalance()
        {
            switch (balanceMode)
            {
                case BalanceMode.UprightTorque:
                    break;

                case BalanceMode.FreezeRotations:
                    ragdoll.ragdollBody.physicalTorso.constraints = 0;
                    break;

                case BalanceMode.StabilizerJoint:
                    var jointDrive = (JointDrive)JointDriveConfig.Zero;
                    _stabilizerJoint.angularXDrive = _stabilizerJoint.angularYZDrive = jointDrive;
                    break;

                case BalanceMode.ManualTorque:
                    break;

            }
        }

        private void StartBalance()
        {
            switch (balanceMode)
            {
                case BalanceMode.UprightTorque:
                    break;

                case BalanceMode.FreezeRotations:
                    ragdoll.ragdollBody.physicalTorso.constraints = RigidbodyConstraints.FreezeRotation;
                    break;

                case BalanceMode.StabilizerJoint:
                    var jointDrive = (JointDrive)_stabilizerJointDriveConfig;
                    _stabilizerJoint.angularXDrive = _stabilizerJoint.angularYZDrive = jointDrive;
                    break;

                case BalanceMode.ManualTorque:
                    break;

            }
        }

    }
}
