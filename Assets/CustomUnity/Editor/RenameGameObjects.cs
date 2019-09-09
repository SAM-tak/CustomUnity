namespace CustomUnity
{
    using UnityEngine;
    using UnityEditor;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class RenameGameObjects : EditorWindow
    {
        public string find;
        public string replcace;
        public bool useRegularExpression = false;
        public bool recursiveForChildObjects = false;

        void OnGUI()
        {
            find = EditorGUILayout.TextField("Find", find);
            replcace = EditorGUILayout.TextField("Replace", replcace);
            useRegularExpression = EditorGUILayout.Toggle("Use regular expression", useRegularExpression);
            recursiveForChildObjects = EditorGUILayout.Toggle("Recursive for child objects", recursiveForChildObjects);

            EditorGUI.BeginDisabledGroup(Selection.gameObjects.Length == 0);

            if(GUILayout.Button("Rename")) {
                var gameObjects = Selection.gameObjects.Where(i => !AssetDatabase.IsMainAsset(i)).ToArray();
                Undo.RecordObjects(gameObjects, "Rename Objects");
                if(useRegularExpression) {
                    var regex = new Regex(find);
                    foreach(var i in gameObjects) {
                        i.name = regex.Replace(i.name, replcace);
                        if(recursiveForChildObjects) {
                            foreach(var j in i.transform.EnumChildrenRecursive()) {
                                j.name = regex.Replace(j.name, replcace);
                            }
                        }
                    }
                }
                else {
                    foreach(var i in gameObjects) {
                        i.name = i.name.Replace(find, replcace);
                        if(recursiveForChildObjects) {
                            foreach(var j in i.transform.EnumChildrenRecursive()) {
                                j.name = j.name.Replace(find, replcace);
                            }
                        }
                    }
                }
            }

            EditorGUI.EndDisabledGroup();
        }

        [MenuItem("Window/Rename GameObjects")]
        static void Open()
        {
            GetWindow<RenameGameObjects>("Rename GameObjects");
        }
    }
}