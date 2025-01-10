//using System.Linq;
using UnityEditor;

namespace CustomUnity
{
    [CustomEditor(typeof(SumAllSignalsSlot))]
    //[CanEditMultipleObjects]
    public class SumAllSignalsEventInspector : Editor
    {
        //SerializedProperty[] properties;

        // TODO: find properties we want to work with
        //void OnEnable()
        //{
        //    serializedObject.FindProperty();
        //    properties = ((SumAllSignalsEvent)target).signals.Keys.Select(x => {
        //        var prop = new SerializedProperty();
        //        prop
        //    }).ToArray()
        //}

        public override void OnInspectorGUI()
        {
            // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
            serializedObject.Update();
            // TODO: Draw UI here
            //EditorGUILayout.PropertyField();
            DrawDefaultInspector();
            // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
            serializedObject.ApplyModifiedProperties();
            
            var signals = ((SumAllSignalsSlot)target).signals;
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.LabelField("Connected Signals");
            EditorGUI.indentLevel++;
            if(signals.Count > 0) {
                foreach(var pair in signals) {
                    EditorGUILayout.ObjectField(pair.Value.ToString(), pair.Key, pair.Key.GetType(), true);
                }
            }
            else EditorGUILayout.LabelField("None");
            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();
        }
    }
}