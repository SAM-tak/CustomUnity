using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace CustomUnity
{
    public class EncloseAnimationClip
    {
        string newClipName;

        Vector2 scrollPosition = Vector2.zero;

        public void DrawGUI(Object target, System.Action<AnimationClip, AnimationClip> replaceReference)
        {
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
                    foreach(var animationClip in DragAndDrop.objectReferences.Cast<AnimationClip>()) {
                        if(clips.Any(item => item.name == animationClip.name) || string.IsNullOrEmpty(animationClip.name)) {
                            EditorUtility.DisplayDialog("Error", "can't add an AnimationClip has duplicate or empty name", "OK");
                        }
                        else {
                            var cloned = Object.Instantiate(animationClip);
                            cloned.name = animationClip.name;
                            AssetDatabase.AddObjectToAsset(cloned, target);
                            replaceReference(animationClip, cloned);
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
                        Object.DestroyImmediate(enclosedClip, true);
                        dirty = true;
                    }
                    if(GUILayout.Button(extractIcon, GUILayout.Width(20))) {
                        var cloned = Object.Instantiate(enclosedClip);
                        cloned.name = enclosedClip.name;
                        var destinationPath = Path.Combine(Path.GetDirectoryName(AssetDatabase.GetAssetPath(target)), cloned.name + ".anim");
                        if(File.Exists(destinationPath)) {
                            EditorUtility.DisplayDialog("Error", "\"" + destinationPath + "\" is already exists.", "OK");
                        }
                        else {
                            Debug.Log("extract to " + destinationPath);
                            AssetDatabase.CreateAsset(cloned, destinationPath);
                            replaceReference(enclosedClip, cloned);
                            Object.DestroyImmediate(enclosedClip, true);
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

        public static void ReplaceReference(AnimatorController animatorController, AnimationClip from, AnimationClip to)
        {
            if (animatorController != null) {
                foreach(var i in animatorController.layers) {
                    foreach(var j in i.stateMachine.states) {
                        if(j.state.motion == from) j.state.motion = to;
                    }
                }
            }
        }

        public static void ReplaceReference(AnimatorOverrideController animatorOverrideController, AnimationClip from, AnimationClip to)
        {
            if (animatorOverrideController != null) {
                var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                animatorOverrideController.GetOverrides(overrides);
                for(int i = 0; i < animatorOverrideController.overridesCount; ++i) {
                    var tmp = overrides[i];
                    if (tmp.Key == from) tmp = new KeyValuePair<AnimationClip, AnimationClip>(to, tmp.Value);
                    if (tmp.Value == from) tmp = new KeyValuePair<AnimationClip, AnimationClip>(tmp.Key, to);
                    overrides[i] = tmp;
                }
                animatorOverrideController.ApplyOverrides(overrides);
            }
        }
    }

    public class EncloseAnimationClipWindow : EditorWindow
    {
        Object target;
        const string menuString = "Assets/Enclose AnimationClip";
        const int priority = 51;
        readonly EncloseAnimationClip encloseAnimationClip = new ();

        [MenuItem(menuString, priority = priority)]
        static public void Open()
        {
            var window = GetWindow<EncloseAnimationClipWindow>(true, "Enclose AnimationClip", true);
            window.target = Selection.activeObject;
        }

        [MenuItem(menuString, priority = priority, validate = true)]
        static public bool Validate()
        {
            if(Selection.activeObject == null) return false;
            var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if(string.IsNullOrEmpty(assetPath)) return false;
            if(Path.GetFileNameWithoutExtension(assetPath) != Selection.activeObject.name) return false;
            var ext = Path.GetExtension(assetPath);
            return ext != ".fbx" && ext != ".dae" && ext != ".3ds" && ext != ".dxf" && ext != ".obj" && ext != ".skp" && ext != ".blender";
        }

        void OnGUI()
        {
            encloseAnimationClip.DrawGUI(target, (from, to) => {
                EncloseAnimationClip.ReplaceReference(target as AnimatorController, from, to);
                EncloseAnimationClip.ReplaceReference(target as AnimatorOverrideController, from, to);
            });
        }
    }

    [CustomEditor(typeof(AnimatorController))]
    public class AnimatorControllerCustomInspector : Editor
    {
        readonly EncloseAnimationClip encloseAnimationClip = new ();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            encloseAnimationClip.DrawGUI(target, (from, to) => EncloseAnimationClip.ReplaceReference(target as AnimatorController, from, to));
        }
    }
}