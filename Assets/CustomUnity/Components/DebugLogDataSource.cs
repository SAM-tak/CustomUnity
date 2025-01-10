using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

namespace CustomUnity
{
    /// <summary>
    /// DebugLog ViewModel
    /// </summary>
    [RequireComponent(typeof(JaggedTableContent))]
    public class DebugLogDataSource : MonoBehaviour, JaggedTableContent.IDataSource
    {
        public struct Log
        {
            public DateTime dateTime;
            public LogType type;
            public string message;
            public string stackTrace;
            public int count;

            public override readonly bool Equals(object obj) => obj is Log log
                && dateTime == log.dateTime
                && type == log.type
                && message == log.message
                && stackTrace == log.stackTrace
                && count == log.count;

            public override readonly int GetHashCode() => HashCode.Combine(dateTime, type, message, stackTrace, count);
        }

        class LogData
        {
            public HashSet<string> texts;
            public Dictionary<(string, string), int> firstUseLogIndex;
            public List<Log> logs;
            public List<Log> filtered;

            public void Add(string message, string stackTrace, LogType type)
            {
                int count = 0;
                if(texts.TryGetValue(message, out var existing)) {
                    message = existing;
                }
                else {
                    texts.Add(message);
                    count = 1;
                }
                if(texts.TryGetValue(stackTrace, out existing)) {
                    stackTrace = existing;
                }
                else {
                    texts.Add(stackTrace);
                    count = 1;
                }
                if(count > 0) {
                    firstUseLogIndex.TryAdd((message, stackTrace), logs.Count);
                }
                else if(firstUseLogIndex.TryGetValue((message, stackTrace), out var index)) {
                    var log = logs[index];
                    log.count++;
                    logs[index] = log;
                }
                logs.Add(new Log { dateTime = DateTime.UtcNow, type = type, message = message, stackTrace = stackTrace, count = count });
            }

            public void Clear()
            {
                texts.Clear();
                firstUseLogIndex.Clear();
                logs.Clear();
                filtered.Clear();
            }
        }

        static readonly Lazy<LogData> logRecords = new(() => new() { texts = new(256), firstUseLogIndex = new(), logs = new(256), filtered = new(256) });
        static event Action OnLogAdded;
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
            OnLogAdded?.Invoke();
        }

        void SetDirty()
        {
            _dirty = true;
        }

        bool _dirty = true;
        bool _includeInfo = true;
        public bool IncludeInfo { get => _includeInfo; set { _dirty = true; _includeInfo = value; } }
        bool _includeWarning = true;
        public bool IncludeWarning { get => _includeWarning; set { _dirty = true; _includeWarning = value; } }
        bool _includeError = true;
        public bool IncludeError { get => _includeError; set { _dirty = true; _includeError = value; } }
        bool _collapse = false;
        public bool Collapse { get => _collapse; set { _dirty = true; _collapse = value; } }
        string _filterString = string.Empty;
        public string FilterString { get => _filterString; set { _dirty = true; _filterString = value; } }

        public int LogCount => logRecords.IsValueCreated ? logRecords.Value.logs.Count : 0;

        bool Includes(Log log) => (!Collapse || log.count > 0)
            && (string.IsNullOrEmpty(FilterString) || log.message.Contains(FilterString))
            && ((IncludeInfo && log.type == LogType.Log)
             || (IncludeWarning && log.type == LogType.Warning)
             || (IncludeError && (log.type == LogType.Error || log.type == LogType.Assert || log.type == LogType.Exception)));

        int _totalCount = 0;
        public int TotalCount {
            get {
                if(_dirty && logRecords.IsValueCreated) {
                    logRecords.Value.filtered.Clear();
                    logRecords.Value.filtered.AddRange(logRecords.Value.logs.Where(i => Includes(i)));
                    _totalCount = logRecords.Value.filtered.Count;
                    InfoCount = logRecords.Value.logs.Count(x => x.type == LogType.Log);
                    WarnningCount = logRecords.Value.logs.Count(x => x.type == LogType.Warning);
                    ErrorCount = logRecords.Value.logs.Count(x => x.type == LogType.Error || x.type == LogType.Assert || x.type == LogType.Exception);
                    _dirty = false;
                }
                return _totalCount;
            }
        }
        public int InfoCount { get; private set; }
        public int WarnningCount { get; private set; }
        public int ErrorCount { get; private set; }

        int prevLogCount = 0;
        int prevTotalCount = 0;

        JaggedTableContent tableContent;

        void JaggedTableContent.IDataSource.OnPreUpdate()
        {
            bool needsRefresh = false;
            if(prevLogCount != LogCount) {
                prevLogCount = LogCount;
                needsRefresh = true;
            }
            if(prevTotalCount != TotalCount) {
                prevTotalCount = TotalCount;
                needsRefresh = true;
            }
            if(needsRefresh) {
                tableContent.Refresh();
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

        float JaggedTableContent.IDataSource.CellSize(int index)
        {
            if(index < 0 || logRecords.Value.filtered.Count <= index) return minHeight;

            var log = logRecords.Value.filtered.ElementAt(index);

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

        void JaggedTableContent.IDataSource.SetUpCell(int index, GameObject cell)
        {
            var data = logRecords.Value.filtered.ElementAt(index);
            if(cell.TryGetComponent<DebugLogLine>(out var logLine)) {
                logLine.SetUp(data, Collapse, index % 2 == 0);
            }
        }

        public void CellDeactivated(GameObject cell)
        {
        }

        float minHeight;
        int wrapColumnCount;
        int fontSize;
        float lineSpacing;
        float mergin;

        void Awake()
        {
            tableContent = GetComponent<JaggedTableContent>();
            StartLogging();
            OnLogAdded += SetDirty;
            OnLogCleared += SetDirty;
            OnLogCleared += ClearPreferredHeightCache;

            var firstChild = transform.GetChild(0);

            minHeight = tableContent.orientaion  == Orientaion.Horizontal
                ? firstChild.GetComponent<RectTransform>().rect.width
                : firstChild.GetComponent<RectTransform>().rect.height;

            var text = firstChild.Find("Message").GetComponent<Text>();
            mergin = tableContent.orientaion == Orientaion.Horizontal
                ? Mathf.Abs(text.rectTransform.sizeDelta.x / 2)
                : Mathf.Abs(text.rectTransform.sizeDelta.y / 2);
            fontSize = text.fontSize;
            lineSpacing = text.lineSpacing;
            wrapColumnCount = Mathf.RoundToInt(text.rectTransform.rect.width / (fontSize + 1));
        }

        void OnDestroy()
        {
            OnLogAdded -= SetDirty;
            OnLogCleared -= SetDirty;
            OnLogCleared -= ClearPreferredHeightCache;
        }
    }
}
