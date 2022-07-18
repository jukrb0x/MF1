using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace RagdollMecanimMixer {
    public partial class RagdollConstructorEditor : Editor {
        
        private readonly BoxBoundsHandle boxBoundsHandle = new BoxBoundsHandle() {
            wireframeColor = Color.green,
            handleColor = Color.green
        };
        private readonly CapsuleBoundsHandle capsuleBoundsHandle = new CapsuleBoundsHandle() {
            wireframeColor = Color.green,
            handleColor = Color.green
        };
        private readonly SphereBoundsHandle sphereBoundsHandle = new SphereBoundsHandle() {
            wireframeColor = Color.green,
            handleColor = Color.green
        };
        
        private readonly JointAngularLimitHandle jointAngularLimitHandle = new JointAngularLimitHandle() {
            radius = 0.2f
        };


        public void OnSceneGUI() {
            if (!tgt.configureBonesMode) return;
            Undo.RecordObject(target, "RagdollMaker");
            if (tgt.selectedBoneID >= tgt.previewBones.Count || tgt.previewBones[tgt.selectedBoneID].tr == null) return;
            DrawLines();
            //DrawColliders();
            DrawColliderHandles();
            DrawJointHandles();

        }
        
        private void DrawLines() {
            foreach (PreviewBone bone in tgt.previewBones) {
                Quaternion globalRot = tgt.transform.rotation * bone.rotation;
                Vector3 dir = globalRot * Vector3.forward;
                Handles.DrawWireDisc(bone.tr.position, dir, 0.01f);
                Handles.DrawWireDisc(bone.tr.position + dir * 0.02f, dir, 0.005f);
                if (bone.childIDs != null) {
                    foreach (int child in bone.childIDs) {
                        Handles.DrawLine(bone.tr.position, tgt.previewBones[child].tr.position);
                    }
                }
            }
        }
        
        private void DrawColliders() {
            if (!tgt.showColliderSettings) return;

            foreach (PreviewBone bone in tgt.previewBones) {
                if (bone.Equals(tgt.previewBones[tgt.selectedBoneID])) continue;
                Quaternion globalRot = tgt.transform.rotation * bone.rotation;
                Matrix4x4 matrix = Matrix4x4.TRS(bone.tr.position, globalRot, Vector3.one);
                using (new Handles.DrawingScope(matrix)) {
                    if (bone.colliderType == ColliderType.Box)
                        Handles.DrawWireCube(bone.bounds.center, bone.bounds.size);
                    else
                        Handles.DrawWireCube(bone.bounds.center, bone.capsuleBounds.GetSize());
                }
            }
        }

        private void DrawColliderHandles() {
            if (!tgt.showColliderSettings) return;

            PreviewBone bone = tgt.previewBones[tgt.selectedBoneID];

            Quaternion globalRot = tgt.transform.rotation * bone.rotation;
            if (tgt.rotationHandle) {
                EditorGUI.BeginChangeCheck();
                Quaternion rot = Handles.RotationHandle(globalRot, bone.tr.position);
                if (EditorGUI.EndChangeCheck())
                    bone.rotation = Quaternion.Inverse(tgt.transform.rotation) * rot;
            }

            Matrix4x4 handleMatrix = Matrix4x4.TRS(bone.tr.position, globalRot, Vector3.one);
            using (new Handles.DrawingScope(handleMatrix)) {
                switch (bone.colliderType) {
                    case ColliderType.Box:
                        boxBoundsHandle.center = bone.bounds.center;
                        boxBoundsHandle.size = bone.bounds.size;
                        EditorGUI.BeginChangeCheck();
                        boxBoundsHandle.DrawHandle();
                        if (EditorGUI.EndChangeCheck()) {
                            bone.bounds.center = boxBoundsHandle.center;
                            bone.bounds.size = boxBoundsHandle.size;
                        }
                        break;
                    case ColliderType.Capsule:
                        capsuleBoundsHandle.center = bone.bounds.center;
                        capsuleBoundsHandle.height = bone.capsuleBounds.height;
                        capsuleBoundsHandle.radius = bone.capsuleBounds.radius;
                        capsuleBoundsHandle.heightAxis = (CapsuleBoundsHandle.HeightAxis)bone.capsuleBounds.direction;
                        EditorGUI.BeginChangeCheck();
                        capsuleBoundsHandle.DrawHandle();
                        if (EditorGUI.EndChangeCheck()) {
                            bone.bounds.center = capsuleBoundsHandle.center;
                            bone.capsuleBounds.height = capsuleBoundsHandle.height;
                            bone.capsuleBounds.radius = capsuleBoundsHandle.radius;
                            bone.capsuleBounds.direction = (CapsuleDirection)capsuleBoundsHandle.heightAxis;
                        }
                        break;
                    case ColliderType.Sphere:
                        sphereBoundsHandle.center = bone.bounds.center;
                        sphereBoundsHandle.radius = bone.capsuleBounds.radius;
                        EditorGUI.BeginChangeCheck();
                        sphereBoundsHandle.DrawHandle();
                        if (EditorGUI.EndChangeCheck()) {
                            bone.bounds.center = sphereBoundsHandle.center;
                            bone.capsuleBounds.radius = sphereBoundsHandle.radius;
                        }
                        break;
                }
            }
        }

        private void DrawJointHandles() {
            if (tgt.previewBones[tgt.selectedBoneID].IsRoot || !tgt.showJointSettings) return;

            PreviewBone bone = tgt.previewBones[tgt.selectedBoneID];
            Quaternion globalRot = tgt.transform.rotation * bone.rotation;

            if (tgt.rotationHandle) {
                Quaternion rot = globalRot * Quaternion.LookRotation(Vector3.Cross(bone.axis, bone.swingAxis), bone.swingAxis);
                EditorGUI.BeginChangeCheck();
                rot = Quaternion.Inverse(globalRot) * Handles.RotationHandle(rot, bone.tr.position);
                if (EditorGUI.EndChangeCheck()) {
                    bone.axis = -Vector3.Cross(rot * Vector3.forward, bone.swingAxis).Round(2);
                    bone.swingAxis = (rot * Vector3.up).Round(2);
                }
            }

            Matrix4x4 jointMatrix = Matrix4x4.TRS(bone.tr.position, globalRot * Quaternion.LookRotation(Vector3.Cross(bone.axis, bone.swingAxis), bone.swingAxis), Vector3.one);
            using (new Handles.DrawingScope(jointMatrix)) {
                jointAngularLimitHandle.xMin = bone.lowTwistLimit;
                jointAngularLimitHandle.xMax = bone.highTwistLimit;
                jointAngularLimitHandle.yMin = -bone.swing1Limit;
                jointAngularLimitHandle.yMax = bone.swing1Limit;
                jointAngularLimitHandle.zMin = -bone.swing2Limit;
                jointAngularLimitHandle.zMax = bone.swing2Limit;
                EditorGUI.BeginChangeCheck();
                jointAngularLimitHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck()) {
                    bone.lowTwistLimit = jointAngularLimitHandle.xMin;
                    bone.highTwistLimit = jointAngularLimitHandle.xMax;
                    bone.swing1Limit = jointAngularLimitHandle.yMax == bone.swing1Limit ? -jointAngularLimitHandle.yMin : jointAngularLimitHandle.yMax;
                    bone.swing2Limit = jointAngularLimitHandle.zMax == bone.swing2Limit ? -jointAngularLimitHandle.zMin : jointAngularLimitHandle.zMax;
                }
            }
        }
    }
}