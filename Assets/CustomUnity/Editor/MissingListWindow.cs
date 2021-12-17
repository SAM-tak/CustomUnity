using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace CustomUnity
{
    public class MissingListWindow : FindAssetResultWindow
    {
        static readonly string[] extensions = { ".scene", ".prefab", ".mat", ".controller", ".shader", ".mask", ".asset" };
        static readonly string[] componentExtensions = { ".scene", ".prefab", ".controller" };

        /// <summary>
        /// Missingがあるアセットを検索してそのリストを表示する
        /// </summary>
        [MenuItem("Assets/Find Missing References...")]
        internal static void ShowMissingList()
        {
            // Missingがあるアセットを検索
            var missingList = Search(false);

            if(missingList.Count > 0) {
                // ウィンドウを表示
                var window = GetWindow<MissingListWindow>();
                window.assetList = missingList;
                window.minSize = new Vector2(900, 300);
            }
        }

        /// <summary>
        /// Missingがあるアセットを検索してそのリストを表示する
        /// </summary>
        [MenuItem("Assets/Find Missing Components...")]
        internal static void ShowMissingComponentList()
        {
            // Missingがあるアセットを検索
            var missingList = Search(true);

            if(missingList.Count > 0) {
                // ウィンドウを表示
                var window = GetWindow<MissingListWindow>();
                window.assetList = missingList;
                window.minSize = new Vector2(900, 300);
            }
        }

        /// <summary>
        /// Missingがあるアセットを検索
        /// </summary>
        static List<Entry> Search(bool componentOnly)
        {
            var missingList = new List<Entry>();

            //Debug.Log(string.Join(" ", Selection.objects.Select(x => x.name).ToArray()));

            var selPaths = Selection.objects.Where(x => {
                var path = AssetDatabase.GetAssetPath(x);
                return !string.IsNullOrEmpty(path) && Directory.Exists(path);
            }).Select(AssetDatabase.GetAssetPath).ToArray();

            //Debug.Log(string.Join(" ", selPaths));

            //if(selPaths == null || selPaths.Length == 0) allPaths = AssetDatabase.GetAllAssetPaths();
            var allPaths = selPaths == null || selPaths.Length == 0 ? AssetDatabase.GetAllAssetPaths() : selPaths.Where(x => AssetDatabase.IsValidFolder(x)).ToArray();
            var length = allPaths.Length;

            //Debug.Log(string.Join(" ", allPaths));

            try {
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                for(int i = 0; i < length; i++) {
                    if(i == 0 || sw.ElapsedMilliseconds > 100) {
                        // プログレスバーを表示
                        if(EditorUtility.DisplayCancelableProgressBar($"Search Missing : {missingList.Count} found", $"{i + 1}/{length}", (float)i / length)) break;
                        sw.Restart();
                    }

                    // Missing状態のプロパティを検索
                    if((componentOnly ? componentExtensions : extensions).Contains(Path.GetExtension(allPaths[i]))) {
                        SearchMissing(allPaths[i], componentOnly, missingList);
                    }
                }
                
                sw.Stop();
            }
            catch(System.Exception ex) {
                Log.Exception(ex);
            }
            // プログレスバーを消す
            EditorUtility.ClearProgressBar();

            Debug.Log(missingList.Count + " missings found.");

            return missingList;
        }

        /// <summary>
        /// 指定アセットにMissingのプロパティがあれば、それをmissingListに追加する
        /// </summary>
        /// <param name="path">Path.</param>
        static void SearchMissing(string path, bool componentOnly, List<Entry> missingList)
        {
            // 指定パスのアセットを全て取得
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);

            // 各アセットについて、Missingのプロパティがあるかチェック
            foreach(var asset in assets) {
                if(asset == null || asset.name == "Deprecated EditorExtensionImpl") continue;

                // SerializedObjectを通してアセットのプロパティを取得する
                var serializedObject = new SerializedObject(asset);
                var property = serializedObject.GetIterator();

                while(property.Next(true)) {
                    // プロパティの種類がオブジェクト（アセット）への参照で、
                    // その参照がnullなのにもかかわらず、参照先インスタンスIDが0でないものはMissing状態！
                    if(property.propertyType == SerializedPropertyType.ObjectReference
                       && (componentOnly ? property.name == "component" || property.name == "m_Script" : property.name != "component" && property.name != "m_Script")
                       && property.objectReferenceValue == null
                       && property.objectReferenceInstanceIDValue != 0) {
                        // Missing状態のプロパティリストに追加する
                        missingList.Add(new Entry {
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