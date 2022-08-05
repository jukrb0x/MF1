using Gameplay.Ragdoll.Grab;
using UnityEngine;

namespace Gameplay.Ragdoll.Core
{
    public class RagdollGrab : RagdollCore
    {

        [Header("Bearing weights")]
        public float leftHandBearWeight = 20f;
        public float              rightHandBearWeight = 20f;
        public JointMotionsConfig jointMotionsConfig;

        private Hand _leftHand, _rightHand;

        private void Start()
        {
            var leftHand = ragdoll.ragdollBody.GetPhysicalBone(HumanBodyBones.LeftHand).gameObject;
            var rightHand = ragdoll.ragdollBody.GetPhysicalBone(HumanBodyBones.RightHand).gameObject;

            leftHand.AddComponent<Hand>();
            rightHand.AddComponent<Hand>();

            _leftHand = leftHand.GetComponent<Hand>();
            _rightHand = rightHand.GetComponent<Hand>();
            _leftHand.Init(ragdoll, jointMotionsConfig);
            _rightHand.Init(ragdoll, jointMotionsConfig);

            // input events binding
            ragdoll.inputs.OnLeftClickDelegates += GrabWithLeftHand;
            ragdoll.inputs.OnRightClickDelegates += GrabWithRightHand;
        }

        public void GrabWithLeftHand(float weight)
        {
            _leftHand.enabled = weight < leftHandBearWeight;
        }

        public void GrabWithRightHand(float weight)
        {
            _rightHand.enabled = weight < rightHandBearWeight;
        }
    }

}
