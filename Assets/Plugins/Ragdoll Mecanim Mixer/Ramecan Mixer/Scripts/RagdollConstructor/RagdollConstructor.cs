#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace RagdollMecanimMixer {
    public class RagdollConstructor : MonoBehaviour {
        
        public RagdollAvatar avatar;
        //select active bones mode
        public List<CharacterBone> characterBones;
        public List<PreviewBone> tempPreviewBones;
        //configure bones mode
        public List<PreviewBone> previewBones;

        public static PreviewBone boneCopy;

        public int selectedBoneID;

        public bool configureBonesMode;

        public float mass = 70;
        public int layer;
        public PhysicMaterial physicMaterial;
        public float spring = 1000;
        public float damper = 10;

        public bool showColliderSettings;
        public bool showJointSettings;

        public bool rotationHandle;

        public bool restorePreviousConfiguration;
        
        public void Reset() {
            characterBones = new List<CharacterBone>();
            FindCharacterBone(transform, -1);
        }
        
        private int FindCharacterBone(Transform tr, int parentID) {
            int id = characterBones.Count;

            characterBones.Add(new CharacterBone(tr, parentID));

            List<int> childs = null;
            if (tr.childCount > 0) {
                childs = new List<int>();
                for (int i = 0; i < tr.childCount; i++)
                    childs.Add(FindCharacterBone(tr.GetChild(i), id));

                if(childs.Count > 0)
                    characterBones[id].SetChilds(childs);
            }

            return id;
        }

        //root bone must be only one
        public bool CheckRoot() {
            int roots = 0;
            foreach (PreviewBone bone in tempPreviewBones) {
                if (bone.IsRoot)
                    roots++;
                if (roots > 1)
                    return false;
            }
            return true;
        }

        //fill temp bones list from selected character bones
        public void FillTempBones() {
            tempPreviewBones = new List<PreviewBone>();
            FillTempBone(0, -1);
        }
        public void FillTempBone(int generalBoneID, int lastActiveID) {
            int id = tempPreviewBones.Count;
            CharacterBone characterBone = characterBones[generalBoneID];

            if (characterBone.selected) {
                PreviewBone parentBone = null;
                if (lastActiveID != -1) {
                    parentBone = tempPreviewBones[lastActiveID];
                    if (parentBone.childIDs == null) parentBone.childIDs = new List<int>();
                    parentBone.childIDs.Add(id);
                }
                tempPreviewBones.Add(new PreviewBone(characterBone.tr, generalBoneID, lastActiveID));
                lastActiveID = id;
            }

            if (characterBone.childIDs != null) {
                for (int i = 0; i < characterBone.childIDs.Count; i++) {
                    int childId = characterBone.childIDs[i];
                    FillTempBone(childId, lastActiveID);
                }
            }
        }

        public void CompareAndChangePreviewBones() {
            layer = avatar.layer;
            mass = avatar.mass;
            physicMaterial = avatar.physicMaterial;
            spring = avatar.spring;
            damper = avatar.damper;

            float massInPercentAvg = 1f / tempPreviewBones.Count;
            for (int i = 0; i < tempPreviewBones.Count; i++) {
                tempPreviewBones[i].massInPercent = massInPercentAvg;

                //set config from avatar bones
                if (avatar.previewBones != null) {
                    for (int j = 0; j < avatar.previewBones.Count; j++) {
                        int generalBoneId = tempPreviewBones[i].characterBoneId;
                        if (avatar.previewBones[j].name.Equals(characterBones[generalBoneId].avatarConnection)) {
                            avatar.previewBones[j].CopyTo(tempPreviewBones[i], false);
                            break;
                        }
                    }
                }

                //restore previous config
                if (restorePreviousConfiguration && previewBones != null) {
                    for (int j = 0; j < previewBones.Count; j++) {
                        if (tempPreviewBones[i].IsSame(previewBones[j])) {
                            previewBones[j].CopyTo(tempPreviewBones[i], false);
                            break;
                        }
                    }
                }

                CalculateRotationAndBounds(tempPreviewBones[i]);
            }
            
            previewBones = tempPreviewBones;
            tempPreviewBones = null;
        }
        
        public void CalculateRotationAndBounds(PreviewBone bone) {
            if (bone.childIDs == null || bone.childIDs.Count == 0) {
                //end bones without childs (for example: head, hand, foot)
                bone.direction = bone.IsRoot ? bone.tr.up : tempPreviewBones[bone.parentID].direction;
                bone.direction = bone.direction.AlignDirectionTo(bone.tr);
                
                if(bone.rotation == Quaternion.identity)
                    bone.rotation = Quaternion.Inverse(transform.rotation) * Quaternion.LookRotation(bone.direction, transform.forward);

                if (bone.bounds.center == Vector3.zero)
                    bone.bounds.center = Vector3.forward * bone.bounds.size.z / 2;
            } else if (bone.childIDs.Count == 1) {
                //intermediate bones with one child(upper/lower arm/leg, stomach)
                CalculateRotationAndBoundsSingleChild(bone, 0);
            } else if (bone.childIDs.Count > 1) {
                //bones with many childs (hip, chest)

                //main bone search
                float dot = -1;
                int id = 0;
                for (int i = 0; i < bone.childIDs.Count; i++) {
                    Vector3 childPos1 = tempPreviewBones[bone.childIDs[i]].tr.position;
                    Vector3 localAbsDir = bone.tr.InverseTransformDirection(childPos1 - bone.tr.position).Abs();
                    float tempDot = Mathf.Max(Vector3.Dot(Vector3.up, localAbsDir), Vector3.Dot(Vector3.right, localAbsDir), Vector3.Dot(Vector3.forward, localAbsDir));
                    if (tempDot > dot) {
                        dot = tempDot;
                        id = i;
                    }
                }

                CalculateRotationAndBoundsSingleChild(bone, id);
            }
        }

        public void CalculateRotationAndBoundsSingleChild(PreviewBone bone, int id) {
            Vector3 childPos = tempPreviewBones[bone.childIDs[id]].tr.position;
            bone.direction = childPos - bone.tr.position;
            
            bone.rotation = Quaternion.Inverse(transform.rotation) * Quaternion.LookRotation(bone.direction, transform.forward);

            bone.bounds.center = new Vector3(bone.bounds.center.x, bone.bounds.center.y, bone.direction.magnitude / 2);
            bone.bounds.size = new Vector3(bone.bounds.size.x, bone.bounds.size.y, bone.direction.magnitude);
            bone.capsuleBounds.SetSize(bone.bounds.size);
        }
        
        public void FindConnectionsWithAvatar() {
            if (avatar == null) return;
            if (avatar.previewBones == null) return;
            //connect avatar and preview bones if they has equal name
            for (int i = 0; i < characterBones.Count; i++) {
                CharacterBone boneG = characterBones[i];
                foreach (PreviewBone boneA in avatar.previewBones) {
                    if (boneA.name.Equals(boneG.name) || boneA.name.Equals(characterBones[i].avatarConnection)) {
                        boneG.selected = true;
                        characterBones[i].avatarConnection = boneA.name;
                        break;
                    }
                }
            }
        }

        public void SaveBonesToAvatar() {
            avatar.layer = layer;
            avatar.mass = mass;
            avatar.physicMaterial = physicMaterial;
            avatar.spring = spring;
            avatar.damper = damper;

            avatar.previewBones = new List<PreviewBone>();
            if (previewBones.Count > 0) {
                foreach (PreviewBone bone in previewBones)
                    avatar.previewBones.Add(new PreviewBone(bone));
            }
        }

        //creating objects and adding components
        public void CreateRagdoll() {
            if(transform.parent != null && transform.parent.name.Equals(transform.name + " Container")) {
                Transform parent = transform.parent;
                transform.parent = null;
                DestroyImmediate(parent.gameObject);
            }
            Transform container = new GameObject().transform;
            Transform ragdoll = new GameObject().transform;
            container.position = ragdoll.position = transform.position;
            container.rotation = ragdoll.rotation = transform.rotation;
            container.name = transform.name + " Container";
            ragdoll.name = "Ragdoll";
            transform.parent = ragdoll.parent = container;

            List<Rigidbody> rigidbodies = new List<Rigidbody>();
            foreach (PreviewBone bone in previewBones) {
                GameObject boneGo = new GameObject();
                Transform boneTr = boneGo.transform;

                boneTr.parent = ragdoll;
                boneTr.name = bone.name;
                boneTr.position = bone.tr.position;
                boneTr.rotation = transform.rotation * bone.rotation;

                boneGo.layer = layer;

                if (bone.colliderType == ColliderType.Box) {
                    BoxCollider collider = boneGo.AddComponent<BoxCollider>();
                    collider.center = bone.bounds.center;
                    collider.size = bone.bounds.size;
                    collider.material = physicMaterial;
                } else if (bone.colliderType == ColliderType.Capsule) {
                    CapsuleCollider collider = boneGo.AddComponent<CapsuleCollider>();
                    collider.center = bone.bounds.center;
                    collider.radius = bone.capsuleBounds.radius;
                    collider.height = bone.capsuleBounds.height;
                    collider.direction = (int) bone.capsuleBounds.direction;
                    collider.material = physicMaterial;
                } else if (bone.colliderType == ColliderType.Sphere) {
                    SphereCollider collider = boneGo.AddComponent<SphereCollider>();
                    collider.center = bone.bounds.center;
                    collider.radius = bone.capsuleBounds.radius;
                    collider.material = physicMaterial;
                }

                Rigidbody rigidbody = boneGo.AddComponent<Rigidbody>();
                rigidbody.mass = Mathf.Round(mass * bone.massInPercent * 100) / 100;
                rigidbodies.Add(rigidbody);

                if (bone.IsRoot) continue;
                ConfigurableJoint joint = boneGo.AddComponent<ConfigurableJoint>();
                joint.connectedBody = rigidbodies[bone.parentID];
                joint.autoConfigureConnectedAnchor = false;
                joint.rotationDriveMode = RotationDriveMode.Slerp;
                joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Locked;
                joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Limited;

                joint.projectionMode = JointProjectionMode.PositionAndRotation;
                joint.enablePreprocessing = true;

                joint.axis = bone.axis;
                joint.secondaryAxis = bone.swingAxis;
                joint.lowAngularXLimit = new SoftJointLimit {
                    limit = bone.lowTwistLimit
                };
                joint.highAngularXLimit = new SoftJointLimit {
                    limit = bone.highTwistLimit
                };
                joint.angularYLimit = new SoftJointLimit {
                    limit = bone.swing1Limit
                };
                joint.angularZLimit = new SoftJointLimit {
                    limit = bone.swing2Limit
                };
            }

            RamecanMixer mixer = gameObject.GetComponent<RamecanMixer>();
            if (mixer != null)
                mixer.Reset();
            else
                mixer = gameObject.AddComponent<RamecanMixer>();
            mixer.SetDrive(spring, damper);
            
            Animator animator = gameObject.GetComponent<Animator>();
            if (animator != null)
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }

        public void CopyBone() {
            RagdollConstructor.boneCopy = new PreviewBone(previewBones[selectedBoneID]);
        }
        public void PasteBone() {
            RagdollConstructor.boneCopy.CopyTo(previewBones[selectedBoneID], true);
        }
    }
}
#endif