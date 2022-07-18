using System.Collections.Generic;
using UnityEngine;

namespace RagdollMecanimMixer {

    [System.Serializable]
    public class State {
        public string name;
        public float transitionSmooth;
        public float transitionTimer;
        public List<BoneState> bonesStates;

        public State(string name, float transitionSmooth, List<Bone> bones, bool isDefault) {
            this.name = name;
            this.transitionSmooth = transitionTimer = transitionSmooth;

            bonesStates = new List<BoneState>();
            for (int i = 0; i < bones.Count; i++)
                bonesStates.Add(new BoneState(bones[i], isDefault));
        }
        
        public State(State state) {
            name = state.name;
            transitionSmooth = transitionTimer = state.transitionSmooth;

            bonesStates = new List<BoneState>();
            for (int i = 0; i < state.bonesStates.Count; i++)
                bonesStates.Add(state.bonesStates[i]);
        }

        public void UpdateState(string name, float transitionSmooth, List<Bone> bones, bool isDefault) {
            this.name = name;
            this.transitionSmooth = transitionTimer = transitionSmooth;

            bonesStates = new List<BoneState>();
            for (int i = 0; i < bones.Count; i++)
                bonesStates.Add(new BoneState(bones[i], isDefault));
        }

        public void BeginTransition(List<Bone> bones) {
            transitionTimer = 0;

            for (int i = 0; i < bonesStates.Count; i++) {
                bones[i].prevStatePos = bones[i].rbLerpPos;
                bones[i].prevStateRot = bones[i].rbLerpRot;
                bonesStates[i].CopyToBone(bones[i]);

                foreach (Bone bone in bones) {
                    Physics.IgnoreCollision(bone.collider, bones[i].collider, !bones[i].selfCollision);
                }
            }
        }
        
        //after animation pass (lateupdate)
        public bool UpdateTransition(List<Bone> bones) {
            if (transitionTimer < transitionSmooth) {
                float percent = transitionSmooth == 0 ? 1 : Mathf.Clamp01(transitionTimer / transitionSmooth);
                percent = Mathf.Sqrt(percent);
                
                foreach (Bone bone in bones) {
                    bone.animTransform.position = bone.prevStatePos = Vector3.Lerp(bone.prevStatePos, bone.rbLerpPos, percent);
                    bone.animTransform.rotation = bone.prevStateRot = Quaternion.Slerp(bone.prevStateRot, bone.rbLerpRot, percent);
                }

                transitionTimer += Time.deltaTime;
                return true;
            } else
                return false;
        }
        
        public void ChangeImmediately(List<Bone> bones) {
            for (int i = 0; i < bonesStates.Count; i++) {
                bonesStates[i].CopyToBone(bones[i]);
                foreach (Bone bone in bones) {
                    Physics.IgnoreCollision(bone.collider, bones[i].collider, !bones[i].selfCollision);
                }
            }
        }
    }
}