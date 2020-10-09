using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CustomUnity
{
    public class FindAssetResultWindow : EditorWindow
    {
        protected List<Entry> assetList;

        Vector2 scrollPos;

        /// <summary>
        /// 結果を表示
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

            if(assetList != null) {
                foreach(var data in assetList) {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField(data.asset, data.asset.GetType(), true, GUILayout.Width(200));
                    EditorGUILayout.TextField($"{data.propertyName} : {data.instanceID}", GUILayout.Width(200));
                    EditorGUILayout.TextField(data.path);
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        protected class Entry
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