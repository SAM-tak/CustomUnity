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
    public class EnumFlagsReadOnlyDrawer : PropertyDrawer
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
}
