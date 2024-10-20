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
        struct Log
        {
            public DateTime dateTime;
            public LogType type;
            public string message;
            public string stackTrace;

            public override readonly bool Equals(object obj) => obj is Log log
                && dateTime == log.dateTime
                && type == log.type
                && message == log.message
                && stackTrace == log.stackTrace;

            public override readonly int GetHashCode() => HashCode.Combine(dateTime, type, message, stackTrace);
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

        void JaggedTableContent.IDataSource.OnPreUpdate()
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

        float JaggedTableContent.IDataSource.CellSize(int index)
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

        void JaggedTableContent.IDataSource.SetUpCell(int index, GameObject cell)
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
            OnLogCleared += ClearPreferredHeightCache;

            var firstChild = transform.GetChild(0);

            minHeight = tableContent.orientaion switch {
                Orientaion.Horizontal => firstChild.GetComponent<RectTransform>().rect.width,
                _ => firstChild.GetComponent<RectTransform>().rect.height
            };

            var text = firstChild.Find("Message").GetComponent<Text>();
            mergin = tableContent.orientaion switch {
                Orientaion.Horizontal => Mathf.Abs(text.rectTransform.sizeDelta.x / 2),
                _ => Mathf.Abs(text.rectTransform.sizeDelta.y / 2)
            };
            fontSize = text.fontSize;
            lineSpacing = text.lineSpacing;
            wrapColumnCount = Mathf.RoundToInt(text.rectTransform.rect.width / (fontSize + 1));
        }

        void OnDestroy()
        {
            OnLogCleared -= ClearPreferredHeightCache;
        }
    }
}
