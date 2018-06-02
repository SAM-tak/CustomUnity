using UnityEngine;
using UnityEditor;

namespace CustomUnity
{
    [CustomEditor(typeof(FixedSizeListViewContent))]
    //[CanEditMultipleObjects]
    public class FixedSizeListViewContentInspector : Editor
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
                var listViewContent = target as FixedSizeListViewContent;
                if(listViewContent.DataSource == null) EditorGUILayout.HelpBox("DataSource is null", MessageType.Warning);
                else EditorGUILayout.HelpBox($"DataSource ({listViewContent.DataSource.GetType()})\nLength : {listViewContent.DataSource.TotalCount}\n{listViewContent.StartIndex} to {listViewContent.EndIndex}", MessageType.Info);
                if(listViewContent.MaxCellsRequired > listViewContent.MaxCells) {
                    EditorGUILayout.HelpBox($"Short of Cell Object : Required {listViewContent.MaxCellsRequired} / Pooled {listViewContent.MaxCells}", MessageType.Warning);
                }
            }

            // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
            serializedObject.ApplyModifiedProperties();
        }
    }
}