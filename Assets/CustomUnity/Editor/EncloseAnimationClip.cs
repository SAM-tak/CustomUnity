using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace CustomUnity
{
    [CustomEditor(typeof(AnimatorController))]
    public class EncloseAnimationClip : Editor
    {
        string newClipName;

        Vector2 scrollPosition = new Vector2(0, 0);

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var clips = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(target)).Where(x => x is AnimationClip).Select(x => x as AnimationClip).ToArray();

            bool dirty = false;

            var dropArea = EditorGUILayout.BeginVertical("box");

            EditorGUILayout.HelpBox("Drop AnimationClip here to enclose.", MessageType.Info, true);

            switch(UnityEngine.Event.current.type) {
            case EventType.DragUpdated:
                if(dropArea.Contains(UnityEngine.Event.current.mousePosition)) {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                }
                break;
            case EventType.DragPerform:
                if(dropArea.Contains(UnityEngine.Event.current.mousePosition)) {
                    DragAndDrop.AcceptDrag();
                    foreach(AnimationClip animationClip in DragAndDrop.objectReferences) {
                        if(clips.Any(item => item.name == animationClip.name) || string.IsNullOrEmpty(animationClip.name)) {
                            EditorUtility.DisplayDialog("Error", "can't add an AnimationClip has duplicate or empty name", "OK");
                        }
                        else {
                            var cloned = Instantiate(animationClip);
                            cloned.name = animationClip.name;
                            AssetDatabase.AddObjectToAsset(cloned, target);
                            ReplaceReference(animationClip, cloned);
                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(animationClip));
                            dirty = true;
                        }
                    }
                }
                break;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("New AnimationClip to enclose");
            EditorGUILayout.BeginHorizontal();
            newClipName = EditorGUILayout.TextField("Name", newClipName);

            var invalid = clips.Any(item => item.name == newClipName) || string.IsNullOrEmpty(newClipName);
            EditorGUI.BeginDisabledGroup(invalid);
            if(GUILayout.Button("Add", GUILayout.Width(60))) {
                var animationClip = AnimatorController.AllocateAnimatorClip(newClipName);
                AssetDatabase.AddObjectToAsset(animationClip, target);
                dirty = true;
                newClipName = null;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            if(invalid && !string.IsNullOrEmpty(newClipName)) EditorGUILayout.HelpBox("can't create duplicate names", MessageType.Warning);

            if(clips.Any()) {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Enclosed Clips");

                var removeIcon = new GUIContent(EditorGUIUtility.IconContent("Toolbar Minus").image, "Remove clip");
                var extractIcon = new GUIContent(EditorGUIUtility.IconContent("BuildSettings.Standalone.Small").image, "Extract clip");

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                foreach(var enclosedClip in clips) {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUI.BeginDisabledGroup((enclosedClip.hideFlags & HideFlags.NotEditable) != 0);

                    var newName = EditorGUILayout.DelayedTextField(enclosedClip.name);

                    if(newName != enclosedClip.name && !string.IsNullOrEmpty(newName) && clips.All(item => item.name != newName)) {
                        enclosedClip.name = newName;
                        dirty = true;
                    }

                    if(GUILayout.Button(removeIcon, GUILayout.Width(20))) {
                        DestroyImmediate(enclosedClip, true);
                        dirty = true;
                    }
                    if(GUILayout.Button(extractIcon, GUILayout.Width(20))) {
                        var cloned = Instantiate(enclosedClip);
                        cloned.name = enclosedClip.name;
                        var destinationPath = Path.Combine(Path.GetDirectoryName(AssetDatabase.GetAssetPath(target)), cloned.name + ".anim");
                        if(File.Exists(destinationPath)) {
                            EditorUtility.DisplayDialog("Error", "\"" + destinationPath + "\" is already exists.", "OK");
                        }
                        else {
                            Debug.Log("extract to " + destinationPath);
                            AssetDatabase.CreateAsset(cloned, destinationPath);
                            ReplaceReference(enclosedClip, cloned);
                            DestroyImmediate(enclosedClip, true);
                            dirty = true;
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }
            if(dirty) {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        void ReplaceReference(AnimationClip from, AnimationClip to)
        {
            var animatorController = target as AnimatorController;
            foreach(var i in animatorController.layers) {
                foreach(var j in i.stateMachine.states) {
                    if(j.state.motion == from) j.state.motion = to;
                }
            }
        }
    }
}