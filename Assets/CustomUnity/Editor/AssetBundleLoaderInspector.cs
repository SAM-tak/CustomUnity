using UnityEditor;

namespace CustomUnity
{
    [CustomEditor(typeof(AssetBundleLoader))]
    public class AssetBundleLoaderInspector : Editor
    {
        SerializedProperty script;
        SerializedProperty foldoutManifest;
        SerializedProperty foldoutLoadedAssetBundles;
        SerializedProperty foldoutDownloadings;
        SerializedProperty foldoutAssetLoadings;

        void OnEnable()
        {
            script = serializedObject.FindProperty("m_Script");
            foldoutManifest = serializedObject.FindProperty("foldoutManifest");
            foldoutLoadedAssetBundles = serializedObject.FindProperty("foldoutLoadedAssetBundles");
            foldoutDownloadings = serializedObject.FindProperty("foldoutDownloadings");
            foldoutAssetLoadings = serializedObject.FindProperty("foldoutAssetLoadings");
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
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Name", "Reference Count");
                EditorGUILayout.LabelField("Implicity");
                EditorGUILayout.EndHorizontal();
                foreach(var i in AssetBundleLoader.LoadedAssetBundles) {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(i.Key, i.Value.ReferencedCount.ToString());
                    EditorGUILayout.LabelField(i.Value.IsImplicit.ToString());
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }

            foldoutDownloadings.isExpanded = EditorGUILayout.Foldout(foldoutDownloadings.isExpanded, "Downloadings");
            if(foldoutDownloadings.isExpanded) {
                EditorGUI.indentLevel++;
                foreach(var i in AssetBundleLoader.InProgressOperations) {
                    var op = i as AssetBundleDownloadOperation;
                    if(op != null) EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), op.Progress(), op.AssetBundleName);
                }
                EditorGUI.indentLevel--;
            }

            foldoutAssetLoadings.isExpanded = EditorGUILayout.Foldout(foldoutAssetLoadings.isExpanded, "Asset Loadings");
            if(foldoutAssetLoadings.isExpanded) {
                EditorGUI.indentLevel++;
                foreach(var i in AssetBundleLoader.InProgressOperations) {
                    var op = i as AssetBundleLoadAssetOperationFull;
                    if(op != null) EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), op.Progress(), op.AssetBundleName + "/" + op.AssetName);
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}