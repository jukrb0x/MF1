using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RagdollMecanimMixer {
    public partial class RagdollConstructorEditor {
        private void ConfigureBonesGUI() {
            GeneralSettings();

            if (tgt.selectedBoneID >= tgt.previewBones.Count) return;

            EditorGUILayout.BeginVertical("HelpBox");
            CopySave();
            MassSettings();
            EditorGUILayout.Space();
            ColliderSettings();
            if (!tgt.previewBones[tgt.selectedBoneID].IsRoot)
                JointSettings();
            EditorGUILayout.EndVertical();
        }
        
        private void GeneralSettings() {
            tgt.layer = EditorGUILayout.LayerField("Layer", tgt.layer);
            tgt.mass = EditorGUILayout.FloatField("Mass", tgt.mass);
            tgt.physicMaterial = (PhysicMaterial)EditorGUILayout.ObjectField("Material", tgt.physicMaterial, typeof(PhysicMaterial), false);
            tgt.spring = EditorGUILayout.FloatField("Spring", tgt.spring);
            tgt.damper = EditorGUILayout.FloatField("Damper", tgt.damper);
        }

        private void CopySave() {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy")) {
                tgt.CopyBone();
            }

            EditorGUI.BeginDisabledGroup(RagdollConstructor.boneCopy == null);
            string name = "";
            if (RagdollConstructor.boneCopy != null) name = " (" + RagdollConstructor.boneCopy.name + ")";
            if (GUILayout.Button("Paste" + name)) {
                tgt.PasteBone();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private void MassSettings() {
            PreviewBone bone = tgt.previewBones[tgt.selectedBoneID];
            float fullMassInPercent = 0;
            for (int i = 0; i < tgt.previewBones.Count; i++) {
                if (tgt.selectedBoneID == i) continue;
                fullMassInPercent += tgt.previewBones[i].massInPercent;
            }
            EditorGUI.BeginDisabledGroup(fullMassInPercent >= 1);
            bone.massInPercent = EditorGUILayout.Slider("Mass In Percent", bone.massInPercent, 0, 1 - fullMassInPercent);
            EditorGUILayout.LabelField("Mass " + (tgt.mass * bone.massInPercent).ToString("0.00") + " kg out of " + (tgt.mass * (fullMassInPercent + bone.massInPercent)).ToString("0.00"));
            EditorGUI.EndDisabledGroup();
        }
        
        private void ColliderSettings() {
            tgt.showColliderSettings = EditorGUILayout.Foldout(tgt.showColliderSettings, "Collider");
            tgt.showJointSettings = !tgt.showColliderSettings;
            if (!tgt.showColliderSettings) return;

            PreviewBone bone = tgt.previewBones[tgt.selectedBoneID];

            bone.ColliderType = (ColliderType)EditorGUILayout.EnumPopup("Collider Type", bone.ColliderType);
            bone.bounds.center = EditorGUILayout.Vector3Field("Center", bone.bounds.center);
            switch (bone.colliderType) {
                case ColliderType.Box:
                    bone.bounds.size = EditorGUILayout.Vector3Field("Size", bone.bounds.size);
                    break;
                case ColliderType.Capsule:
                    bone.capsuleBounds.height = EditorGUILayout.FloatField("Height", bone.capsuleBounds.height);
                    bone.capsuleBounds.radius = EditorGUILayout.FloatField("Radius", bone.capsuleBounds.radius);
                    bone.capsuleBounds.direction = (CapsuleDirection)EditorGUILayout.EnumPopup("Direction", bone.capsuleBounds.direction);
                    bone.bounds.size = bone.capsuleBounds.GetSize();
                    break;
                case ColliderType.Sphere:
                    bone.capsuleBounds.radius = EditorGUILayout.FloatField("Radius", bone.capsuleBounds.radius);
                    bone.capsuleBounds.height = bone.capsuleBounds.radius * 2;
                    bone.bounds.size = bone.capsuleBounds.GetSize();
                    break;
            }

            EditorGUILayout.Space();

            tgt.rotationHandle = EditorGUILayout.Toggle("Edit Bone Rotation", tgt.rotationHandle);
            EditorGUI.BeginDisabledGroup(!tgt.rotationHandle);
            EditorGUI.BeginChangeCheck();
            Vector3 rot = bone.rotation.eulerAngles;
            rot = EditorGUILayout.Vector3Field("Rotation", rot);
            if (EditorGUI.EndChangeCheck())
                bone.rotation = Quaternion.Euler(rot);
            EditorGUI.EndDisabledGroup();

        }

        private void JointSettings() {
            tgt.showJointSettings = EditorGUILayout.Foldout(tgt.showJointSettings, "Joint");
            tgt.showColliderSettings = !tgt.showJointSettings;
            if (!tgt.showJointSettings) return;

            PreviewBone bone = tgt.previewBones[tgt.selectedBoneID];
            
            bone.lowTwistLimit = Mathf.Clamp(EditorGUILayout.FloatField("Low Twist Limit", bone.lowTwistLimit), -180, 180);
            bone.highTwistLimit = Mathf.Clamp(EditorGUILayout.FloatField("High Twist Limit", bone.highTwistLimit), -180, 180);
            bone.swing1Limit = Mathf.Clamp(EditorGUILayout.FloatField("Swing 1 Limit", bone.swing1Limit), 0, 180);
            bone.swing2Limit = Mathf.Clamp(EditorGUILayout.FloatField("Swing 2 Limit", bone.swing2Limit), 0, 180);

            EditorGUILayout.Space();

            tgt.rotationHandle = EditorGUILayout.Toggle("Edit Axis Rotation", tgt.rotationHandle);
            EditorGUI.BeginDisabledGroup(!tgt.rotationHandle);
            bone.axis = EditorGUILayout.Vector3Field("Axis", bone.axis);
            bone.swingAxis = EditorGUILayout.Vector3Field("Swing Axis", bone.swingAxis);
            EditorGUI.EndDisabledGroup();
        }
    }
}