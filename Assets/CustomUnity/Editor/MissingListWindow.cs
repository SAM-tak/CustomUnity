using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace CustomUnity
{
    public class MissingListWindow : EditorWindow
    {
        static readonly string[] extensions = { ".scene", ".prefab", ".mat", ".controller", ".shader", ".mask", ".asset" };
        static readonly string[] componentExtensions = { ".scene", ".prefab", ".controller" };

        static readonly List<Entry> missingList = new List<Entry>();

        Vector2 scrollPos;

        /// <summary>
        /// Missingがあるアセットを検索してそのリストを表示する
        /// </summary>
        [MenuItem("Assets/Find Missing References...")]
        static void ShowMissingList()
        {
            // Missingがあるアセットを検索
            Search(false);

            if(missingList.Count > 0) {
                // ウィンドウを表示
                var window = GetWindow<MissingListWindow>();
                window.minSize = new Vector2(900, 300);
            }
        }

        /// <summary>
        /// Missingがあるアセットを検索してそのリストを表示する
        /// </summary>
        [MenuItem("Assets/Find Missing Components...")]
        static void ShowMissingComponentList()
        {
            // Missingがあるアセットを検索
            Search(true);

            if(missingList.Count > 0) {
                // ウィンドウを表示
                var window = GetWindow<MissingListWindow>();
                window.minSize = new Vector2(900, 300);
            }
        }

        /// <summary>
        /// Missingがあるアセットを検索
        /// </summary>
        static void Search(bool componentOnly)
        {
            missingList.Clear();

            //Debug.Log(string.Join(" ", Selection.objects.Select(x => x.name).ToArray()));

            var selPaths = Selection.objects.Where(x => {
                var path = AssetDatabase.GetAssetPath(x);
                return !string.IsNullOrEmpty(path) && Directory.Exists(path);
            }).Select(AssetDatabase.GetAssetPath).ToArray();

            //Debug.Log(string.Join(" ", selPaths));

            //if(selPaths == null || selPaths.Length == 0) allPaths = AssetDatabase.GetAllAssetPaths();
            var allPaths = selPaths == null || selPaths.Length == 0
                         ? AssetDatabase.GetAllAssetPaths()
                         : AssetDatabase.GetAllAssetPaths().Where(x => selPaths.Any(y => x.StartsWith(y, System.StringComparison.CurrentCulture))).ToArray();
            var length = allPaths.Length;

            //Debug.Log(string.Join(" ", allPaths));

            try {
                for(int i = 0; i < length; i++) {
                    // プログレスバーを表示
                    if(i % 50 == 0 && Progress(i, length)) break;

                    // Missing状態のプロパティを検索
                    if((componentOnly ? componentExtensions : extensions).Contains(Path.GetExtension(allPaths[i]))) {
                        if(i % 50 != 0 && Progress(i, length)) break;
                        SearchMissing(allPaths[i], componentOnly);
                    }
                }
            }
            catch(System.Exception ex) {
                Log.Exception(ex);
            }
            // プログレスバーを消す
            EditorUtility.ClearProgressBar();

            Debug.Log(missingList.Count + " missings found.");
        }

        /// <summary>
        /// 指定アセットにMissingのプロパティがあれば、それをmissingListに追加する
        /// </summary>
        /// <param name="path">Path.</param>
        static void SearchMissing(string path, bool componentOnly)
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
                       && (componentOnly ? property.name == "component" : property.name != "component")
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

        static bool Progress(int i, int length)
        {
            return EditorUtility.DisplayCancelableProgressBar(string.Format("Search Missing : {0} found", missingList.Count),
                                                              string.Format("{0}/{1}", i + 1, length), (float)i / length);
        }

        /// <summary>
        /// Missingのリストを表示
        /// </summary>
        void OnGUI()
        {
            // 列見出し
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Asset", GUILayout.Width(200));
            EditorGUILayout.LabelField("Property", GUILayout.Width(200));
            EditorGUILayout.LabelField("Path");
            EditorGUILayout.EndHorizontal();

            // リスト表示
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            foreach(var data in missingList) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(data.asset, data.asset.GetType(), true, GUILayout.Width(200));
                EditorGUILayout.TextField(data.propertyName + " : " + data.instanceID, GUILayout.Width(200));
                EditorGUILayout.TextField(data.path);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        class Entry
        {
            /// アセットのObject自体
            public Object asset;
            /// アセットのパス
            public string path;
            /// プロパティ名
            public string propertyName;
            /// プロパティパス
            public string propertyPath;
            /// インスタンスID
            public int instanceID;
        }
    }
}