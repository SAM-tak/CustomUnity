using UnityEngine;
using UnityEditor;

namespace CustomUnity
{
    public static class CustomEditorUI
    {
        public static bool Foldout(bool expanded, string title)
        {
            var style = new GUIStyle("ShurikenModuleTitle") {
                font = new GUIStyle(EditorStyles.label).font,
                border = new RectOffset(15, 7, 4, 4),
                fixedHeight = 22,
                contentOffset = new Vector2(20f, -2f)
            };

            var rect = GUILayoutUtility.GetRect(16f, 22f, style);
            GUI.Box(rect, title, style);

            var e = UnityEngine.Event.current;

            var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
            if(e.type == EventType.Repaint) {
                EditorStyles.foldout.Draw(toggleRect, false, false, expanded, false);
            }

            if(e.type == EventType.MouseDown && rect.Contains(e.mousePosition)) {
                expanded = !expanded;
                e.Use();
            }

            return expanded;
        }
    }
}