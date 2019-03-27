using UnityEngine;
using UnityEditor;
using System.IO;

namespace CustomUnity
{
    public static class CustomInspectorTemplate
    {
        const string menuString = "Assets/Create/C# Inspector Script";
        const int priority = 81;

        [MenuItem(menuString, priority = priority)]
        static void NewEditorScript()
        {
            foreach(var script in Selection.objects) BuildEditorFile(script);
            AssetDatabase.Refresh();
        }

        [MenuItem(menuString, priority = priority, validate = true)]
        static bool ValidateNewEditorScript()
        {
            foreach(var script in Selection.objects) {
                if(script.GetType() != typeof(MonoScript)) return false;

                var path = AssetDatabase.GetAssetPath(script);
                if(!path.EndsWith(".cs") || path.Contains("Editor")) return false;
            }
            return Selection.objects != null && Selection.objects.Length > 0;
        }

        static void BuildEditorFile(Object obj)
        {
            var monoScript = obj as MonoScript;
            if(monoScript == null) {
                Debug.LogError("Cannot generate a custom inspector, Selected script was not a MonoBehaviour.");
                return;
            }

            var assetPath = AssetDatabase.GetAssetPath(obj);
            
            // make sure a editor folder exists for us to put this script into...       
            var editorFolder = Path.GetDirectoryName(assetPath) + "/Editor";

            if(!Directory.Exists(editorFolder)) {
                Directory.CreateDirectory(editorFolder);
            }

            var filename = Path.GetFileNameWithoutExtension(assetPath);

            if(File.Exists(editorFolder + "/" + filename + "Inspector.cs")) {
                Debug.LogErrorFormat("{0}Inspector.cs is already exists.", filename);
                return;
            }

            var scriptNamespace = monoScript.GetClass().Namespace;
            var script = string.Format(string.IsNullOrEmpty(scriptNamespace) ? template : namespaceTemplate, filename, scriptNamespace);

            // finally write out the new editor
            File.WriteAllText(editorFolder + "/" + filename + "Inspector.cs", script);
        }

        #region Templates
        static readonly string template = @"using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof({0}))]
//[CanEditMultipleObjects]
public class {0}Inspector : Editor
{{
    void OnEnable()
    {{
        // TODO: find properties we want to work with
        //serializedObject.FindProperty();
    }}

    public override void OnInspectorGUI()
    {{
        // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
        serializedObject.Update();
        // TODO: Draw UI here
        //EditorGUILayout.PropertyField();
        DrawDefaultInspector();
        // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
        serializedObject.ApplyModifiedProperties();
    }}
}}";

        static readonly string namespaceTemplate = @"using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace {1}
{{
    [CustomEditor(typeof({0}))]
    //[CanEditMultipleObjects]
    public class {0}Inspector : Editor
    {{
        void OnEnable()
        {{
            // TODO: find properties we want to work with
            //serializedObject.FindProperty();
        }}

        public override void OnInspectorGUI()
        {{
            // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
            serializedObject.Update();
            // TODO: Draw UI here
            //EditorGUILayout.PropertyField();
            DrawDefaultInspector();
            // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
            serializedObject.ApplyModifiedProperties();
        }}
    }}
}}";
        #endregion
    }
}
