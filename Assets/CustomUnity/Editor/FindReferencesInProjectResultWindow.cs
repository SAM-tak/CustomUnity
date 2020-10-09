using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace CustomUnity
{
    public class FindReferencesInProjectResultWindow : FindAssetResultWindow
    {
        static readonly string[] extensions = { ".scene", ".prefab", ".mat", ".controller", ".shader", ".mask", ".asset" };

        private const string MenuItemText = "Assets/Find References In Project";

        [MenuItem(MenuItemText, false, 25)]
        internal static void FindReferencesInProject()
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            
            var referencerList = new List<Entry>();
            var allAssetPaths = AssetDatabase.GetAllAssetPaths();
            var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            var length = allAssetPaths.Length;
            for(var i = 0; i < length; i++) {
                var path = allAssetPaths[i];

                // プログレスバーを表示
                if(i == 0 || sw.ElapsedMilliseconds > 100) {
                    if(EditorUtility.DisplayCancelableProgressBar($"Find References : {referencerList.Count} found", $"{i + 1}/{length}", (float)i / length)) break;
                    sw.Restart();
                }

                if(assetPath != path && extensions.Contains(Path.GetExtension(path))) FindReference(path, assetPath, referencerList);
            }

            sw.Stop();

            // プログレスバーを消す
            EditorUtility.ClearProgressBar();

            if(referencerList.Count > 0) {
                // ウィンドウを表示
                var window = GetWindow<FindReferencesInProjectResultWindow>();
                window.assetList = referencerList;
                window.minSize = new Vector2(900, 300);
            }
        }

        [MenuItem(MenuItemText, true)]
        internal static bool Validate()
        {
            if(Selection.activeObject) {
                string path = AssetDatabase.GetAssetPath(Selection.activeObject);
                return !AssetDatabase.IsValidFolder(path);
            }

            return false;
        }

        /// <summary>
        /// 指定アセットに指定のアセットを参照しているプロパティがあれば、それをreferencerListに追加する
        /// </summary>
        /// <param name="path">Asset Path.</param>
        /// <param name="findee">Findee Asset Path.</param>
        static void FindReference(string path, string findee, List<Entry> referencerList)
        {
            // 指定パスのアセットを全て取得
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);

            // 各アセットについて、指定のアセットを参照しているプロパティがあるかチェック
            foreach(var asset in assets) {
                if(asset == null || asset.name == "Deprecated EditorExtensionImpl") continue;

                // SerializedObjectを通してアセットのプロパティを取得する
                var serializedObject = new SerializedObject(asset);
                var property = serializedObject.GetIterator();

                while(property.Next(true)) {
                    if(property.propertyType == SerializedPropertyType.ObjectReference
                        && property.objectReferenceValue != null
                        && AssetDatabase.GetAssetPath(property.objectReferenceValue) == findee) {
                        referencerList.Add(new Entry {
                            asset = asset,
                            path = path,
                            propertyName = property.name,
                            propertyPath = property.propertyPath,
                            instanceID = property.objectReferenceInstanceIDValue
                        });
                    }
                }
            }
        }
    }
}
