using System;
using UnityEngine;

namespace Gameplay.Ragdoll
{
    public class FloorCheck : MonoBehaviour
    {
        private Ragdoll   _ragdoll;
        private Rigidbody _footLeft, _footRight;

        // todo: this should be encapsulated in ragdoll class
        public float maxSlopeAngle = 60;
        public float raycastMaxDistance = 0.2f;

        public bool IsOnFloor { get; private set; }

        private void Start()
        {
            // get left foot and right foot rigidbodies

        }

        private void Update()
        {
            bool isOnFloorLast = IsOnFloor;

            IsOnFloor = CheckRigidbodyOnFloor(_footLeft) || CheckRigidbodyOnFloor(_footRight);
            
            if(isOnFloorLast != IsOnFloor) _ragdoll.inputs.OnFloorDelegates(IsOnFloor); // fixme


        }

        public bool CheckRigidbodyOnFloor(Rigidbody rb)
        {
            Ray ray = new Ray(rb.position, Vector3.down);
            bool onFloor = Physics.Raycast(ray, out RaycastHit hit, raycastMaxDistance,
                ~(1 << rb.gameObject.layer)); // raycast only ignore self layer

            // slope checks
            onFloor = onFloor && Vector3.Angle(hit.normal, Vector3.up) <= maxSlopeAngle;

            return onFloor;
        }

    }
}
