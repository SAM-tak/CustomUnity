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
        static SerializedProperty sortinglayer = null;
        static SerializedProperty SortingLayer {
            get {
                if(sortinglayer == null) {
                    var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                    sortinglayer = tagManager.FindProperty("m_SortingLayers");
                }
                return sortinglayer;
            }
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var layers = new string[SortingLayer.arraySize];
            for(int i = 0; i < SortingLayer.arraySize; i++) layers[i] = SortingLayer.GetArrayElementAtIndex(i).displayName;
            var selectedIndex = Array.FindIndex(layers, x => x == property.stringValue);
            if(selectedIndex == -1) selectedIndex = Array.FindIndex(layers, x => x.Equals("Default"));
            selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, layers);

            if(0 <= selectedIndex && selectedIndex < layers.Length) property.stringValue = layers[selectedIndex];
        }
    }
}