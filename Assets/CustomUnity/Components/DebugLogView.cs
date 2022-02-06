using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CustomUnity
{
    public class DebugLogView : MonoBehaviour
    {
        const string prefKeyPrefix = "customunity.debuglogview.";
        const string prefIncludeInfoKey = prefKeyPrefix + "includeinfo";
        const string prefIncludeWarningKey = prefKeyPrefix + "includewaring";
        const string prefIncludeErrorKey = prefKeyPrefix + "includeerror";

        public Toggle infoToggle;
        public Toggle warningToggle;
        public Toggle errorToggle;

        void OnEnable()
        {
            infoToggle.isOn = PlayerPrefs.GetInt(prefIncludeInfoKey, 1) > 0;
            warningToggle.isOn = PlayerPrefs.GetInt(prefIncludeWarningKey, 1) > 0;
            errorToggle.isOn = PlayerPrefs.GetInt(prefIncludeErrorKey, 1) > 0;
        }

        void OnDisable()
        {
            PlayerPrefs.SetInt(prefIncludeInfoKey, infoToggle.isOn ? 1 : 0);
            PlayerPrefs.SetInt(prefIncludeWarningKey, warningToggle.isOn ? 1 : 0);
            PlayerPrefs.SetInt(prefIncludeErrorKey, errorToggle.isOn ? 1 : 0);
        }

#if UNITY_EDITOR
        [MenuItem("GameObject/UI/Log View")]
        static void CreateDebugLogView(MenuCommand menuCommand)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Packages/net.sam-tak.customunity/Prefabs/Debug Log View.prefab");
            if(!prefab) prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CustomUnity/Prefabs/Debug Log View.prefab");
            if(prefab) {
                var parent = (Selection.activeObject ? Selection.activeObject : menuCommand.context) as GameObject;
                var go = Instantiate(prefab, parent ? parent.transform : null);
                go.UniqueName(prefab.name);
                // Register the creation in the undo system
                Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
                Selection.activeObject = go;
            }
        }
#endif
    }
}
