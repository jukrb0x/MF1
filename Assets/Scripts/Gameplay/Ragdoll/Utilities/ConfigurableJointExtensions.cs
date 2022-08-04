﻿// this script is originated by Michael Stevenson https://www.mstevenson.net/

using UnityEngine;

namespace Gameplay.Ragdoll.Utilities
{
    public static class ConfigurableJointExtensions
    {
        /// <summary>
        ///     Sets a joint's targetRotation to match a given local rotation.
        ///     The joint transform's local rotation must be cached on Start and passed into this method.
        /// </summary>
        public static void SetTargetRotationLocal(this ConfigurableJoint joint, Quaternion targetLocalRotation,
                                                  Quaternion startLocalRotation)
        {
            if (joint.configuredInWorldSpace)
                Debug.LogError(
                    "SetTargetRotationLocal should not be used with joints that are configured in world space. For world space joints, use SetTargetRotation.",
                    joint);
            SetTargetRotationInternal(joint, targetLocalRotation, startLocalRotation, Space.Self);
        }

        /// <summary>
        ///     Sets a joint's targetRotation to match a given world rotation.
        ///     The joint transform's world rotation must be cached on Start and passed into this method.
        /// </summary>
        public static void SetTargetRotation(this ConfigurableJoint joint, Quaternion targetWorldRotation,
                                             Quaternion startWorldRotation)
        {
            if (!joint.configuredInWorldSpace)
                Debug.LogError(
                    "SetTargetRotation must be used with joints that are configured in world space. For local space joints, use SetTargetRotationLocal.",
                    joint);
            SetTargetRotationInternal(joint, targetWorldRotation, startWorldRotation, Space.World);
        }

        // not pure function, joint is modified
        private static void SetTargetRotationInternal(ConfigurableJoint joint, Quaternion targetRotation,
                                                      Quaternion startRotation, Space space)
        {
            // Calculate the rotation expressed by the joint's axis and secondary axis
            var axis = joint.axis;
            var right = axis;
            var forward = Vector3.Cross(axis, joint.secondaryAxis).normalized;
            var up = Vector3.Cross(forward, right).normalized;
            var worldToJointSpace = Quaternion.LookRotation(forward, up);

            // Transform into world space
            var resultRotation = Quaternion.Inverse(worldToJointSpace);

            // Counter-rotate and apply the new local rotation.
            // Joint space is the inverse of world space, so we need to invert our value
            if (space == Space.World)
                resultRotation *= startRotation * Quaternion.Inverse(targetRotation);
            else
                resultRotation *= Quaternion.Inverse(targetRotation) * startRotation;

            // Transform back into joint space
            resultRotation *= worldToJointSpace;

            // Set target rotation to our newly calculated rotation
            joint.targetRotation = resultRotation;
        }
    }
}
