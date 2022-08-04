using UnityEngine;

namespace Gameplay.Ragdoll.Utilities
{
    public class MathHelper : MonoBehaviour
    {
        /// Get the projection of a vector onto the ground (0,1,0) in unity
        public static Vector3 GetProjectionOnGround(in Vector3 target)
        {
            return Vector3.ProjectOnPlane(target, Vector3.up).normalized;
        }
    }
}
