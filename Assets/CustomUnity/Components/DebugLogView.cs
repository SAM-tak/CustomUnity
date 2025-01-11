using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
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
        const string kCollapse = nameof(DebugLogView) + ".collapse";

        public Toggle infoToggle;
        public Toggle warningToggle;
        public Toggle errorToggle;
        public Toggle collapseToggle;
        public Text infoCount;
        public Text warningCount;
        public Text errorCount;
        public ScrollRect detailView;
        public Text detailContent;

        DebugLogDataSource _debugLogDataSource;

        void Awake()
        {
            _debugLogDataSource = GetComponentInChildren<DebugLogDataSource>();
        }

        void OnEnable()
        {
            _debugLogDataSource.IncludeInfo = infoToggle.isOn = PlayerPrefs.Get(kIncludeInfo, true);
            _debugLogDataSource.IncludeWarning = warningToggle.isOn = PlayerPrefs.Get(kIncludeWarning, true);
            _debugLogDataSource.IncludeError = errorToggle.isOn = PlayerPrefs.Get(kIncludeError, true);
            _debugLogDataSource.Collapse = collapseToggle.isOn = PlayerPrefs.Get(kCollapse, false);
            detailView.gameObject.SetActive(false);
        }

        void OnDisable()
        {
            PlayerPrefs.Set(kIncludeInfo, infoToggle.isOn);
            PlayerPrefs.Set(kIncludeWarning, warningToggle.isOn);
            PlayerPrefs.Set(kIncludeError, errorToggle.isOn);
            PlayerPrefs.Set(kCollapse, collapseToggle.isOn);
        }

        void Update()
        {
            infoCount.text = $"{Mathf.Min(_debugLogDataSource.InfoCount, 999)}";
            warningCount.text = $"{Mathf.Min(_debugLogDataSource.WarnningCount, 999)}";
            errorCount.text = $"{Mathf.Min(_debugLogDataSource.ErrorCount, 999)}";
        }

        public void ShowDetail(DebugLogLine debugLogLine)
        {
            detailView.gameObject.SetActive(true);
            detailContent.text = $"{debugLogLine.dateTime.text}\n{debugLogLine.message.text}\n{debugLogLine.stackTrace}";
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
                StageUtility.PlaceGameObjectInCurrentStage(go);
                go.UniqueName(prefab.name);
                // Register the creation in the undo system
                Undo.RegisterCreatedObjectUndo(go, $"Create {go.name}");
                Selection.activeObject = go;
            }
        }
#endif
    }
}
