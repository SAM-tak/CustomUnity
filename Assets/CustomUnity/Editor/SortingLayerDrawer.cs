/// Set Sorting Layer
/// Copyright (c) 2014 Tatsuhiko Yamamura
/// Released under the MIT license
// / http://opensource.org/licenses/mit-license.php

using System;
using UnityEngine;
using UnityEditor;

namespace CustomUnity
{
    [CustomPropertyDrawer(typeof(SortingLayerAttribute))]
    public class SortingLayerDrawerAttribute : PropertyDrawer
    {
        static string[] sortingLayerNames = null;
        static string[] SortingLayerNames {
            get {
                if(sortingLayerNames == null) {
                    var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                    var sortinglayers = tagManager.FindProperty("m_SortingLayers");
                    sortingLayerNames = new string[sortinglayers.arraySize + 2];
                    for(int i = 0; i < sortinglayers.arraySize; i++) sortingLayerNames[i] = sortinglayers.GetArrayElementAtIndex(i).displayName;
                    sortingLayerNames[sortinglayers.arraySize + 1] = "Refrash Sorting Layer List";
                }
                return sortingLayerNames;
            }
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var selectedIndex = Array.FindIndex(SortingLayerNames, x => x == property.stringValue);
            if(selectedIndex == -1) selectedIndex = Array.FindIndex(SortingLayerNames, x => x.Equals("Default"));
            selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, SortingLayerNames);

            if(0 <= selectedIndex) {
                if(selectedIndex < SortingLayerNames.Length - 1) property.stringValue = SortingLayerNames[selectedIndex];
                else sortingLayerNames = null;
            }
        }
    }
}