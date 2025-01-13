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
    [RequireComponent(typeof(TableContent))]
    public class DebugLogDataSource : MonoBehaviour, TableContent.IDataSource
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

        static readonly Lazy<LogData> _logData = new(() => new() { texts = new(256), firstUseLogIndex = new(), logs = new(256), filtered = new(256) });
        static event Action OnLogAdded;
        static event Action OnLogCleared;
        static bool _isStarted;

        [Conditional("DEVELOPMENT_BUILD")]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void StartLogging()
        {
            lock(_logData) {
                if(!_isStarted) {
                    _isStarted = true;
                    Application.logMessageReceivedThreaded += LogCallback;
                    Application.quitting += StopLogging;
                }
            }
        }

        public static void StopLogging()
        {
            lock(_logData) {
                Application.logMessageReceivedThreaded -= LogCallback;
                Application.quitting -= StopLogging;
                _isStarted = false;
            }
        }

        public static void ClearLog()
        {
            if(_logData.IsValueCreated) {
                _logData.Value.Clear();
                OnLogCleared();
            }
        }

        static void LogCallback(string message, string stackTrace, LogType type)
        {
            _logData.Value.Add(message, stackTrace, type);
            OnLogAdded?.Invoke();
        }

        void SetDirty()
        {
            _dirty = true;
        }

        bool _dirty = true;
        bool _includeInfo = true;
        public bool IncludeInfo { get => _includeInfo; set { if(_includeInfo != value) _dirty = true; _includeInfo = value; } }
        bool _includeWarning = true;
        public bool IncludeWarning { get => _includeWarning; set { if(_includeWarning != value) _dirty = true; _includeWarning = value; } }
        bool _includeError = true;
        public bool IncludeError { get => _includeError; set { if(_includeError != value) _dirty = true; _includeError = value; } }
        bool _collapse = false;
        public bool Collapse { get => _collapse; set { if(_collapse != value) _dirty = true; _collapse = value; } }
        string _filterString = string.Empty;
        public string FilterString { get => _filterString; set { if(_filterString != value) _dirty = true; _filterString = value; } }

        public int LogCount => _logData.IsValueCreated ? _logData.Value.logs.Count : 0;

        bool Includes(Log log) => (!Collapse || log.count > 0)
            && ((IncludeInfo && log.type == LogType.Log)
             || (IncludeWarning && log.type == LogType.Warning)
             || (IncludeError && (log.type == LogType.Error || log.type == LogType.Assert || log.type == LogType.Exception)))
            && (string.IsNullOrEmpty(FilterString) || log.message.Contains(FilterString));

        int _totalCount = 0;
        public int TotalCount {
            get {
                if(_dirty && _logData.IsValueCreated) {
                    _logData.Value.filtered.Clear();
                    _logData.Value.filtered.AddRange(_logData.Value.logs.Where(i => Includes(i)));
                    _totalCount = _logData.Value.filtered.Count;
                    InfoCount = _logData.Value.logs.Count(x => x.type == LogType.Log);
                    WarnningCount = _logData.Value.logs.Count(x => x.type == LogType.Warning);
                    ErrorCount = _logData.Value.logs.Count(x => x.type == LogType.Error || x.type == LogType.Assert || x.type == LogType.Exception);
                    _dirty = false;
                }
                return _totalCount;
            }
        }
        public int InfoCount { get; private set; }
        public int WarnningCount { get; private set; }
        public int ErrorCount { get; private set; }

        int _prevLogCount = 0;
        int _prevTotalCount = 0;

        TableContent _tableContent;

        public void OnPreUpdate()
        {
            bool needsRefresh = false;
            if(_prevLogCount != LogCount) {
                _prevLogCount = LogCount;
                needsRefresh = true;
            }
            if(_prevTotalCount != TotalCount) {
                _prevTotalCount = TotalCount;
                needsRefresh = true;
            }
            if(needsRefresh) {
                _tableContent.Refresh();
            }
        }

        // float EstimateHeight(int lineCount)
        // {
        //     return Mathf.Max(_minHeight, lineCount * (_fontSize + 2) + Mathf.Max(0, lineCount - 1) * _lineSpacing + _mergin * 2f);
        // }

        // readonly Dictionary<int, float> _preferredHeightCache = new(256);

        // public void ClearPreferredHeightCache()
        // {
        //     _preferredHeightCache.Clear();
        // }

        // public float CellSize(int index)
        // {
        //     if(index < 0 || _logData.Value.filtered.Count <= index) return _minHeight;

        //     var log = _logData.Value.filtered.ElementAt(index);

        //     var trueIndex = _logData.Value.logs.IndexOf(log);

        //     if(_preferredHeightCache.ContainsKey(trueIndex)) return _preferredHeightCache[trueIndex];

        //     var cell = _tableContent.GetActiveCell(index);
        //     if(cell && cell.activeInHierarchy) {
        //         var text = cell.transform.Find("Message").GetComponent<Text>();
        //         if(text.text.Equals(log.message)) {
        //             var preferredHeight = Mathf.Max(_minHeight, text.preferredHeight + _mergin * 2f);
        //             _preferredHeightCache[trueIndex] = preferredHeight;
        //             _tableContent.NeedsRelayout();
        //             return preferredHeight;
        //         }
        //     }

        //     var lineCount = _wrapColumnCount > 0 ? log.message.LineCount(_wrapColumnCount) : log.message.LineCount();
        //     //StopLogging();
        //     //LogInfo($"lineCount = {lineCount} fontSize = {fontSize} lineSpacing = {lineSpacing} mergin = {mergin} height = {EstimateHeight(lineCount)}");
        //     //StartLogging();
        //     return EstimateHeight(lineCount);
        // }

        public void SetUpCell(int index, GameObject cell)
        {
            var data = _logData.Value.filtered.ElementAt(index);
            if(cell.TryGetComponent<DebugLogLine>(out var logLine)) {
                logLine.SetUp(data, Collapse, index % 2 == 0);
            }
        }

        public void CellDeactivated(GameObject cell)
        {
        }

        float _minHeight;
        int _wrapColumnCount;
        int _fontSize;
        float _lineSpacing;
        float _mergin;

        void Awake()
        {
            _tableContent = GetComponent<TableContent>();
            StartLogging();
            OnLogAdded += SetDirty;
            OnLogCleared += SetDirty;
            // OnLogCleared += ClearPreferredHeightCache;

            var firstChild = transform.GetChild(0);

            _minHeight = _tableContent.orientaion switch {
                TableOrientaion.Horizontal => firstChild.GetComponent<RectTransform>().rect.width,
                TableOrientaion.Vertical => firstChild.GetComponent<RectTransform>().rect.height,
                _ => 0f
            };

            var text = firstChild.Find("Message").GetComponent<Text>();
            _mergin = _tableContent.orientaion switch {
                TableOrientaion.Horizontal => Mathf.Abs(text.rectTransform.sizeDelta.x / 2),
                TableOrientaion.Vertical => Mathf.Abs(text.rectTransform.sizeDelta.y / 2),
                _ => 0f
            };
            _fontSize = text.fontSize;
            _lineSpacing = text.lineSpacing;
            _wrapColumnCount = Mathf.RoundToInt(text.rectTransform.rect.width / (_fontSize + 1));
        }

        void OnDestroy()
        {
            OnLogAdded -= SetDirty;
            OnLogCleared -= SetDirty;
            // OnLogCleared -= ClearPreferredHeightCache;
        }
    }
}
