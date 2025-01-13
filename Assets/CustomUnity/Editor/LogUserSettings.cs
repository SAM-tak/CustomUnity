using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;
using Object = UnityEngine.Object;

namespace CustomUnity
{
    [FilePath("UserSettings/" + nameof(CustomUnity) + "LogSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class LogUserSettings : ScriptableSingleton<LogUserSettings>
    {
        [Serializable]
        public class FilterDefination
        {
            public bool disabled;
            public bool ignoreCase;
            [TextArea]
            public string includePatterns;
            [TextArea]
            public string excludePatterns;

            Regex[] includeRegexes;
            protected Regex[] IncludeRegexes {
                get {
                    if(includeRegexes is null) {
                        if(string.IsNullOrEmpty(includePatterns)) return null;
                        includeRegexes = ToRegexes(includePatterns, ToRegex);
                    }
                    else if(string.IsNullOrEmpty(includePatterns)) includeRegexes = null;
                    return includeRegexes;
                }
            }

            Regex[] excludeRegexes;
            protected Regex[] ExcludeRegexes {
                get {
                    if(excludeRegexes is null) {
                        if(string.IsNullOrEmpty(excludePatterns)) return null;
                        excludeRegexes = ToRegexes(excludePatterns, ToRegex);
                    }
                    else if(string.IsNullOrEmpty(excludePatterns)) excludeRegexes = null;
                    return excludeRegexes;
                }
            }

            public bool IsValid => !disabled && IncludeRegexes != null || ExcludeRegexes != null;

            public bool PassFilter(string testee)
            {
                if(disabled) return true;
                if(IncludeRegexes != null && !IncludeRegexes.Any(i => i.IsMatch(testee))) return false;
                if(ExcludeRegexes != null && ExcludeRegexes.Any(i => i.IsMatch(testee))) return false;
                return true;
            }

            protected virtual Regex ToRegex(string line) => new(line, (ignoreCase ? RegexOptions.IgnoreCase : 0) | RegexOptions.Singleline);

            protected static Regex[] ToRegexes(string text, Func<string, Regex> regex)
            {
                using(var stringReader = new StringReader(text)) {
                    List<Regex> ret = null;
                    for(var line = stringReader.ReadLine(); line != null; line = stringReader.ReadLine()) {
                        if(!string.IsNullOrEmpty(line)) {
                            if(ret == null) ret = new List<Regex>();
                            ret.Add(regex(line));
                        }
                    }
                    if(ret != null && ret.Count > 0) return ret.ToArray();
                }
                return null;
            }
        }

        [Serializable]
        public class GlobFilterDefination : FilterDefination
        {
            protected override Regex ToRegex(string line)
            {
                return new Regex(Regex.Escape(line).Replace(@"\*", ".*").Replace(@"\?", "."), (ignoreCase ? RegexOptions.IgnoreCase : 0) | RegexOptions.Singleline);
            }
        }

        static bool IsValid(FilterDefination[] filters) => filters != null && filters.Length > 0 && filters.Any(i => i.IsValid);

        public GlobFilterDefination[] callerFilters;
        public FilterDefination[] messageFilters;
        public FilterDefination[] objectFilters;

        public void Save() => Save(true);

        bool FilterByCaller(StackFrame callerFrame)
        {
            var fileName = callerFrame.GetFileName();
            if(string.IsNullOrEmpty(fileName)) return true;
            if(IsValid(callerFilters)) return callerFilters.Any(i => i.PassFilter($"{callerFrame.GetFileName()}:{callerFrame.GetFileLineNumber()}"));
            return true;
        }

        bool FilterByMessage(string message)
        {
            if(string.IsNullOrEmpty(message)) return true;
            if(IsValid(messageFilters)) return messageFilters.Any(i => i.PassFilter(message));
            return true;
        }

        bool FilterByObject(Object @object)
        {
            if(@object is null) return true;
            if(IsValid(objectFilters)) return objectFilters.Any(i => i.PassFilter($"{@object.GetType().FullName} {@object.name}"));
            return true;
        }

        public void SetUpLogFilterDelegate()
        {
            Log.FilterByCaller -= FilterByCaller;
            if(IsValid(callerFilters)) Log.FilterByCaller += FilterByCaller;
            Log.FilterByMessage -= FilterByMessage;
            if(IsValid(messageFilters)) Log.FilterByMessage += FilterByMessage;
            Log.FilterByObject -= FilterByObject;
            if(IsValid(objectFilters)) Log.FilterByObject += FilterByObject;
            Log.OnFilterChanged();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void Initialize()
        {
            instance?.SetUpLogFilterDelegate();
        }
    }

    class LogUserSettingsProvider : SettingsProvider
    {
        public LogUserSettingsProvider(string path, SettingsScope scopes) : base(path, scopes)
        {
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingProvider()
        {
            return new LogUserSettingsProvider($"Preferences/{nameof(CustomUnity)}/Log Settings", SettingsScope.User);
        }

        private UnityEditor.Editor _editor;

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            var settings = LogUserSettings.instance;
            settings.SetUpLogFilterDelegate();
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
                LogUserSettings.instance.Save();
                LogUserSettings.instance.SetUpLogFilterDelegate();
            }
        }
    }
}
