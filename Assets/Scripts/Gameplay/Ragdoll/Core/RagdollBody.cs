using System;
using System.Collections.Generic;
using Gameplay.Ragdoll.Utilities;
using UnityEngine;

namespace Gameplay.Ragdoll.Core
{
    // The boyd and connections of the ragdoll
    // the ragdoll has two bodies: animated one and physical one
    // they bodies need to be synchronized in the runtime
    public class RagdollBody : RagdollCore
    {
        [Header("Body Torso")]
        public Transform animatedTorso;
        public Rigidbody physicalTorso;

        [Header("Body Animator")]
        public Animator animatedAnimator;
        public Animator physicalAnimator;

        public List<BodyPart> bodyParts;

        [Header("Physics")]
        public float maxAngularVelocity;
        public Transform[]         animatedBones;
        public ConfigurableJoint[] physicalJoints;
        public Rigidbody[]         physicalBodies;

        // Synchronization
        public AnimatorHelper animatorHelper;


        private void Awake()
        {
            animatedBones ??= animatedTorso.GetComponentsInChildren<Transform>();
            physicalJoints ??= GetComponentsInChildren<ConfigurableJoint>();
            physicalBodies ??= GetComponentsInChildren<Rigidbody>();

            foreach (var rb in physicalBodies)
            {
                rb.maxAngularVelocity = maxAngularVelocity;
            }

            // Init each body part
            foreach (var part in bodyParts)
            {
                part.Init();
            }
            // add animator helper in runtime
            animatorHelper = animatedAnimator.gameObject.AddComponent<AnimatorHelper>();

        }

        private void OnValidate()
        {
            // for auto fetch
            // animated must be above physical in the hierarchy
            Animator[] animators = GetComponentsInChildren<Animator>();
            if (animators.Length >= 2)
            {
                if (animatedAnimator == null) animatedAnimator = animators[0];
                if (physicalAnimator == null) physicalAnimator = animators[1];

                if (animatedTorso == null)
                    animatedTorso = animatedAnimator.GetBoneTransform(HumanBodyBones.Hips);
                if (physicalTorso == null)
                    physicalTorso = physicalAnimator.GetBoneTransform(HumanBodyBones.Hips).GetComponent<Rigidbody>();
            }

            if (bodyParts.Count == 0)
                GenerateDefaultBodyParts();
        }

        private void FixedUpdate()
        {
            SyncAnimatedWithPhysical();
        }

        private void SyncAnimatedWithPhysical()
        {
            var tf = animatedAnimator.transform;
            // position
            tf.position = physicalTorso.position + (tf.position - animatedTorso.position);
            // rotation
            tf.rotation = physicalTorso.rotation;
        }

        public BodyPart GetBodyPart(string name)
        {
            foreach (BodyPart part in bodyParts)
                if (part.name == name)
                    return part;

            return null;
        }

        public Transform GetAnimatedBone(HumanBodyBones bone)
        {
            return animatedAnimator.GetBoneTransform(bone);
        }

        public Transform GetPhysicalBone(HumanBodyBones bone)
        {
            return physicalAnimator.GetBoneTransform(bone);
        }

        public void GenerateDefaultBodyParts()
        {
            bodyParts.Add(new BodyPart("Head Neck",
                TryGetJoints(HumanBodyBones.Head, HumanBodyBones.Neck)));
            bodyParts.Add(new BodyPart("Torso",
                TryGetJoints(HumanBodyBones.Spine, HumanBodyBones.Chest, HumanBodyBones.UpperChest)));
            bodyParts.Add(new BodyPart("Left Arm",
                TryGetJoints(HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand)));
            bodyParts.Add(new BodyPart("Right Arm",
                TryGetJoints(HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand)));
            bodyParts.Add(new BodyPart("Left Leg",
                TryGetJoints(HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot)));
            bodyParts.Add(new BodyPart("Right Leg",
                TryGetJoints(HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot)));
        }

