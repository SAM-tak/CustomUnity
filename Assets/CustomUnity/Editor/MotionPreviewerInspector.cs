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
                bool dirty = false;
                if(motionPreview.index > motionPreview.clips.Length - 1) {
                    motionPreview.index = 0;
                    dirty = true;
                }
                var index = EditorGUILayout.Popup("Clip", motionPreview.index, motionPreview.ClipNames);
                if(index != motionPreview.index) {
                    motionPreview.index = index;
                    dirty = true;
                }
                if(motionPreview.index < motionPreview.clips.Length && motionPreview.clips[motionPreview.index]) {
                    var frame = EditorGUILayout.IntSlider("Frame", motionPreview.frame, 0, Mathf.FloorToInt(motionPreview.clips[motionPreview.index].length * 60));
                    if(frame != motionPreview.frame) {
                        motionPreview.frame = frame;
                        dirty = true;
                    }
                }
                if(dirty) EditorUtility.SetDirty(target);
            }
        }
    }
}
