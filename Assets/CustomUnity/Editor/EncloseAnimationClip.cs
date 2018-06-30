using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace CustomUnity
{
    public class EncloseAnimationClip : EditorWindow
    {
        AnimatorController controller;

        string clipName;

        const string menuString = "Assets/Enclose AnimationClip";
        const int priority = 51;

        [MenuItem(menuString, priority = priority)]
        static void Open()
        {
            var window = GetWindow<EncloseAnimationClip>(true, "Enclose AnimationClip", true);
            window.controller = Selection.activeObject as AnimatorController;
        }

        [MenuItem(menuString, priority = priority, validate = true)]
        static bool Validate()
        {
            return Selection.activeObject is AnimatorController;
        }

        static readonly GUIContent removeIcon = new GUIContent(EditorGUIUtility.IconContent("Toolbar Minus").image, "Remove clip");
        static readonly GUIContent extractIcon = new GUIContent(EditorGUIUtility.IconContent("BuildSettings.Standalone.Small").image, "Extract clip");

        void OnGUI()
        {
            EditorGUILayout.LabelField("Animator Controller");
            controller = EditorGUILayout.ObjectField(controller, typeof(AnimatorController), false) as AnimatorController;

            if (controller == null) return;

            var clipList = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(controller)).Where(x => x is AnimationClip).Select(x => x as AnimationClip).ToList();

            var dropArea = EditorGUILayout.BeginVertical("box");

            EditorGUILayout.HelpBox("Drop AnimationClip to add here.", MessageType.Info, true);

            bool dirty = false;

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
                        if(clipList.Exists(item => item.name == animationClip.name) || string.IsNullOrEmpty(animationClip.name)) {
                            EditorUtility.DisplayDialog("Error", "can't add an AnimationClip has duplicate or empty name", "OK");
                        }
                        else {
                            var cloned = Instantiate(animationClip);
                            cloned.name = animationClip.name;
                            AssetDatabase.AddObjectToAsset(cloned, controller);
                            dirty = true;
                        }
                    }
                }
                break;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            clipName = EditorGUILayout.TextField(clipName);

            var invalid = clipList.Exists(item => item.name == clipName) || string.IsNullOrEmpty(clipName);
            EditorGUI.BeginDisabledGroup(invalid);
            if(GUILayout.Button("Add", GUILayout.Width(60))) {
                var animationClip = AnimatorController.AllocateAnimatorClip(clipName);
                AssetDatabase.AddObjectToAsset(animationClip, controller);
                dirty = true;
                clipName = null;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            if(invalid) EditorGUILayout.HelpBox("can't create duplicate names or empty", MessageType.Warning);

            if(clipList.Count > 0) {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Enclosed Clips");
                foreach(var enclosedClip in clipList) {
                    EditorGUILayout.BeginHorizontal();

                    var newName = EditorGUILayout.DelayedTextField(enclosedClip.name);

                    if(newName != enclosedClip.name && !clipList.Exists(item => item.name == newName) && !string.IsNullOrEmpty(newName)) {
                        enclosedClip.name = newName;
                        dirty = true;
                    }

                    if(GUILayout.Button(removeIcon, GUILayout.Width(20))) {
                        DestroyImmediate(enclosedClip, true);
                        dirty = true;
                    }
                    if(GUILayout.Button(extractIcon, GUILayout.Width(20))) {
                        var cloned = Instantiate(enclosedClip) as AnimationClip;
                        cloned.name = enclosedClip.name;
                        var destinationPath = Path.Combine(Path.GetDirectoryName(AssetDatabase.GetAssetPath(controller)), cloned.name + ".anim");
                        if(File.Exists(destinationPath)) {
                            EditorUtility.DisplayDialog("Error", "\"" + destinationPath + "\" is already exists.", "OK");
                        }
                        else {
                            Debug.Log("extract to " + destinationPath);
                            AssetDatabase.CreateAsset(cloned, destinationPath);
                            dirty = true;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            if(dirty) {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }
}