        private List<ConfigurableJoint> TryGetJoints(params HumanBodyBones[] bones)
        {
            List<ConfigurableJoint> jointList = new List<ConfigurableJoint>();
            foreach (HumanBodyBones bone in bones)
            {
                Transform boneTransform = physicalAnimator.GetBoneTransform(bone);
                if (boneTransform != null && (boneTransform.TryGetComponent(out ConfigurableJoint joint)))
                    jointList.Add(joint);
            }

            return jointList;
        }

        public void SetStrengthScaleForAllBodyParts(float scale)
        {
            foreach (BodyPart bodyPart in bodyParts)
                bodyPart.SetStrengthScale(scale);
        }

    }

    [Serializable]
    public class BodyPart
    {
        public  string                  name;
        public  List<ConfigurableJoint> joints;
        public  float                   strengthScale = 1;
        private List<JointDriveConfig>  XjointDriveConfigs;
        private List<JointDriveConfig>  YZjointDriveConfigs;

        public BodyPart(string name, List<ConfigurableJoint> joints)
        {
            this.name = name;
            this.joints = joints;
        }
        public void SetStrengthScale(float scale)
        {
            for (var i = 0; i < joints.Count; i++)
            {
                // they were in order to place in
                joints[i].angularXDrive = (JointDrive)(XjointDriveConfigs[i] * scale);
                joints[i].angularYZDrive = (JointDrive)(YZjointDriveConfigs[i] * scale);
            }

            strengthScale = scale;
        }

        public void Init()
        {
            XjointDriveConfigs = new List<JointDriveConfig>();
            YZjointDriveConfigs = new List<JointDriveConfig>();

            foreach (var joint in joints)
            {
                XjointDriveConfigs.Add((JointDriveConfig)joint.angularXDrive);
                YZjointDriveConfigs.Add((JointDriveConfig)joint.angularYZDrive);
            }

            strengthScale = 1;
        }
    }

    [Serializable]
    public struct JointDriveConfig
    {
        public static readonly JointDriveConfig Zero = new JointDriveConfig
            { _positionSpring = 0, _positionDamper = 0, _maximumForce = 0 };

        // Variables are exposed in the editor, but are kept readonly from code since
        // changing them would have no effect until assigned to a JointDrive.
        [SerializeField] private float _positionSpring, _positionDamper, _maximumForce;

        public float PositionSpring => _positionSpring;

        public float PositionDamper => _positionDamper;

        public float MaximumForce => _maximumForce;


        public static explicit operator JointDrive(JointDriveConfig config)
        {
            var jointDrive = new JointDrive
            {
                positionSpring = config._positionSpring,
                positionDamper = config._positionDamper,
                maximumForce = config._maximumForce
            };

            return jointDrive;
        }

        public static explicit operator JointDriveConfig(JointDrive jointDrive)
        {
            var jointDriveConfig = new JointDriveConfig
            {
                _positionSpring = jointDrive.positionSpring,
                _positionDamper = jointDrive.positionDamper,
                _maximumForce = jointDrive.maximumForce
            };

            return jointDriveConfig;
        }

        public static JointDriveConfig operator *(JointDriveConfig config, float multiplier)
        {
            return new JointDriveConfig
            {
                _positionSpring = config.PositionSpring * multiplier,
                _positionDamper = config.PositionDamper * multiplier,
                _maximumForce = config.MaximumForce * multiplier
            };
        }
    }
    
    [Serializable]
    public struct JointMotionsConfig // todo: will need when grab things
    {
        public ConfigurableJointMotion angularXMotion, angularYMotion, angularZMotion;
        public float                   angularXLimit,  angularYLimit,  angularZLimit;

        public void ApplyTo(ref ConfigurableJoint joint)
        {
            joint.angularXMotion = angularXMotion;
            joint.angularYMotion = angularYMotion;
            joint.angularZMotion = angularZMotion;

            var softJointLimit = new SoftJointLimit();

            softJointLimit.limit = angularXLimit / 2;
            joint.highAngularXLimit = softJointLimit;

            softJointLimit.limit = -softJointLimit.limit;
            joint.lowAngularXLimit = softJointLimit;

            softJointLimit.limit = angularYLimit;
            joint.angularYLimit = softJointLimit;

            softJointLimit.limit = angularZLimit;
            joint.angularZLimit = softJointLimit;
        }
    }
}
