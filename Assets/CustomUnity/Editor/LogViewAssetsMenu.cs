using UnityEngine;
using UnityEditor;

namespace CustomUnity
{
    public static class LogViewAssetsMenu
    {
        [MenuItem("GameObject/UI/Log View")]
        static void CreateLogView(MenuCommand menuCommand)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CustomUnity/Prefabs/Log View.prefab");
            if(!prefab) {
                AssetDatabaseExtension.DuplicateAssetsAndReplaceReference(
                    new string[] { "Packages/CustomUnity/Prefabs/Log View.prefab", "Packages/CustomUnity/Prefabs/LogLine.prefab" },
                    "Assets/CustomUnity/Prefabs"
                );
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CustomUnity/Prefabs/Log View.prefab");
            }
            if(prefab) {
                var parent = (Selection.activeObject ? Selection.activeObject : menuCommand.context) as GameObject;
                var go = Object.Instantiate(prefab, parent ? parent.transform : null);
                go.UniqueName(prefab.name);
                // Register the creation in the undo system
                Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
                Selection.activeObject = go;
            }
        }
    }
}
