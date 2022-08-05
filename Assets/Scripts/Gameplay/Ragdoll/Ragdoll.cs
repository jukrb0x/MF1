using System;
using Gameplay.Ragdoll.Core;
using Gameplay.Ragdoll.Input;
using UnityEngine;

namespace Gameplay.Ragdoll
{
    [RequireComponent(typeof(RagdollGrab))]
    [RequireComponent(typeof(RagdollCamera))]
    [RequireComponent(typeof(RagdollMovement))]
    [RequireComponent(typeof(RagdollPhysics))]
    [RequireComponent(typeof(RagdollBody))]
    [RequireComponent(typeof(RagdollAnimation))]
    [RequireComponent(typeof(RagdollInputs))]
    // The modularized Ragdoll System
    public class Ragdoll : MonoBehaviour
    {
        // Ragdoll Components
        [Header("Ragdoll System")]
        public RagdollInputs inputs;
        public RagdollAnimation ragdollAnimation;
        public RagdollBody      ragdollBody;
        public RagdollPhysics   ragdollPhysics;
        public RagdollMovement  ragdollMovement;
        public RagdollCamera    ragdollCamera;
        public RagdollGrab      ragdollGrab;

        // Ragdoll Internals

        private void OnValidate()
        {
            if (inputs == null) inputs = GetComponent<RagdollInputs>();
            if (ragdollAnimation == null) ragdollAnimation = GetComponent<RagdollAnimation>();
            if (ragdollBody == null) ragdollBody = GetComponent<RagdollBody>();
            if (ragdollPhysics == null) ragdollPhysics = GetComponent<RagdollPhysics>();
            if (ragdollMovement == null) ragdollMovement = GetComponent<RagdollMovement>();
            if (ragdollCamera == null) ragdollCamera = GetComponent<RagdollCamera>();
            if (ragdollGrab == null) ragdollGrab = GetComponent<RagdollGrab>();
        }



    }
}
