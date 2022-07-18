#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace RagdollMecanimMixer {
    [System.Serializable]
    [CreateAssetMenu(fileName = "New RagdollAvatar", menuName = "Ragdoll Avatar", order = 51)]
    public class RagdollAvatar : ScriptableObject {
        public float mass = 70;
        public int layer;
        public PhysicMaterial physicMaterial;

        public float spring = 1000;
        public float damper = 10;
        public List<PreviewBone> previewBones;
    }
}
#endif