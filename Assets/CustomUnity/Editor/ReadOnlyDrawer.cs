using System;
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
            var previousEnabled = GUI.enabled;
            GUI.enabled = !Application.isPlaying;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = previousEnabled;
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

    [CustomPropertyDrawer(typeof(ReadOnlyIfAttribute))]
    public class ReadOnlyIfPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attribute = base.attribute as ReadOnlyIfAttribute;
            var comparedField = property.serializedObject.FindProperty(attribute.ComparedPropertyName);

            // Compare the values to see if the condition is met.
            bool conditionMet = false;

            var field = comparedField != null ? property.serializedObject.targetObject.GetType().GetField(comparedField.propertyPath) : null;
            if(field != null) {
                try {
                    // Get the value of the compared field.
                    object comparedFieldValue = field.GetValue(property.serializedObject.targetObject);

                    if(attribute.ComparisonType < ReadOnlyIfAttribute.Comparison.GreaterThan || comparedFieldValue is IComparable) {
                        switch(attribute.ComparisonType) {
                        case ReadOnlyIfAttribute.Comparison.Equals:
                            conditionMet = comparedFieldValue.Equals(attribute.ComparedValue);
                            break;

                        case ReadOnlyIfAttribute.Comparison.NotEqual:
                            conditionMet = !comparedFieldValue.Equals(attribute.ComparedValue);
                            break;

                        case ReadOnlyIfAttribute.Comparison.GreaterThan:
                            conditionMet = (comparedFieldValue as IComparable).CompareTo(attribute.ComparedValue) > 0;
                            break;

                        case ReadOnlyIfAttribute.Comparison.GreaterOrEqual:
                            conditionMet = (comparedFieldValue as IComparable).CompareTo(attribute.ComparedValue) >= 0;
                            break;

                        case ReadOnlyIfAttribute.Comparison.LessThan:
                            conditionMet = (comparedFieldValue as IComparable).CompareTo(attribute.ComparedValue) < 0;
                            break;

                        case ReadOnlyIfAttribute.Comparison.LessOrEqual:
                            conditionMet = (comparedFieldValue as IComparable).CompareTo(attribute.ComparedValue) <= 0;
                            break;
                        }
                    }
                    else {
                        Debug.LogError(comparedField.type + " is not supported of " + (property.propertyPath.Contains(".") ? System.IO.Path.ChangeExtension(property.propertyPath, attribute.ComparedPropertyName) : attribute.ComparedPropertyName));
                    }
                }
                catch(Exception ex) {
                    Debug.LogException(ex);
                }
            }
            else {
                Debug.LogError(attribute.ComparedPropertyName + " is not found.");
            }

            if(conditionMet) {
                var previousEnabled = GUI.enabled;
                GUI.enabled = false;
                EditorGUI.PropertyField(position, property);
                GUI.enabled = previousEnabled;
            }
            else {
                EditorGUI.PropertyField(position, property);
            }
        }
    }
}
