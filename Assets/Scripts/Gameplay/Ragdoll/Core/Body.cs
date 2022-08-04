using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Ragdoll.Core
{
    // The boyd and connections of the ragdoll
    public class Body : RagCoreBase
    {
        [Header("Body Torso")]
        public Transform animatedTorso;
        public Rigidbody physicalTorso;

        [Header("Body Animator")]
        public Animator animatedAnimator;
        public Animator physicalAnimator;

        public List<BodyPart> bodyParts;


        private void Awake()
        {
            // todo: some checks and defaults
        }

        private void OnValidate()
        {
            // todo get the default body parts
        }


        public BodyPart GetBodyPart(string name)
        {
            foreach (BodyPart part in bodyParts)
                if (part.name == name)
                    return part;

            return null;
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
}
