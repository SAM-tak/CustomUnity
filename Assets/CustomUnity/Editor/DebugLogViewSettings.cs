using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace CustomUnity
{
    [FilePath("ProjectSettings/" + nameof(CustomUnity) + "DebugLogViewSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class DebugLogViewSettings : ScriptableSingleton<DebugLogViewSettings>
    {
        [Tooltip("Default is 'u'")]
        public string dateTimeFormatString = "u";
        public bool universalTime = false;

        public void Save() => Save(true);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void Initialize()
        {
            if(instance) {
                DebugLogLine.dateTimeFormatString = instance.dateTimeFormatString;
                DebugLogLine.universalTime = instance.universalTime;
            }
        }
    }

    class DebugLogViewSettingsProvider : SettingsProvider
    {
        public DebugLogViewSettingsProvider(string path, SettingsScope scopes) : base(path, scopes)
        {
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingProvider()
        {
            return new DebugLogViewSettingsProvider($"Project/{nameof(CustomUnity)}/Debug Log View Settings", SettingsScope.Project);
        }

        private UnityEditor.Editor _editor;

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            var settings = DebugLogViewSettings.instance;
            // set to editable ScriptableSingleton
            settings.hideFlags = HideFlags.HideAndDontSave & ~HideFlags.NotEditable;
            // create default inspector
            UnityEditor.Editor.CreateCachedEditor(settings, null, ref _editor);
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUI.BeginChangeCheck();
            // 設定ファイルの標準のインスペクターを表示
            if(_editor) _editor.OnInspectorGUI();

            if(EditorGUI.EndChangeCheck()) {
                // 差分があったら保存
                DebugLogViewSettings.instance.Save();
                DebugLogLine.dateTimeFormatString = DebugLogViewSettings.instance.dateTimeFormatString;
                DebugLogLine.universalTime = DebugLogViewSettings.instance.universalTime;
            }
        }
    }
}
