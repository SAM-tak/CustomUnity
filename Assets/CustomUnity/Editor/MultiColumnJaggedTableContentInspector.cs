using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace CustomUnity
{
    [CustomEditor(typeof(MultiColumnJaggedTableContent))]
    //[CanEditMultipleObjects]
    public class MultiColumnJaggedTableContentInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            if(!Application.isPlaying) {
                var tableContent = target as MultiColumnJaggedTableContent;
                if(tableContent.TryGetComponent<LayoutGroup>(out var layoutGroup) && layoutGroup.enabled) {
                    EditorGUILayout.HelpBox("Layout Group Component will corrupt table view or cause of glitch. Please disable it before save a prefab/scene or before play.", MessageType.Warning);
                }
                for(int i = 0; i < tableContent.transform.childCount; ++i) {
                    var c = tableContent.transform.GetChild(i);
                    if(c.TryGetComponent<RectTransform>(out var crt)) {
                        if(tableContent.orientaion == TableOrientaion.Horizontal) {
                            if(!Mathf.Approximately(crt.pivot.x, 0.5f) || !Mathf.Approximately(crt.anchorMin.x, crt.anchorMax.x)) {
                                EditorGUILayout.HelpBox("This component assumes that the pivot X of the cells is 0.5 and X is not expandable when orientation is horizontal.\nThere are cells with a pivot X that is not 0.5 or horitontal is expandable.", MessageType.Warning);
                                break;
                            }
                        }
                        else {
                            if(!Mathf.Approximately(crt.pivot.y, 0.5f) || !Mathf.Approximately(crt.anchorMin.y, crt.anchorMax.y)) {
                                EditorGUILayout.HelpBox("This component assumes that the pivot Y of the cells is 0.5 and Y is not expandable when orientation is vertical.\nThere are cells with a pivot Y that is not 0.5 or vertical is expandable.", MessageType.Warning);
                                break;
                            }
                        }
                    }
                }
            }
            // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
            serializedObject.Update();
            // TODO: Draw UI here
            //EditorGUILayout.PropertyField();
            DrawDefaultInspector();

            if(Application.isPlaying) {
                var tableContent = target as MultiColumnJaggedTableContent;
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