using UnityEditor;

namespace CustomUnity
{
    [CustomEditor(typeof(AssetBundleLoader))]
    public class AssetBundleLoaderInspector : Editor
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

            if(AssetBundleLoader.SimulatesAssetBundleInEditor) return;

            EditorGUILayout.LabelField("BaseDownloadingURL : " + AssetBundleLoader.BaseDownloadingURL);

            if(AssetBundleLoader.Manifest) {
                foldoutManifest.isExpanded = EditorGUILayout.Foldout(foldoutManifest.isExpanded, "Manifests");
                if(foldoutManifest.isExpanded) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("Name", "Hash");
                    foreach(var i in AssetBundleLoader.Manifest.GetAllAssetBundles()) {
                        EditorGUILayout.LabelField(i, AssetBundleLoader.Manifest.GetAssetBundleHash(i).ToString());
                    }
                    EditorGUI.indentLevel--;
                }
            }

            foldoutLoadedAssetBundles.isExpanded = EditorGUILayout.Foldout(foldoutLoadedAssetBundles.isExpanded, "Loaded Asset Bundles");
            if(foldoutLoadedAssetBundles.isExpanded) {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Name", "Reference Count");
                foreach(var i in AssetBundleLoader.LoadedAssetBundles) {
                    EditorGUILayout.LabelField(i.Key, i.Value.ReferencedCount.ToString());
                }
                EditorGUI.indentLevel--;
            }

            foldoutDownLoadings.isExpanded = EditorGUILayout.Foldout(foldoutDownLoadings.isExpanded, "DownLoadings");
            if(foldoutDownLoadings.isExpanded) {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Name", "Progress");
                foreach(var i in AssetBundleLoader.InProgressOperations) {
                    var op = i as AssetBundleDownloadOperation;
                    if(op != null) EditorGUILayout.LabelField(op.AssetBundleName, op.Progress().ToString());
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}