namespace RagdollMecanimMixer {
    [System.Serializable]
    public class BoneState {
        public bool isKinematic;
        public bool onlyAnimation;
        public bool selfCollision;
        public float positionAccuracy;
        public float rotationAccuracy;
        public bool angularLimits;

        public BoneState(Bone bone, bool isDefault) {
            isKinematic = isDefault ? false : bone.IsKinematic;
            onlyAnimation = isDefault ? false : bone.onlyAnimation;
            selfCollision = isDefault ? false : bone.selfCollision;
            positionAccuracy = isDefault ? 0.3f : bone.positionAccuracy;
            rotationAccuracy = isDefault ? 1 : bone.RotationAccuracy;
            angularLimits = isDefault ? true : bone.AngularLimits;
        }

        public void CopyToBone(Bone bone) {
            bone.IsKinematic = isKinematic;
            bone.onlyAnimation = onlyAnimation;
            bone.selfCollision = selfCollision;
            bone.positionAccuracy = positionAccuracy;
            bone.RotationAccuracy = rotationAccuracy;
            bone.AngularLimits = angularLimits;
        }
    }
}