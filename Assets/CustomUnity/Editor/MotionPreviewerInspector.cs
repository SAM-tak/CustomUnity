using UnityEngine;
using UnityEditor;

namespace CustomUnity
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MotionPreviewer))]
    public class MotionPreviewerInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var motionPreview = target as MotionPreviewer;
            if(motionPreview.clips != null && motionPreview.clips.Length > 0) {
                if(motionPreview.index > motionPreview.clips.Length - 1) motionPreview.index = 0;
                motionPreview.index = EditorGUILayout.Popup("Clip", motionPreview.index, motionPreview.ClipNames);
                if(motionPreview.index < motionPreview.clips.Length && motionPreview.clips[motionPreview.index]) {
                    motionPreview.frame = EditorGUILayout.IntSlider("Frame", motionPreview.frame, 0, Mathf.FloorToInt(motionPreview.clips[motionPreview.index].length * 60));
                }
            }
        }
    }
}
