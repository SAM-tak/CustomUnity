using UnityEngine;
using UnityEditor;

namespace CustomUnity
{
    [CustomEditor(typeof(AssetBundleManager))]
    public class AssetBundleManagerInspector : Editor
    {
        SerializedProperty script;
        SerializedProperty foldoutManifest;
        SerializedProperty foldoutLoadedAssetBundles;
        SerializedProperty foldoutDownLoadings;

        void OnEnable()
        {
            script = serializedObject.FindProperty("m_Script");
            foldoutManifest = serializedObject.FindProperty("foldoutManifest");
            foldoutLoadedAssetBundles = serializedObject.FindProperty("foldoutLoadedAssetBundles");
            foldoutDownLoadings = serializedObject.FindProperty("foldoutDownLoadings");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(false);
            EditorGUILayout.PropertyField(script);
            EditorGUI.EndDisabledGroup();

            if(AssetBundleManager.SimulateAssetBundleInEditor) return;

            EditorGUILayout.LabelField("BaseDownloadingURL : " + AssetBundleManager.BaseDownloadingURL);

            if(AssetBundleManager.Manifest) {
                foldoutManifest.isExpanded = EditorGUILayout.Foldout(foldoutManifest.isExpanded, "Manifests");
                if(foldoutManifest.isExpanded) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("Name", "Hash");
                    foreach(var i in AssetBundleManager.Manifest.GetAllAssetBundles()) {
                        EditorGUILayout.LabelField(i, AssetBundleManager.Manifest.GetAssetBundleHash(i).ToString());
                    }
                    var bundlesWithVariant = AssetBundleManager.Manifest.GetAllAssetBundlesWithVariant();
                    if(bundlesWithVariant != null && bundlesWithVariant.Length > 0) {
                        EditorGUILayout.Space();
                        foreach(var i in AssetBundleManager.Manifest.GetAllAssetBundlesWithVariant()) {
                            EditorGUILayout.LabelField(i, AssetBundleManager.Manifest.GetAssetBundleHash(i).ToString());
                        }
                    }
                    EditorGUI.indentLevel--;
                }
            }

            foldoutLoadedAssetBundles.isExpanded = EditorGUILayout.Foldout(foldoutLoadedAssetBundles.isExpanded, "Loaded Asset Bundles");
            if(foldoutLoadedAssetBundles.isExpanded) {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Name", "Reference Count");
                foreach(var i in AssetBundleManager.LoadedAssetBundles) {
                    EditorGUILayout.LabelField(i.Key, i.Value.ReferencedCount.ToString());
                }
                EditorGUI.indentLevel--;
            }

            foldoutDownLoadings.isExpanded = EditorGUILayout.Foldout(foldoutDownLoadings.isExpanded, "DownLoadings");
            if(foldoutDownLoadings.isExpanded) {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Name", "Progress");
                foreach(var i in AssetBundleManager.InProgressOperations) {
                    var op = i as AssetBundleDownloadOperation;
                    if(op != null) EditorGUILayout.LabelField(op.AssetBundleName, op.Progress().ToString());
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}