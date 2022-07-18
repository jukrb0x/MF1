using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RagdollMecanimMixer {
    [CustomEditor(typeof(RagdollConstructor))]
    public partial class RagdollConstructorEditor : Editor {
        private RagdollConstructor tgt;

        private bool lineDrag;
        private PreviewBone lineStartBone;
        private Vector2 scrollPosition;
        private Rect scrollRect;

        private void OnEnable() {
            tgt = (RagdollConstructor)target;
        }

        public override void OnInspectorGUI() {
            Undo.RecordObject(target, "RagdollConstructor");

            if (!tgt.configureBonesMode) {
                //BEGIN SELECT ACTIVE BONES MODE
                //BEGIN CHARACTER BONES HIERARCHY
                EditorGUILayout.BeginVertical(GUILayout.MinHeight(240));
                EditorGUILayout.LabelField("Character bones hierarchy.");
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                DrawBonesHierarchy(tgt.characterBones[0], true, false, true);
                EditorGUILayout.EndScrollView();
                scrollRect = GUILayoutUtility.GetLastRect();
                foreach (CharacterBone bone in tgt.characterBones)
                    bone.rect.position += scrollRect.position - scrollPosition;
                EditorGUILayout.EndVertical();

                if (ClickCharacterBone()) {
                    tgt.FillTempBones();
                    Repaint();
                    return;
                }
                //END CHARACTER BONES HIERARCHY
                EditorGUILayout.Space();

                EditorGUILayout.BeginVertical("HelpBox");
                //BEGIN AVATAR BONES HIERARCHY
                if (tgt.avatar != null && tgt.avatar.previewBones != null && tgt.avatar.previewBones.Count > 0) {
                    DrawAvatarBonesHierarchy(0, true);
                    EditorGUILayout.LabelField("Connect corresponding bones.");
                    if (CheckAndBeginLineDrag()) {
                        EditorGUILayout.EndVertical();
                        Repaint();
                        return;
                    }
                }
                //END AVATAR BONES HIERARCHY

                //BEGIN AVATAR
                EditorGUI.BeginChangeCheck();
                tgt.avatar = (RagdollAvatar)EditorGUILayout.ObjectField("Avatar", tgt.avatar, typeof(RagdollAvatar), false);
                if (EditorGUI.EndChangeCheck()) {
                    tgt.FindConnectionsWithAvatar();
                    tgt.FillTempBones();
                }
                //END AVATAR
                EditorGUILayout.EndVertical();

                if (tgt.avatar == null) return;
                if (UpdateLineDrag()) {
                    tgt.FillTempBones();
                    Repaint();
                    return;
                }

                if (tgt.tempPreviewBones == null || tgt.tempPreviewBones.Count == 0) return;
                EditorGUI.BeginDisabledGroup(tgt.previewBones == null || tgt.previewBones.Count == 0);
                tgt.restorePreviousConfiguration = EditorGUILayout.ToggleLeft("Restore Previous Configuration", tgt.restorePreviousConfiguration);
                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button("Configure Selected Bones")) {
                    tgt.FillTempBones();
                    if (tgt.CheckRoot()) {
                        tgt.CompareAndChangePreviewBones();
                        if (tgt.selectedBoneID < tgt.previewBones.Count)
                            tgt.previewBones[tgt.selectedBoneID].selected = true;

                        tgt.configureBonesMode = true;
                    } else {
                        Debug.LogError("The root bone must be single.");
                    }
                }
                //END SELECT ACTIVE BONES MODE
            } else {
                if (GUILayout.Button("Change Active Bones")) {
                    tgt.FindConnectionsWithAvatar();
                    tgt.FillTempBones();
                    tgt.configureBonesMode = false;
                }

                DrawBonesHierarchy(tgt.previewBones[0], true, true, false);
                if (ClickPreviewBone()) {
                    Repaint();
                    //move camera to bone
                    PreviewBone bone = tgt.previewBones[tgt.selectedBoneID];
                    Bounds bounds = bone.bounds;
                    bounds.center = bone.tr.position + bone.rotation * bounds.center;
                    if (bone.colliderType != ColliderType.Box)
                        bounds.size = bone.capsuleBounds.GetSize();
                    if (SceneView.lastActiveSceneView != null)
                        SceneView.lastActiveSceneView.Frame(bounds, false);
                    return;
                }
                ConfigureBonesGUI();

                if (tgt.avatar != null && GUILayout.Button("Save Avatar '" + tgt.avatar.name + "'")) {
                    tgt.SaveBonesToAvatar();
                    EditorUtility.SetDirty(tgt.avatar);
                }
                if (GUILayout.Button("Create Ragdoll")) {
                    tgt.CreateRagdoll();
                }
            }

            if (GUI.changed) {
                EditorUtility.SetDirty(tgt);
                EditorSceneManager.MarkSceneDirty(tgt.gameObject.scene);
            }
        }

        private bool CheckAndBeginLineDrag() {
            //click on bone
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
                foreach (PreviewBone bone in tgt.avatar.previewBones) {
                    if (bone.rect.Contains(Event.current.mousePosition)) {
                        lineStartBone = bone;
                        lineDrag = true;
                        return true;
                    }
                }
            }
            return false;
        }

        private bool UpdateLineDrag() {
            if (lineDrag) {
                CharacterBone lineEndBone = null;
                for (int i = 0; i < tgt.characterBones.Count; i++) {
                    if (scrollRect.Contains(Event.current.mousePosition) && tgt.characterBones[i].rect.Contains(Event.current.mousePosition)) {
                        lineEndBone = tgt.characterBones[i];
                        break;
                    }
                }

                if (lineEndBone != null)
                    DrawConnector(lineStartBone.rect, lineEndBone.rect, Color.black, true);
                else
                    DrawConnector(lineStartBone.rect, new Rect(Event.current.mousePosition - new Vector2(10, 0), Vector2.zero), Color.black, false);

                if (Event.current.type == EventType.MouseUp && Event.current.button == 0) {
                    lineDrag = false;

                    int generalBoneID = -1;
                    for (int i = 0; i < tgt.characterBones.Count; i++) {
                        if (lineStartBone.name.Equals(tgt.characterBones[i].avatarConnection)) {
                            generalBoneID = i;
                            break;
                        }
                    }

                    if (lineEndBone != null) {
                        if (lineStartBone.name.Equals(lineEndBone.avatarConnection)) return false;
                        if (generalBoneID >= 0 && !lineEndBone.Equals(tgt.characterBones[generalBoneID])) {
                            tgt.characterBones[generalBoneID].avatarConnection = null;
                            tgt.characterBones[generalBoneID].selected = false;
                        }
                        lineEndBone.avatarConnection = lineStartBone.name;
                        lineEndBone.selected = true;
                        return true;
                    } else if (generalBoneID >= 0) {
                        tgt.characterBones[generalBoneID].avatarConnection = null;
                        tgt.characterBones[generalBoneID].selected = false;
                        return true;
                    }
                }
                Repaint();
            }
            return false;
        }

        private bool ClickCharacterBone() {
            //click on bone
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
                foreach (CharacterBone bone in tgt.characterBones) {
                    if (scrollRect.Contains(Event.current.mousePosition) && bone.rect.Contains(Event.current.mousePosition)) {
                        bone.selected = !bone.selected;
                        return true;
                    }
                }
            }
            return false;
        }

        private bool ClickPreviewBone() {
            //click on bone
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
                for (int i = 0; i < tgt.previewBones.Count; i++) {
                    //foreach (BonePreview bone in tgt.previewBones) {
                    if (tgt.previewBones[i].rect.Contains(Event.current.mousePosition)) {
                        if (tgt.selectedBoneID <= tgt.previewBones.Count)
                            tgt.previewBones[tgt.selectedBoneID].selected = false;
                        tgt.previewBones[i].selected = true;
                        tgt.selectedBoneID = i;
                        return true;
                    }
                }
            }
            return false;
        }

        private void DrawConnector(Rect rectP, Rect rectC, Color color, bool limitPos) {
            Vector2 posP = new Vector2(rectP.x, rectP.y + rectP.height / 2);
            Vector2 posC = new Vector2(rectC.x, rectC.y + rectC.height / 2);

            Handles.color = color;
            float top = scrollRect.position.y;
            float bottom = scrollRect.position.y + scrollRect.height;

            if (posC.y > bottom && limitPos) {
                posC.y = bottom;
            } else if (posC.y < top && limitPos) {
                posC.y = top;
            } else
                Handles.DrawLine(posC, new Vector2(posC.x + 10, rectC.y + rectC.height / 2));

            Handles.DrawLine(posP, posC);
            Handles.DrawLine(posP, new Vector2(posP.x + 10, rectP.y + rectP.height / 2));
        }

        private void DrawAvatarBonesHierarchy(int boneID, bool singleBranch) {
            if (boneID >= tgt.avatar.previewBones.Count) {
                Debug.LogError("Ragdoll avatar is damaged. Some bones are lost.");
                return;
            }
            PreviewBone bone = tgt.avatar.previewBones[boneID];
            if (!singleBranch && bone.childIDs != null) GUILayout.BeginVertical();

            //change color if connected
            bool isConnected = false;
            int characterBoneID = -1;

            for (int i = 0; i < tgt.characterBones.Count; i++) {
                if (bone.name.Equals(tgt.characterBones[i].avatarConnection)) {
                    characterBoneID = i;
                    isConnected = true;
                    break;
                }
            }

            DrawBoneBox(bone.name, out bone.rect, Color.red, !isConnected);

            if (bone.childIDs != null) {
                if (bone.childIDs.Count > 1) GUILayout.BeginHorizontal();

                for (int i = 0; i < bone.childIDs.Count; i++) {
                    int id = bone.childIDs[i];
                    DrawAvatarBonesHierarchy(id, bone.childIDs.Count == 1);
                }

                if (bone.childIDs.Count > 1) GUILayout.EndHorizontal();
            }

            if (!singleBranch && bone.childIDs != null) GUILayout.EndVertical();

            //bottom line to childs
            if (!bone.IsRoot) {
                DrawBottomLineToChilds(bone.rect);
            }

            Handles.BeginGUI();
            if (isConnected)
                DrawConnector(bone.rect, tgt.characterBones[characterBoneID].rect, Color.gray, true);
            Handles.EndGUI();
        }

        private void DrawBonesHierarchy(CharacterBone bone, bool singleBranch, bool isPreview, bool isCharacterBones) {
            if (!singleBranch && bone.childIDs != null) GUILayout.BeginVertical();

            if (isCharacterBones)
                DrawScrollBoneBox(bone.name, out bone.rect, Color.green, bone.selected);
            else
                DrawBoneBox(bone.name, out bone.rect, Color.green, bone.selected);

            if (bone.childIDs != null) {
                if (bone.childIDs.Count > 1) GUILayout.BeginHorizontal();

                for (int i = 0; i < bone.childIDs.Count; i++) {
                    int id = bone.childIDs[i];
                    CharacterBone child = isPreview ? tgt.previewBones[id] : tgt.characterBones[id];
                    DrawBonesHierarchy(child, bone.childIDs.Count == 1, isPreview, isCharacterBones);
                }

                if (bone.childIDs.Count > 1) GUILayout.EndHorizontal();
            }

            if (!singleBranch && bone.childIDs != null) GUILayout.EndVertical();

            //bottom line to childs
            if (bone.IsRoot) return;
            DrawBottomLineToChilds(bone.rect);
        }

        public static void DrawBoneBox(string name, out Rect rect, Color color, bool change) {
            if (change) GUI.backgroundColor = color;
            rect = GUILayoutUtility.GetRect(new GUIContent(name), "HelpBox");
            GUI.Box(rect, name, "HelpBox");
            if (change) GUI.backgroundColor = Color.white;
        }

        public void DrawScrollBoneBox(string name, out Rect rect, Color color, bool change) {
            if (change) GUI.backgroundColor = color;
            rect = GUILayoutUtility.GetRect(new GUIContent(name), "HelpBox");
            float right = Screen.width + scrollPosition.x;
            float left = scrollPosition.x;
            if (rect.x < right && rect.x + rect.width > left) {
                if (rect.x < left) {
                    float width = rect.width - (left - rect.x);
                    if (width > 9) {
                        rect.width = width;
                        rect.x = left;

                    }
                }
                GUI.Box(rect, name, "HelpBox");
            }
            if (change) GUI.backgroundColor = Color.white;
        }

        public static void DrawBottomLineToChilds(Rect rect) {
            Handles.BeginGUI();
            Handles.color = Color.black;
            Vector2 fpos = rect.position + new Vector2(rect.width / 2, 0);
            Handles.DrawLine(fpos, fpos - new Vector2(0, 4));
            Handles.EndGUI();
        }
    }
}