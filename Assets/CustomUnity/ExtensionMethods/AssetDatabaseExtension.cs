using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;

namespace CustomUnity
{
    public static class AssetDatabaseExtension
    {
        public static void DuplicateAssetsAndReplaceReference(string[] paths, string location)
        {
            if(!AssetDatabase.IsValidFolder(location)) return;
            // Duplicate assets
            var duped = new List<Tuple<string, string, string>>();
            foreach(var i in paths.Distinct()) {
                string fileName = Path.GetFileName(i);
                var j = Path.Combine(location, fileName);
                if(AssetDatabase.CopyAsset(i, j)) {
                    duped.Add(new(j, AssetDatabase.AssetPathToGUID(i), AssetDatabase.AssetPathToGUID(j)));
                }
            }
            // Resolve reference
            bool needsRefresh = false;
            foreach(var i in duped) {
                if(!".unity;.asset;.prefab;.mat;.anim;.controller;.overridecontroller".Contains(Path.GetExtension(i.Item1).ToLower())) continue;
                bool replaced = false;
                var yaml = File.ReadAllText(i.Item1);
                foreach(var j in duped) {
                    if(!i.Item1.Equals(j.Item1)) {
                        var replacedYaml = yaml.Replace($" guid: {j.Item2},", $" guid: {j.Item3},");
                        if(!replacedYaml.Equals(yaml)) {
                            yaml = replacedYaml;
                            replaced = true;
                            Log.Info("replaced");
                        }
                    }
                }
                if(replaced) {
                    File.WriteAllText(i.Item1, yaml);
                    needsRefresh = true;
                }
            }
            if(needsRefresh) AssetDatabase.Refresh();
        }

        const string menuString = "Assets/Duplicate Assets And Replace Reference";

        [MenuItem(menuString)]
        static void DuplicateAssetsAndReplaceReference()
        {
            var selected = Selection.assetGUIDs;
            var folderGUID = selected.First(i => AssetDatabase.IsValidFolder(AssetDatabase.GUIDToAssetPath(i)));
            DuplicateAssetsAndReplaceReference(
                selected.Where(i => !i.Equals(folderGUID)).Select(i => AssetDatabase.GUIDToAssetPath(i)).ToArray(),
                AssetDatabase.GUIDToAssetPath(folderGUID)
            );
        }

        [MenuItem(menuString, true)]
        static bool DuplicateAssetsAndReplaceReferenceValidate()
        {
            var selected = Selection.assetGUIDs;
            return selected.Length > 2 && selected.Count(i => AssetDatabase.IsValidFolder(AssetDatabase.GUIDToAssetPath(i))) == 1;
        }
    }
}
#endif
