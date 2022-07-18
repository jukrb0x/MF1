using UnityEngine;
using UnityEditor;

namespace RagdollMecanimMixer {
    [CustomEditor(typeof(RamecanMixer))]
    public class RamecanMixerEditor : Editor {
        private RamecanMixer tgt;

        private Rect[] rects;
        private bool[] selected;
        private string stateName = "";
        private float stateTransitionSmooth = 0f;
        private bool showDriveSettings = false;
        
        private void OnEnable() {
            tgt = (RamecanMixer)target;
            if (tgt.bones == null) return;
            rects = new Rect[tgt.bones.Count];
            selected = new bool[tgt.bones.Count];

            if (tgt.states.Count == 0) return;
            stateName = tgt.states[tgt.currentState].name;
            stateTransitionSmooth = tgt.states[tgt.currentState].transitionSmooth;
        }

        public override void OnInspectorGUI() {
            //if cannot find ragdoll
            if (tgt.bones == null || tgt.bones.Count == 0 || tgt.ragdollContainer == null) {
                EditorStyles.label.wordWrap = true;
                EditorGUILayout.LabelField("Cannot find an object 'Ragdoll' near '" + tgt.transform.name + "' in the hierarchy (example below).");
                EditorGUILayout.LabelField("");
                EditorGUILayout.LabelField(tgt.transform.name + " Container");
                EditorGUILayout.LabelField(" - " + tgt.transform.name);
                EditorGUILayout.LabelField(" - Ragdoll");

                if (Event.current.type == EventType.Layout) return;
                tgt.Reset();
                if (tgt.bones == null) return;
                rects = new Rect[tgt.bones.Count];
                selected = new bool[tgt.bones.Count];
                return;
            }

            Undo.RecordObject(target, "RamecanMixer");

            //BEGIN STATES SETTINGS
            //states names array for popup
            string[] states = new string[tgt.states.Count];
            if (tgt.states.Count > 0)
                for (int i = 0; i < tgt.states.Count; i++)
                    states[i] = i + ". " + (tgt.states[i].name.Length == 0 ? "[no name]" : tgt.states[i].name);

            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(tgt.states.Count == 0);
            //if state changed
            EditorGUI.BeginChangeCheck();
            tgt.currentState = EditorGUILayout.Popup(tgt.currentState, states);
            if (EditorGUI.EndChangeCheck()) {
                stateName = tgt.states[tgt.currentState].name;
                stateTransitionSmooth = tgt.states[tgt.currentState].transitionSmooth;
                tgt.ChangeStateImmediately();
            }

            if (GUILayout.Button("Delete", "miniButton", GUILayout.Width(50))) {
                tgt.states.RemoveAt(tgt.currentState);
                if(tgt.currentState > 0)
                    tgt.currentState--;
                if (tgt.states.Count > 0) {
                    stateName = tgt.states[tgt.currentState].name;
                    stateTransitionSmooth = tgt.states[tgt.currentState].transitionSmooth;
                    tgt.ChangeStateImmediately();
                }
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            stateName = EditorGUILayout.TextField("State Name", stateName);
            stateTransitionSmooth = EditorGUILayout.FloatField("Transition Smooth", stateTransitionSmooth);
            EditorGUILayout.EndVertical();

            bool stateNameExists = false;
            foreach (string name in states) {
                string sub = name.Substring(name.IndexOf('.') + 2);
                if (sub.Equals(stateName)) {
                    stateNameExists = true;
                    break;
                }
            }
            if (stateNameExists) {
                if (GUILayout.Button("Update", GUILayout.Width(50), GUILayout.Height(32))) {
                    if (stateName.Length > 0) {
                        tgt.states[tgt.currentState].UpdateState(stateName, stateTransitionSmooth, tgt.bones, false);
                        EditorUtility.SetDirty(target);
                    }
                }
            } else {
                EditorGUI.BeginDisabledGroup(stateName.Length == 0);
                if (GUILayout.Button("Add", GUILayout.Width(50), GUILayout.Height(32))) {
                    if (stateName.Length > 0) {
                        tgt.states.Add(new State(stateName, stateTransitionSmooth, tgt.bones, false));
                        tgt.currentState = tgt.states.Count - 1;
                        EditorUtility.SetDirty(target);
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(tgt.states.Count == 0);
            if (GUILayout.Button("Copy States")) {
                tgt.CopyStates();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(RamecanMixer.statesCopy == null || RamecanMixer.statesCopy.Length == 0);
            if (GUILayout.Button("Paste States")) {
                tgt.PasteStates();
                tgt.currentState = 0;
                stateName = tgt.states[tgt.currentState].name;
                stateTransitionSmooth = tgt.states[tgt.currentState].transitionSmooth;
                tgt.ChangeStateImmediately();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            //END STATES SETTINGS

            EditorGUILayout.Space();
            
            //BEGIN BONES HIERARCHY
            //select or deselect all bones
            if (GUILayout.Button("Select/Deselect Bones")) {
                bool change = false;
                for (int i = 0; i < selected.Length; i++) {
                    if (selected[i]) {
                        change = true;
                        break;
                    }
                }
                for (int i = 0; i < selected.Length; i++)
                    selected[i] = !change;
            }
            
            DrawBonesHierarchy(0, true);

            if (ClickBone()) {
                Repaint();
                return;
            }
            //END BONES HIERARCHY

            //BEGIN BONE SETTINGS
            //temp vars for multi edit check
            bool isKinematic, onlyAnimation, selfCollision, angularLimits;
            float positionAccuracy, rotationAccuracy;
            float posSpring, posDamper, rotSpring, rotDamper;
            bool[] different = new bool[10];
            bool[] changed = new bool[10];
            bool isRoot = false;

            //find first selected bone
            int first = -1;
            for (int i = 0; i < selected.Length; i++)
                if (selected[i]) first = i;
            if (first != -1) {
                //set values to temp vars from first selected bone
                Bone bone = tgt.bones[first];
                if (bone.IsRoot) isRoot = true;
                isKinematic = bone.IsKinematic;
                onlyAnimation = bone.onlyAnimation;
                selfCollision = bone.selfCollision;
                positionAccuracy = bone.positionAccuracy;
                rotationAccuracy = bone.RotationAccuracy;
                angularLimits = bone.AngularLimits;

                posSpring = bone.positionDriveSpring;
                posDamper = bone.positionDriveDamper;
                rotSpring = bone.rotationDriveSpring;
                rotDamper = bone.rotationDriveDamper;

                //check differences between bones
                for (int i = 0; i < selected.Length; i++) {
                    if (!selected[i]) continue;
                    bone = tgt.bones[i];
                    if (bone.IsRoot) isRoot = true;

                    if (isKinematic != bone.IsKinematic) different[0] = true;
                    if (onlyAnimation != bone.onlyAnimation) different[1] = true;
                    if (selfCollision != bone.selfCollision) different[2] = true;
                    if (positionAccuracy != bone.positionAccuracy) different[3] = true;
                    if (rotationAccuracy != bone.RotationAccuracy) different[4] = true;
                    if (angularLimits != bone.AngularLimits) different[5] = true;

                    if (posSpring != bone.positionDriveSpring) different[6] = true;
                    if (posDamper != bone.positionDriveDamper) different[7] = true;
                    if (rotSpring != bone.rotationDriveSpring) different[8] = true;
                    if (rotDamper != bone.rotationDriveDamper) different[9] = true;
                }

                EditorGUILayout.BeginVertical("HelpBox");
                //Show GUI for parameters
                changed[0] = ShowAndCheck("Is Kinematic", ref isKinematic, different[0]);
                EditorGUI.BeginDisabledGroup(!isKinematic);
                changed[1] = ShowAndCheck("Only animation", ref onlyAnimation, different[1]);
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(isKinematic);
                changed[2] = ShowAndCheck("Self Collision", ref selfCollision, different[2]);
                changed[3] = ShowAndCheck("Position Accuracy", ref positionAccuracy, different[3], 0, 1);
                EditorGUI.BeginDisabledGroup(isRoot);
                if (isRoot) different[4] = different[5] = true;
                changed[4] = ShowAndCheck("Rotation Accuracy", ref rotationAccuracy, different[4], 0, 1);
                changed[5] = ShowAndCheck("Angular Limits", ref angularLimits, different[5]);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndVertical();

                showDriveSettings = EditorGUILayout.Foldout(showDriveSettings, "Joint Drive");
                if (showDriveSettings) {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical();
                    GUILayout.Label("");
                    GUILayout.Label("Spring");
                    GUILayout.Label("Damper");
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical();
                    GUILayout.Label("Position");
                    changed[6] = ShowAndCheck(ref posSpring, different[6]);
                    changed[7] = ShowAndCheck(ref posDamper, different[7]);
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical();
                    EditorGUI.BeginDisabledGroup(isRoot);
                    GUILayout.Label("Rotation");
                    if (isRoot) different[8] = different[9] = true;
                    changed[8] = ShowAndCheck(ref rotSpring, different[8]);
                    changed[9] = ShowAndCheck(ref rotDamper, different[9]);
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.EndDisabledGroup();

                //edit values of selected bones when changed
                for (int i = 0; i < selected.Length; i++) {
                    if (!selected[i]) continue;
                    bone = tgt.bones[i];
                    if (changed[0]) bone.IsKinematic = isKinematic;
                    if (changed[1]) bone.onlyAnimation = onlyAnimation;
                    if (changed[2]) bone.selfCollision = selfCollision;
                    if (changed[3]) bone.positionAccuracy = positionAccuracy;
                    if (changed[4]) bone.RotationAccuracy = rotationAccuracy;
                    if (changed[5]) bone.AngularLimits = angularLimits;

                    if (changed[6]) bone.positionDriveSpring = posSpring;
                    if (changed[7]) bone.positionDriveDamper = posDamper;
                    if (changed[8]) bone.rotationDriveSpring = rotSpring;
                    if (changed[9]) bone.rotationDriveDamper = rotDamper;

                    foreach (Bone b in tgt.bones) {
                        Physics.IgnoreCollision(b.collider, bone.collider, !bone.selfCollision);
                    }
                }
            }
            //END BONE SETTINGS
        }

        private bool ClickBone() {
            //click on bone
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
                for (int i = 0; i < rects.Length; i++) {
                    if (rects[i].Contains(Event.current.mousePosition)) {
                        selected[i] = !selected[i];
                        return true;
                    }
                }
            }
            return false;
        }

        private bool ShowAndCheck(string text, ref float value, bool different, float min, float max) {
            EditorGUI.BeginChangeCheck();
            if (different) EditorGUI.showMixedValue = true;
            value = EditorGUILayout.Slider(text, value, min, max);
            if (different) EditorGUI.showMixedValue = false;
            return EditorGUI.EndChangeCheck();
        }
        private bool ShowAndCheck(ref float value, bool different) {
            EditorGUI.BeginChangeCheck();
            if (different) EditorGUI.showMixedValue = true;
            value = EditorGUILayout.FloatField(value);
            if (different) EditorGUI.showMixedValue = false;
            return EditorGUI.EndChangeCheck();
        }
        private bool ShowAndCheck(string text, ref bool value, bool different) {
            EditorGUI.BeginChangeCheck();
            if (different) EditorGUI.showMixedValue = true;
            value = EditorGUILayout.Toggle(text, value);
            if (different) EditorGUI.showMixedValue = false;
            return EditorGUI.EndChangeCheck();
        }

        private void DrawBonesHierarchy(int id, bool singleBranch) {
            Bone bone = tgt.bones[id];
            
            if (!singleBranch && bone.childIDs != null) GUILayout.BeginVertical();

            RagdollConstructorEditor.DrawBoneBox(bone.name, out rects[id], Color.green, selected[id]);

            if (bone.childIDs != null) {
                if (bone.childIDs.Count > 1) GUILayout.BeginHorizontal();

                for (int i = 0; i < bone.childIDs.Count; i++)
                    DrawBonesHierarchy(bone.childIDs[i], bone.childIDs.Count == 1);

                if (bone.childIDs.Count > 1) GUILayout.EndHorizontal();
            }

            if (!singleBranch && bone.childIDs != null) GUILayout.EndVertical();

            //bottom line to childs
            if (bone.IsRoot) return;
            RagdollConstructorEditor.DrawBottomLineToChilds(rects[id]);
        }
    }
}