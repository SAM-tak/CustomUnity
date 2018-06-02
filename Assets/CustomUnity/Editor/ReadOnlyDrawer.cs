using UnityEngine;
using UnityEditor;

namespace CustomUnity
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var previousEnabled = GUI.enabled;
            GUI.enabled = false;
            Log.Info(property.propertyPath);
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = previousEnabled;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, property.isExpanded);
        }
    }

    [CustomPropertyDrawer(typeof(ReadOnlyEnumFlagsAttribute))]
    public class ReadOnlyEnumFlagsDrawer : PropertyDrawer
    {
        static GUIStyle customStyle;
        static GUIStyle CustomStyle {
            get {
                if(customStyle == null) {
                    customStyle = new GUIStyle(EditorStyles.layerMaskField);
                    customStyle.normal.textColor = customStyle.hover.textColor = customStyle.focused.textColor = Color.gray;
                }
                return customStyle;
            }
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.MaskField(position, label, property.intValue, property.enumNames, CustomStyle);
        }
    }

    [CustomPropertyDrawer(typeof(ReadOnlyWhenPlayingAttribute))]
    public class ReadOnlyWhenPlayingAttributeDrawer : PropertyDrawer
    {
        // Necessary since some properties tend to collapse smaller than their content
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        // Draw a disabled property field
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = !Application.isPlaying;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
    
    [CustomPropertyDrawer(typeof(ReadOnlyEnumFlagsWhenPlayingAttribute))]
    public class ReadOnlyEnumFlagsWhenPlayingDrawer : PropertyDrawer
    {
        static GUIStyle customStyle;
        static GUIStyle CustomStyle {
            get {
                if(customStyle == null) {
                    customStyle = new GUIStyle(EditorStyles.layerMaskField);
                    customStyle.normal.textColor = customStyle.hover.textColor = customStyle.focused.textColor = Color.gray;
                }
                return customStyle;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(Application.isPlaying) EditorGUI.MaskField(position, label, property.intValue, property.enumNames, CustomStyle);
            else EditorGUI.MaskField(position, label, property.intValue, property.enumNames);
        }
    }
}
