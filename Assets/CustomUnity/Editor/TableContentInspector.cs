using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace CustomUnity
{
    [CustomEditor(typeof(TableContent))]
    //[CanEditMultipleObjects]
    public class TableContentInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            if(!Application.isPlaying) {
                var tableContent = target as TableContent;
                var layoutGroup = tableContent.GetComponent<LayoutGroup>();
                if(layoutGroup != null && layoutGroup.enabled) {
                    EditorGUILayout.HelpBox("Layout Group Component will corrupt table view or cause of glitch. Please disable it before save a prefab/scene or before play.", MessageType.Warning);
                }
            }
            // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
            serializedObject.Update();
            // TODO: Draw UI here
            //EditorGUILayout.PropertyField();
            DrawDefaultInspector();

            if(Application.isPlaying) {
                var tableContent = target as TableContent;
                if(tableContent.DataSource == null) {
                    EditorGUILayout.HelpBox("DataSource is null", MessageType.Warning);
                }
                else {
                    EditorGUILayout.HelpBox($"DataSource ({tableContent.DataSource.GetType()})\nLength : {tableContent.DataSource.TotalCount}", MessageType.Info);
                } 
                if(tableContent.MaxCellsRequired > tableContent.MaxCells) {
                    EditorGUILayout.HelpBox($"Short of Cell Object : Required {tableContent.MaxCellsRequired} / Pooled {tableContent.MaxCells}", MessageType.Warning);
                }
            }

            // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
            serializedObject.ApplyModifiedProperties();
        }
    }
}