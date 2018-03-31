using UnityEngine;
using UnityEditor;

namespace CustomUnity
{
    [CustomEditor(typeof(SumSomeFlagsEvent))]
    [CanEditMultipleObjects]
    public class SumSomeFlagsEventInspector : Editor
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

        void OnEnable()
        {
            // TODO: find properties we want to work with
            //serializedObject.FindProperty();
        }

        public override void OnInspectorGUI()
        {
            // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
            serializedObject.Update();
            // TODO: Draw UI here
            //EditorGUILayout.PropertyField();
            DrawDefaultInspector();
            // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            EditorGUILayout.EnumFlagsField("Current Flags", ((SumSomeFlagsEvent)target).CurrentFlags, CustomStyle);
        }
    }
}