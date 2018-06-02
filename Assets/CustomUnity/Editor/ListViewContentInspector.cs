using UnityEngine;
using UnityEditor;

namespace CustomUnity
{
    [CustomEditor(typeof(ListViewContent))]
    //[CanEditMultipleObjects]
    public class ListViewContentInspector : Editor
    {
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

            if(Application.isPlaying) {
                var contentFilter = target as ListViewContent;
                if(contentFilter.DataSource == null) EditorGUILayout.HelpBox("DataSource is null", MessageType.Warning);
                else EditorGUILayout.HelpBox($"DataSource ({contentFilter.DataSource.GetType()})\nLength : {contentFilter.DataSource.TotalCount}", MessageType.Info);
                if(contentFilter.MaxCellsRequired > contentFilter.MaxCells) {
                    EditorGUILayout.HelpBox($"Short of Cell Object : Required {contentFilter.MaxCellsRequired} / Pooled {contentFilter.MaxCells}", MessageType.Warning);
                }
            }

            // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
            serializedObject.ApplyModifiedProperties();
        }
    }
}