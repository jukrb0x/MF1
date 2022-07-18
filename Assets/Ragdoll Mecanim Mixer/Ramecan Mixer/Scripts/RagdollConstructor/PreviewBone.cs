#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace RagdollMecanimMixer {
    [System.Serializable]
    public class PreviewBone : CharacterBone {
        public int characterBoneId;

        public Quaternion rotation;

        public ColliderType ColliderType {
            set {
                ChangeColliderType(colliderType, value);
                colliderType = value;
            }
            get {
                return colliderType;
            }
        }
        public ColliderType colliderType;
        public Bounds bounds;
        public CapsuleBounds capsuleBounds;

        public Vector3 axis;
        public Vector3 swingAxis;
        public float lowTwistLimit;
        public float highTwistLimit;
        public float swing1Limit;
        public float swing2Limit;
        
        public Vector3 direction;
        public float massInPercent;

        public PreviewBone(Transform tr, int characterBoneId, int parentID) : base(tr, parentID) {
            this.characterBoneId = characterBoneId;
            SetDefaultValues();
        }

        public PreviewBone(PreviewBone avatarBone) {
            characterBoneId = -1;
            parentID = avatarBone.parentID;
            childIDs = avatarBone.childIDs;

            name = avatarBone.name;

            massInPercent = avatarBone.massInPercent;

            rotation = avatarBone.rotation;

            colliderType = avatarBone.colliderType;
            bounds = avatarBone.bounds;
            capsuleBounds = avatarBone.capsuleBounds;

            axis = avatarBone.axis;
            swingAxis = avatarBone.swingAxis;
            lowTwistLimit = avatarBone.lowTwistLimit;
            highTwistLimit = avatarBone.highTwistLimit;
            swing1Limit = avatarBone.swing1Limit;
            swing2Limit = avatarBone.swing2Limit;
        }

        public void CopyTo(PreviewBone bone, bool mirror) {
            bone.massInPercent = massInPercent;

            if(!mirror)
                bone.rotation = rotation;

            bone.colliderType = colliderType;
            bone.bounds = bounds;
            bone.capsuleBounds = capsuleBounds;

            bone.axis = axis;
            bone.swingAxis = swingAxis;
            bone.lowTwistLimit = lowTwistLimit;
            bone.highTwistLimit = highTwistLimit;
            bone.swing1Limit = swing1Limit;
            bone.swing2Limit = swing2Limit;
        }

        private void SetDefaultValues() {
            rotation = Quaternion.identity;

            bounds = new Bounds(Vector3.zero, Vector3.one * 0.1f);

            axis = new Vector3(1, 0, 0);
            swingAxis = new Vector3(0, 1, 0);
            lowTwistLimit = -20;
            highTwistLimit = 20;
        }
        
        public bool IsSame(PreviewBone bone) {
            return characterBoneId == bone.characterBoneId;
        }

        public void ChangeColliderType(ColliderType oldc, ColliderType newc) {
            if (oldc == ColliderType.Box) {
                capsuleBounds.SetSize(bounds.size);
            } else if (newc == ColliderType.Box) {
                bounds.size = capsuleBounds.GetSize();
            }
        }

    }

    [System.Serializable]
    public class CharacterBone {
        public string name;
        public string avatarConnection;
        public Transform tr;
        public int parentID;
        public List<int> childIDs;

        public Rect rect;
        public bool selected;

        public bool IsRoot {
            get {
                return parentID == -1;
            }
        }

        public CharacterBone(Transform tr, int parentID) {
            this.tr = tr;
            name = tr.name;
            this.parentID = parentID;
        }

        public CharacterBone() {
        }

        public void SetChilds(List<int> childIDs) {
            this.childIDs = childIDs;
        }
    }

    [System.Serializable]
    public struct CapsuleBounds {
        public float height;
        public float radius;
        public CapsuleDirection direction;

        public Vector3 GetSize() {
            Vector3 size = new Vector3();
            if (direction == CapsuleDirection.XAxis) {
                size.x = height;
                size.y = size.z = radius * 2;
            } else if (direction == CapsuleDirection.YAxis) {
                size.y = height;
                size.x = size.z = radius * 2;
            } else if (direction == CapsuleDirection.ZAxis) {
                size.z = height;
                size.x = size.y = radius * 2;
            }
            return size;
        }

        public void SetSize(Vector3 size) {
            float h = size.x;
            float r = Mathf.Max(size.y, size.z);
            int d = 0;
            if (size.y > h) {
                h = size.y;
                r = Mathf.Max(size.x, size.z);
                d = 1;
            }
            if (size.z > h) {
                h = size.z;
                r = Mathf.Max(size.x, size.y);
                d = 2;
            }
            height = h;
            radius = r / 2;
            direction = (CapsuleDirection)d;
        }
    }

    public enum ColliderType {
        Box,
        Capsule,
        Sphere
    }

    public enum CapsuleDirection {
        XAxis, YAxis, ZAxis
    }
}
#endif