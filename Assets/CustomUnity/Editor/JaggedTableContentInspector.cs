using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace CustomUnity
{
    [CustomEditor(typeof(JaggedTableContent))]
    //[CanEditMultipleObjects]
    public class JaggedTableContentInspector : Editor
    {
        // void OnEnable()
        // {
        //     // TODO: find properties we want to work with
        //     //serializedObject.FindProperty();
        // }

        public override void OnInspectorGUI()
        {
            if(!Application.isPlaying) {
                var tableContent = target as JaggedTableContent;
                var layoutGroup = tableContent.GetComponent<LayoutGroup>();
                if(layoutGroup != null && layoutGroup.enabled) {
                    EditorGUILayout.HelpBox("Layout Group Component will be disabled automatically by this component in runtime.", MessageType.Warning);
                }
            }
            // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
            serializedObject.Update();
            // TODO: Draw UI here
            //EditorGUILayout.PropertyField();
            DrawDefaultInspector();

            if(Application.isPlaying) {
                var tableContent = target as JaggedTableContent;
                if(tableContent.DataSource == null) EditorGUILayout.HelpBox("DataSource is null", MessageType.Warning);
                else EditorGUILayout.HelpBox($"DataSource ({tableContent.DataSource.GetType()})\nLength : {tableContent.DataSource.TotalCount}", MessageType.Info);
                if(tableContent.MaxCellsRequired > tableContent.MaxCells) {
                    EditorGUILayout.HelpBox($"Short of Cell Object : Required {tableContent.MaxCellsRequired} / Pooled {tableContent.MaxCells}", MessageType.Warning);
                }
            }

            // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
            serializedObject.ApplyModifiedProperties();
        }
    }
}