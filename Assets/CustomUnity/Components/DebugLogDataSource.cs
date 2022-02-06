using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CustomUnity
{
    [RequireComponent(typeof(JaggedTableContent))]
    public class DebugLogDataSource : MonoBehaviour, JaggedTableContent.IDataSource
    {
        struct Log
        {
            public DateTime dateTime;
            public LogType type;
            public string message;
            public string stackTrace;

            public override bool Equals(object obj) => obj is Log log
                && dateTime == log.dateTime
                && type == log.type
                && message == log.message
                && stackTrace == log.stackTrace;

            public override int GetHashCode() => HashCode.Combine(dateTime, type, message, stackTrace);
        }

        class LogData
        {
            public HashSet<string> texts;
            public List<Log> logs;

            public void Add(string message, string stackTrace, LogType type)
            {
                if(texts.TryGetValue(message, out var existing)) message = existing;
                else texts.Add(message);
                if(texts.TryGetValue(stackTrace, out existing)) stackTrace = existing;
                else texts.Add(stackTrace);
                logs.Add(new Log { dateTime = DateTime.UtcNow, type = type, message = message, stackTrace = stackTrace });
            }

            public void Clear()
            {
                logs.Clear();
                texts.Clear();
            }
        }

        static readonly Lazy<LogData> logRecords = new(() => new() { texts = new(256), logs = new(256) });
        static event Action OnLogCleared;
        static bool isStarted;

        [Conditional("DEVELOPMENT_BUILD")]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void StartLogging()
        {
            lock(logRecords) {
                if(!isStarted) {
                    isStarted = true;
                    Application.logMessageReceivedThreaded += LogCallback;
                    Application.quitting += StopLogging;
                }
            }
        }

        public static void StopLogging()
        {
            lock(logRecords) {
                Application.logMessageReceivedThreaded -= LogCallback;
                Application.quitting -= StopLogging;
                isStarted = false;
            }
        }

        public static void ClearLog()
        {
            if(logRecords.IsValueCreated) {
                logRecords.Value.Clear();
                OnLogCleared();
            }
        }

        static void LogCallback(string message, string stackTrace, LogType type)
        {
            logRecords.Value.Add(message, stackTrace, type);
        }

        public bool IncludeInfo { get; set; } = true;
        public bool IncludeWarning { get; set; } = true;
        public bool IncludeError { get; set; } = true;

        bool Includes(Log log) => (IncludeInfo && log.type == LogType.Log)
            || (IncludeWarning && log.type == LogType.Warning)
            || (IncludeError && (log.type == LogType.Error || log.type == LogType.Assert || log.type == LogType.Exception));

        public int TotalCount => logRecords.IsValueCreated ? logRecords.Value.logs.Count(i => Includes(i)) : 0;

        int prevTotalCount = 0;

        JaggedTableContent tableContent;

        void OnPreUpdate()
        {
            if(prevTotalCount != TotalCount) {
                tableContent.Refresh();
                prevTotalCount = TotalCount;
            }
        }

        float EstimateHeight(int lineCount)
        {
            return Mathf.Max(minHeight, lineCount * (fontSize + 2) + Mathf.Max(0, lineCount - 1) * lineSpacing + mergin * 2f);
        }

        readonly Dictionary<int, float> preferredHeightCache = new(256);

        public void ClearPreferredHeightCache()
        {
            preferredHeightCache.Clear();
        }

        public float CellSize(int index)
        {
            if(index < 0 || logRecords.Value.logs.Count(i => Includes(i)) <= index) return minHeight;

            var log = logRecords.Value.logs.Where(i => Includes(i)).ElementAt(index);

            var trueIndex = logRecords.Value.logs.IndexOf(log);

            if(preferredHeightCache.ContainsKey(trueIndex)) return preferredHeightCache[trueIndex];

            var cell = tableContent.GetActiveCell(index);
            if(cell && cell.activeInHierarchy) {
                var text = cell.transform.Find("Message").GetComponent<Text>();
                if(text.text.Equals(log.message)) {
                    var preferredHeight = Mathf.Max(minHeight, text.preferredHeight + mergin * 2f);
                    preferredHeightCache[trueIndex] = preferredHeight;
                    tableContent.NeedsRelayout();
                    return preferredHeight;
                }
            }

            var lineCount = wrapColumnCount > 0 ? log.message.LineCount(wrapColumnCount) : log.message.LineCount();
            //StopLogging();
            //LogInfo($"lineCount = {lineCount} fontSize = {fontSize} lineSpacing = {lineSpacing} mergin = {mergin} height = {EstimateHeight(lineCount)}");
            //StartLogging();
            return EstimateHeight(lineCount);
        }

        public void SetUpCell(int index, GameObject cell)
        {
            var data = logRecords.Value.logs.Where(i => Includes(i)).ElementAt(index);
            cell.transform.Find("DateTime").GetComponent<Text>().text = data.dateTime.ToString("u");
            switch(data.type) {
            case LogType.Log:
                cell.transform.Find("Type/Info").gameObject.SetActive(true);
                cell.transform.Find("Type/Warning").gameObject.SetActive(false);
                cell.transform.Find("Type/Error").gameObject.SetActive(false);
                break;
            case LogType.Warning:
                cell.transform.Find("Type/Info").gameObject.SetActive(false);
                cell.transform.Find("Type/Warning").gameObject.SetActive(true);
                cell.transform.Find("Type/Error").gameObject.SetActive(false);
                break;
            case LogType.Error:
            case LogType.Exception:
            case LogType.Assert:
                cell.transform.Find("Type/Info").gameObject.SetActive(false);
                cell.transform.Find("Type/Warning").gameObject.SetActive(false);
                cell.transform.Find("Type/Error").gameObject.SetActive(true);
                break;
            }
            cell.transform.Find("Message").GetComponent<Text>().text = data.message;
            cell.transform.Find("Background").gameObject.SetActive(index % 2 != 0);
            cell.transform.Find("AltBackground").gameObject.SetActive(index % 2 == 0);
        }

        void Awake()
        {
            tableContent = GetComponent<JaggedTableContent>();
            tableContent.DataSource = this;
            tableContent.OnPreUpdate += OnPreUpdate;
            StartLogging();
            OnLogCleared += ClearPreferredHeightCache;
        }

        void OnDestroy()
        {
            if(tableContent) tableContent.OnPreUpdate -= OnPreUpdate;
            OnLogCleared -= ClearPreferredHeightCache;
        }

        public float minHeight = 34f;
        public int wrapColumnCount = 0;

        int fontSize;
        float lineSpacing;
        float mergin;

        void Start()
        {
            var text = transform.GetChild(0).Find("Message").GetComponent<Text>();
            mergin  = Mathf.Abs(text.rectTransform.sizeDelta.y / 2);
            fontSize = text.fontSize;
            lineSpacing = text.lineSpacing;
        }

#if UNITY_EDITOR
        [MenuItem("GameObject/UI/Log View")]
        static void CreateLogView(MenuCommand menuCommand)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Packages/CustomUnity/Prefabs/Log View.prefab");
            if(!prefab) prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CustomUnity/Prefabs/Log View.prefab");
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
