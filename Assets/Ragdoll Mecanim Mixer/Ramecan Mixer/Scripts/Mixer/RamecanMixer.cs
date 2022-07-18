using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RagdollMecanimMixer {
    public class RamecanMixer : MonoBehaviour {
        public List<Bone> bones;
        public List<State> states;
        public int currentState;

        public Transform ragdollContainer;

        public Transform RootBoneTr {
            get {
                return bones[0].physTransform;
            }
        }
        public Rigidbody RootBoneRb {
            get {
                return bones[0].rigidbody;
            }
        }

        void Start() {
            foreach (Bone bone in bones) {
                //ignore collision between bones
                foreach (Bone bone1 in bones)
                    Physics.IgnoreCollision(bone.collider, bone1.collider, !bone.selfCollision);

                //ignore collision between bones and character collider
                if (GetComponent<Collider>())
                    Physics.IgnoreCollision(bone.collider, GetComponent<Collider>());
            }

            ChangeStateImmediately();

            foreach (Bone bone in bones) {
                bone.beforeAnimationPos = bone.animPosition = bone.animTransform.position;
                bone.beforeAnimationRot = bone.animTransform.rotation;

                bone.rbPrevPos = bone.rigidbody.position;
                bone.rbPrevRot = bone.rigidbody.rotation;
            }
        }

        public void ChangeStateImmediately() {
            states[currentState].ChangeImmediately(bones);
        }

        public void BeginStateTransition(int stateID) {
            if (currentState == stateID) return;
            currentState = stateID;
            states[currentState].BeginTransition(bones);
        }
        public void BeginStateTransition(string stateName) {
            int stateID = -1;
            for (int i = 0; i < states.Count; i++) {
                if (states[i].name.Equals(stateName)) {
                    stateID = i;
                    break;
                }
            }
            if (stateID == -1) {
                Debug.LogError("There is no state with name '" + stateName + "'");
                return;
            }
            if (currentState == stateID) return;
            currentState = stateID;
            states[currentState].BeginTransition(bones);
        }

        private void FixedUpdate() {
            foreach (Bone bone in bones) {
                if (!bone.rigidbody.isKinematic) {
                    //rotation drive
                    if (!bone.IsRoot) {
                        if (!bone.withoutAnimation)
                            bone.joint.targetRotation = CalculateRotation(bone.joint.axis, bone.joint.secondaryAxis, bone.animLocalRotation, bone.physStartLocalRotation);
                        else
                            bone.joint.targetRotation = Quaternion.identity;
                    }
                    //position drive
                    if (bone.positionAccuracy > 0 && !bone.withoutAnimation) {
                        float force = Vector3.Distance(bone.animPosition, bone.rigidbody.position) * bone.positionDriveSpring * bone.positionAccuracy;
                        Vector3 direction = (bone.animPosition - bone.rigidbody.position).normalized;
                        Vector3 velocity = bone.rigidbody.velocity;
                        bone.rigidbody.AddForce(force * direction - velocity * bone.positionDriveDamper, ForceMode.Acceleration);
                    }

                    bone.kinVelocity = bone.rigidbody.velocity;
                    bone.kinAngularVelocity = bone.rigidbody.angularVelocity;
                } else {
                    //calculate velocity when isKinematic
                    bone.kinVelocity = (bone.rigidbody.position - bone.rbPrevPos) / Time.fixedDeltaTime;

                    Quaternion deltaRotation = bone.rigidbody.rotation * Quaternion.Inverse(bone.rbPrevRot);
                    float angle = 0.0f;
                    Vector3 axis = Vector3.zero;
                    deltaRotation.ToAngleAxis(out angle, out axis);
                    angle *= Mathf.Deg2Rad;
                    bone.kinAngularVelocity = axis * angle * (1.0f / Time.fixedDeltaTime);
                }

                bone.rbPrevPos = bone.rigidbody.position;
                bone.rbPrevRot = bone.rigidbody.rotation;
            }
        }

        private void Update() {
            foreach (Bone bone in bones) {
                bone.beforeAnimationPos = bone.animTransform.position;
                bone.beforeAnimationRot = bone.animTransform.rotation;
            }
        }

        private void LateUpdate() {
            Mix();
        }

        private void Mix() {
            foreach (Bone bone in bones) {
                //bone.withoutAnimation helps if a certain bone has no animation
                bone.withoutAnimation = bone.beforeAnimationPos == bone.animTransform.position && bone.beforeAnimationRot == bone.animTransform.rotation;

                if (!bone.IsRoot && !bone.withoutAnimation) {
                    //setting joint anchors
                    Matrix4x4 matrixTrans = Matrix4x4.identity;
                    Bone parent = bones[bone.parentID];
                    matrixTrans.SetTRS(parent.animTransform.position, parent.animTransform.rotation * parent.rotOffset, Vector3.one);
                    bone.joint.connectedAnchor = matrixTrans.inverse.MultiplyPoint3x4(bone.animTransform.position);
                }

                if (!bone.rigidbody.isKinematic) {
                    //saving bones position and rotation after animation pass (between Update and LateUpdate)
                    if (!bone.IsRoot) {
                        Bone parent = bones[bone.parentID];
                        Quaternion parentRot = parent.animTransform.rotation * parent.rotOffset;
                        Quaternion rot = bone.animTransform.rotation * bone.rotOffset;
                        bone.animLocalRotation = Quaternion.Inverse(parentRot) * rot;
                    }
                    bone.animPosition = bone.animTransform.position;
                } else if (bone.onlyAnimation) {
                    //setting bones pos and rot directly
                    bone.rigidbody.position = bone.animTransform.position;
                    bone.rigidbody.rotation = bone.animTransform.rotation * bone.rotOffset;
                }
            }

            //interpolation for smooth movements
            float alpha = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
            foreach (Bone bone in bones) {
                bone.rbLerpPos = Vector3.Lerp(bone.rbPrevPos, bone.rigidbody.position, alpha);
                bone.rbLerpRot = Quaternion.Slerp(bone.rbPrevRot, bone.rigidbody.rotation, alpha) * Quaternion.Inverse(bone.rotOffset);
            }


            if (!states[currentState].UpdateTransition(bones)) {
                //if there is no transition between states, set pos and rot of the bone
                foreach (Bone bone in bones) {
                    bone.animTransform.position = bone.rbLerpPos;
                    bone.animTransform.rotation = bone.rbLerpRot;
                }
            }
        }

        private Quaternion CalculateRotation(Vector3 axis, Vector3 secondaryAxis, Quaternion targetRotation, Quaternion startRotation) {
            //calculate rotation from joint axis
            var right = axis;
            var forward = Vector3.Cross(axis, secondaryAxis).normalized;
            var up = Vector3.Cross(forward, right).normalized;
            Quaternion jointRotation = Quaternion.LookRotation(forward, up);
            //Трансформация в мировую систему
            Quaternion resultRotation = Quaternion.Inverse(jointRotation);
            //Контр ротация и принятие новой локальной ротации
            resultRotation *= Quaternion.Inverse(targetRotation) * startRotation;
            //Трансформация обратно в систему джоинта
            resultRotation *= jointRotation;
            return resultRotation;
        }

#if UNITY_EDITOR
        public static State[] statesCopy;
        public static int[] bonesParentsCopy;

        public void CopyStates() {
            RamecanMixer.statesCopy = new State[states.Count];
            RamecanMixer.bonesParentsCopy = new int[bones.Count];
            for (int i = 0; i < states.Count; i++)
                RamecanMixer.statesCopy[i] = new State(states[i]);
            for (int i = 0; i < RamecanMixer.bonesParentsCopy.Length; i++)
                RamecanMixer.bonesParentsCopy[i] = bones[i].parentID;
        }

        public void PasteStates() {
            if (RamecanMixer.statesCopy == null || RamecanMixer.statesCopy.Length == 0) return;
            if (RamecanMixer.bonesParentsCopy.Length != bones.Count) {
                Debug.LogError("Cannot paste due to different bone hierarchies.");
                return;
            }

            bool rightAvatar = true;
            for (int i = 0; i < bones.Count; i++)
                if (bones[i].parentID != RamecanMixer.bonesParentsCopy[i])
                    rightAvatar = false;
            if (!rightAvatar) {
                Debug.LogError("Cannot paste due to different bone hierarchies.");
                return;
            }

            states.Clear();
            foreach (State state in RamecanMixer.statesCopy) {
                states.Add(new State(state));
            }
        }

        public void Reset() {
            if (!FindBones()) return;
            FindParents();
            Prepare();

            foreach (Bone bone in bones) {
                if (bone.positionDriveSpring == 0) bone.positionDriveSpring = 1000;
                if (bone.positionDriveDamper == 0) bone.positionDriveDamper = 10;
                if (bone.rotationDriveSpring == 0) bone.rotationDriveSpring = 1000;
                if (bone.rotationDriveDamper == 0) bone.rotationDriveDamper = 10;
                bone.RotationAccuracy = bone.RotationAccuracy;
            }

            states = new List<State>();
            states.Add(new State("default", 1, bones, true));
            ChangeStateImmediately();
        }

        public void SetDrive(float spring, float damper) {
            foreach (Bone bone in bones) {
                bone.positionDriveSpring = spring;
                bone.positionDriveDamper = damper;
                bone.rotationDriveSpring = spring;
                bone.rotationDriveDamper = damper;

                bone.RotationAccuracy = bone.RotationAccuracy;
            }
        }

        private void Prepare() {
            foreach (Bone bone in bones) {
                if (!bone.IsRoot) {
                    Bone parent = bones[bone.parentID];
                    bone.physStartLocalRotation = Quaternion.Inverse(parent.rigidbody.rotation) * bone.rigidbody.rotation;

                    Quaternion parentRot = parent.animTransform.rotation * parent.rotOffset;
                    Quaternion rot = bone.animTransform.rotation * bone.rotOffset;
                    bone.animLocalRotation = Quaternion.Inverse(parentRot) * rot;
                }
            }
        }

        private bool FindBones() {
            if (transform.parent == null) return false;
            ragdollContainer = transform.parent.Find("Ragdoll");
            if (ragdollContainer == null) return false;

            bones = new List<Bone>();

            for (int i = 0; i < ragdollContainer.childCount; i++) {
                Transform physTR = ragdollContainer.GetChild(i);

                Transform animTR = transform.FindDeep(physTR.name);
                if (animTR == physTR)
                    animTR = null;

                Bone bone = new Bone {
                    name = physTR.name,
                    animTransform = animTR,
                    physTransform = physTR,
                    rigidbody = physTR.GetComponent<Rigidbody>(),
                    joint = physTR.GetComponent<ConfigurableJoint>(),
                    collider = physTR.GetComponent<Collider>(),

                    rotOffset = Quaternion.Inverse(animTR.rotation) * physTR.rotation,
                };
                bones.Add(bone);
            }
            return true;
        }

        private void FindParents() {
            for (int i = 0; i < bones.Count; i++) {
                Bone bone = bones[i];
                if (bone.joint == null) {
                    bone.parentID = -1;
                    continue;
                }
                for (int j = 0; j < bones.Count; j++) {
                    Bone parent = bones[j];
                    if (bone == parent) continue;
                    if (bone.joint.connectedBody == parent.rigidbody) {
                        bone.parentID = j;
                        if (parent.childIDs == null)
                            parent.childIDs = new List<int>();
                        parent.childIDs.Add(i);
                        break;
                    }
                }
            }
        }
#endif
    }
}