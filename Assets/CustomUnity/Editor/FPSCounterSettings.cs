using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace CustomUnity
{
    [FilePath("UserSettings/CustomUnityFPSCounterSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class FPSCounterSettings : ScriptableSingleton<FPSCounterSettings>
    {
        public bool hideOnRuntime;

        public void Save() => Save(true);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void Initialize()
        {
            if(instance) FPSCounter.hideOnRuntime = instance.hideOnRuntime;
        }
    }

    class FPSCounterSettingsProvider : SettingsProvider
    {
        public FPSCounterSettingsProvider(string path, SettingsScope scopes) : base(path, scopes)
        {
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingProvider()
        {
            return new FPSCounterSettingsProvider("Preferences/CustomUnity/FPSCounter Settings", SettingsScope.User);
        }

        private UnityEditor.Editor _editor;

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            var settings = FPSCounterSettings.instance;
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
                FPSCounterSettings.instance.Save();
                FPSCounter.hideOnRuntime = FPSCounterSettings.instance.hideOnRuntime;
            }
        }
    }
}
