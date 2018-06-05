using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

namespace CustomUnity
{
	public class EncloseAnimationClip : EditorWindow
	{
		AnimatorController controller;

		string clipName;

        const string menuString = "Assets/Enclose AnimationClip";
        const int priority = 51;

        [MenuItem(menuString, priority = priority)]
		static void Create()
		{
			var window = GetWindow<EncloseAnimationClip>(true, "Enclose AnimationClip", true);
			window.controller = Selection.activeObject as AnimatorController;
		}

        [MenuItem(menuString, priority = priority, validate = true)]
        static bool Validate()
        {
            return Selection.activeObject is AnimatorController;
        }

        void OnGUI()
		{
			EditorGUILayout.LabelField("Animator Controller");
			controller = EditorGUILayout.ObjectField(controller, typeof(AnimatorController), false) as AnimatorController;

			if(controller == null) return;

			var clipList = new List<AnimationClip>();
			var allAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(controller));
			foreach(var asset in allAssets ) {
				if(asset is AnimationClip) {
					var removeClip = asset as AnimationClip;
					if(! clipList.Contains(removeClip ) ){
						clipList.Add(removeClip);
					}
				}
			}

			var dropArea = EditorGUILayout.BeginVertical("box");

            EditorGUILayout.HelpBox("Drop AnimationClip to add here.", MessageType.Info, true);

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
                            var cloned = Instantiate(animationClip) as AnimationClip;
                            cloned.name = animationClip.name;
                            AssetDatabase.AddObjectToAsset(cloned, controller);
                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(controller));
                            AssetDatabase.Refresh();
                        }
                    }
                }
                break;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Add new AnimationClip");

            clipName = EditorGUILayout.TextField(clipName);
			
			if(clipList.Exists(item=> item.name == clipName ) || string.IsNullOrEmpty(clipName)) {
                EditorGUILayout.HelpBox("can't create duplicate names or empty", MessageType.Warning);
            }
            else {
				if(GUILayout.Button("Create")) {
					var animationClip = AnimatorController.AllocateAnimatorClip(clipName);
					AssetDatabase.AddObjectToAsset(animationClip, controller);
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(controller));
					AssetDatabase.Refresh();
				}
			}
			EditorGUILayout.EndVertical();

			if(clipList.Count == 0) return;

			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Enclosed Clips");

			foreach(var enclosedClip in clipList) {
				EditorGUILayout.BeginHorizontal();
				
				EditorGUILayout.LabelField(enclosedClip.name);
				if(GUILayout.Button("Remove" , GUILayout.Width(80))) {
					DestroyImmediate(enclosedClip, true);
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(controller));
				}
                if(GUILayout.Button("Extract", GUILayout.Width(80))) {
                    var cloned = Instantiate(enclosedClip) as AnimationClip;
                    cloned.name = enclosedClip.name;
                    var destinationPath = Path.Combine(Path.GetDirectoryName(AssetDatabase.GetAssetPath(controller)), cloned.name + ".anim");
                    if(File.Exists(destinationPath)) {
                        EditorUtility.DisplayDialog("Error", "\"" + destinationPath + "\" is already exists.", "OK");
                    }
                    else {
                        Debug.Log("extract to " + destinationPath);
                        AssetDatabase.CreateAsset(cloned, destinationPath);
                    }
                }
                EditorGUILayout.EndHorizontal();
			}
		}
	}
}