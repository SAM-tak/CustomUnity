using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CustomUnity
{
    /// <summary>
    /// DebugLog View Controller
    /// </summary>
    public class DebugLogView : MonoBehaviour
    {
        const string kIncludeInfo = nameof(DebugLogView) + ".includeinfo";
        const string kIncludeWarning = nameof(DebugLogView) + ".includewaring";
        const string kIncludeError = nameof(DebugLogView) + ".includeerror";

        public Toggle infoToggle;
        public Toggle warningToggle;
        public Toggle errorToggle;

        void OnEnable()
        {
            infoToggle.isOn = PlayerPrefs.Get(kIncludeInfo, true);
            warningToggle.isOn = PlayerPrefs.Get(kIncludeWarning, true);
            errorToggle.isOn = PlayerPrefs.Get(kIncludeError, true);
        }

        void OnDisable()
        {
            PlayerPrefs.Set(kIncludeInfo, infoToggle.isOn);
            PlayerPrefs.Set(kIncludeWarning, warningToggle.isOn);
            PlayerPrefs.Set(kIncludeError, errorToggle.isOn);
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
