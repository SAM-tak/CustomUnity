using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;
using UnityEditor;

namespace CustomUnity
{
	public class EncloseAnimationClip : EditorWindow
	{
		private AnimatorController controller;

		string clipName;

		[MenuItem("Assets/Enclose AnimationClip")]
		static void Create()
		{
			var window = EncloseAnimationClip.GetWindow(typeof(EncloseAnimationClip)) as EncloseAnimationClip;
			if(Selection.activeObject is AnimatorController) {
				window.controller = Selection.activeObject as AnimatorController;
			}
		}

		void OnGUI()
		{
			EditorGUILayout.LabelField("Target Animator Controller");
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

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Add new clip");
			EditorGUILayout.BeginVertical("box");
			
			clipName = EditorGUILayout.TextField(clipName);
			
			if(clipList.Exists(item=> item.name == clipName ) || string.IsNullOrEmpty(clipName)) {
				EditorGUILayout.LabelField("can't create duplicate names or empty");
			}
			else{
				if(GUILayout.Button("Create")) {
					var animationClip = UnityEditor.Animations.AnimatorController.AllocateAnimatorClip(clipName);
					AssetDatabase.AddObjectToAsset(animationClip, controller);
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(controller));
					AssetDatabase.Refresh();
				}
			}
			EditorGUILayout.EndVertical();

			if(clipList.Count == 0) return;

			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Enclosed Clips");
			EditorGUILayout.BeginVertical("box");

			foreach(var removeClip in clipList) {
				EditorGUILayout.BeginHorizontal();
				
				EditorGUILayout.LabelField(removeClip.name);
				if(GUILayout.Button("Remove" , GUILayout.Width(100))) {
					Object.DestroyImmediate(removeClip, true);
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(controller));
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();
		}
	}
}