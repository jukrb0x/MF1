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

        public Animator animator;
        
        // Define the rotation range in which the target direction influences arm movement.
        public float minTargetDirAngle = - 30,
                     maxTargetDirAngle = 60;

    }
}
