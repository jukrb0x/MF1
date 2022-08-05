using Gameplay.Ragdoll.Core;
using UnityEngine;

namespace Gameplay.Ragdoll.Grab
{
    // the script can be attached to a rigidbody
    // that specifies the configurable joint motion
    // for the objects that can be grabbed
    public class GrabbableObject : MonoBehaviour
    {
        public JointMotionsConfig jointMotionsConfig;

    }
}
