using System.IO;
using UnityEngine;
using UnityEditor;

namespace CustomUnity
{
    public class EnumContainerTemplate : EditorWindow
    {
        bool shouldClose = false;

        string @namespace = "";
        string enumName = "EnumName";
        string folderPath;

        #region OnGUI()
        void OnGUI()
        {
            // Check if Esc/Return have been pressed
            var e = UnityEngine.Event.current;
            if(e.type == EventType.KeyDown) {
                switch(e.keyCode) {
                // Escape pressed
                case KeyCode.Escape:
                    shouldClose = true;
                    e.Use();
                    break;

                // Enter pressed
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    BuilScriptFile(folderPath, enumName, @namespace);
                    shouldClose = true;
                    e.Use();
                    break;
                }
            }

            if(shouldClose) {  // Close this dialog
                Close();
            }

            // Draw our control
            EditorGUILayout.BeginVertical();

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Input namespace and Enum name");

            EditorGUILayout.Space(8);
            folderPath = EditorGUILayout.TextField("path", folderPath);
            EditorGUILayout.Space(8);
            enumName = EditorGUILayout.TextField("enum name", enumName);
            EditorGUILayout.Space(8);
            @namespace = EditorGUILayout.TextField("namespace", @namespace);
            EditorGUILayout.Space(12);

            // Draw OK / Cancel buttons
            var r = EditorGUILayout.GetControlRect();
            r.width /= 2;
            if(GUI.Button(r, "OK")) {
                if(!shouldClose) BuilScriptFile(folderPath, enumName, @namespace);
                shouldClose = true;
            }

            r.x += r.width;
            if(GUI.Button(r, "Cancel")) {
                shouldClose = true;
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.EndVertical();
        }
        #endregion OnGUI()

        const string menuString = "Assets/Create/C# Enum Container";
        const int priority = 81;

        [MenuItem(menuString, priority = priority)]
        static void NewEnumContainer()
        {
            string folderPath = "Assets";
            foreach(var obj in Selection.objects) {
                var path = AssetDatabase.GetAssetPath(obj);
                if(!string.IsNullOrEmpty(path)) {
                    if(Directory.Exists(path)) {
                        folderPath = path;
                        break;
                    }
                    else if(File.Exists(path)) {
                        folderPath = Path.GetDirectoryName(path);
                        break;
                    }
                }
            }
            var window = CreateInstance<EnumContainerTemplate>();
            window.titleContent = new GUIContent("Create Enum Container");
            window.minSize = new Vector2(220f, 166f);
            window.@namespace = EditorSettings.projectGenerationRootNamespace;
            window.folderPath = folderPath;
            window.ShowModal();
        }

        static void BuilScriptFile(string folderPath, string enumName, string @namespace)
        {
            if(string.IsNullOrWhiteSpace(enumName)) {
                Debug.LogError("Cannot generate a enum container, Enum Name was not specified.");
                return;
            }

            if(File.Exists($"{folderPath}/{enumName}Container.cs")) {
                Debug.LogError($"{enumName}Container.cs is already exists.");
                return;
            }

            var script = string.Format(string.IsNullOrEmpty(@namespace) ? template : namespaceTemplate, enumName, @namespace);

            // finally write out the new editor
            File.WriteAllText($"{folderPath}/{enumName}Container.cs", script);
            AssetDatabase.Refresh();
        }

        #region Templates
        static readonly string template = @"using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class {0}Container : MonoBehaviour
{{
    //[EnumFlags] // commentout for use as flags
    public {0} value;

#if UNITY_EDITOR
    [MenuItem(""GameObject/EnumContainer/"" + nameof({0}), false, 10)]
    static void Create(MenuCommand menuCommand)
    {{
        // Create a custom game object
        GameObject go = new GameObject(nameof({0}), typeof({0}Container));
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, ""Create "" + go.name);
        Selection.activeObject = go;
    }}
#endif
}}";

        static readonly string namespaceTemplate = @"using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace {1}
{{
    public class {0}Container : MonoBehaviour
    {{
        //[EnumFlags] // commentout for use as flags
        public {0} value;

#if UNITY_EDITOR
        [MenuItem(""GameObject/EnumContainer/"" + nameof({0}), false, 10)]
        static void Create(MenuCommand menuCommand)
        {{
            // Create a custom game object
            GameObject go = new GameObject(nameof({0}), typeof({0}Container));
            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, ""Create "" + go.name);
            Selection.activeObject = go;
        }}
#endif
    }}
}}";
        #endregion
    }
}
