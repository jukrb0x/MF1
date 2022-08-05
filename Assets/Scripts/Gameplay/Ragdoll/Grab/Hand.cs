using System;
using Gameplay.Ragdoll.Core;
using UnityEngine;

namespace Gameplay.Ragdoll.Grab
{
    public class Hand : MonoBehaviour
    {
        private GrabbableObject    _grabbableObject;
        private Rigidbody          _lastCollided;
        private ConfigurableJoint  _joint;
        private JointMotionsConfig _jointMotionsConfig;
        private Ragdoll            _ragdoll;

        public void Init(Ragdoll ragdoll, JointMotionsConfig jointMotionsConfig)
        {
            _ragdoll = ragdoll;
            _jointMotionsConfig = jointMotionsConfig;
        }

        private void Grab(Rigidbody body)
        {
            if (!enabled)
            {
                _lastCollided = body;
                return;
            }
            // grabbing
            if (_joint != null) return;
            
            // don't grab myself
            if (body.transform.IsChildOf(_ragdoll.transform)) return;

            _joint = gameObject.AddComponent<ConfigurableJoint>();
            _joint.connectedBody = body;
            // lock the joint
            _joint.xMotion = ConfigurableJointMotion.Locked;
            _joint.yMotion = ConfigurableJointMotion.Locked;
            _joint.zMotion = ConfigurableJointMotion.Locked;

            if (body.TryGetComponent(out _grabbableObject))
            {
                _grabbableObject.jointMotionsConfig.ApplyTo(ref _joint);
            }
            else
            {
                _jointMotionsConfig.ApplyTo(ref _joint);
            }
        }

        // unity events
        private void Start()
        {
            enabled = false;
        }


        private void Drop()
        {
            if(_joint == null) return;
            Destroy(_joint);
            _joint = null;
            _grabbableObject = null;
        }

        private void OnEnable()
        {
            if (_lastCollided != null)
                Grab(_lastCollided);
        }

        private void OnDisable()
        {
            Drop();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.rigidbody != null)
                Grab(collision.rigidbody);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.rigidbody == _lastCollided)
                _lastCollided = null;

        }



    }
}
