using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CustomUnity
{
    [RequireComponent(typeof(JaggedTableContent))]
    public class LogDataSource : MonoBehaviour, JaggedTableContent.IDataSource
    {
        struct Log
        {
            public DateTime dateTime;
            public string message;
            public string stackTrace;
            public LogType type;

            public float EstimatedHeight()
            {
                return 80f;
            }
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
                logs.Add(new Log { dateTime = DateTime.UtcNow, message = message, stackTrace = stackTrace, type = type });
            }

            public void Clear()
            {
                logs.Clear();
                texts.Clear();
            }
        }

        static readonly Lazy<LogData> logRecords = new(() => new() { texts = new(256), logs = new(256) });

        public static void StartLogging()
        {
            Application.logMessageReceivedThreaded -= LogCallback;
            Application.logMessageReceivedThreaded += LogCallback;
        }

        public static void StopLogging()
        {
            Application.logMessageReceivedThreaded -= LogCallback;
        }

        public static void ClearLog()
        {
            if(logRecords.IsValueCreated) logRecords.Value.Clear();
        }

        static void LogCallback(string message, string stackTrace, LogType type)
        {
            logRecords.Value.Add(message, stackTrace, type);
        }

        public int TotalCount => logRecords.IsValueCreated ? logRecords.Value.logs.Count : 0;

        int prevTotalCount = 0;

        JaggedTableContent tableContent;

        public void OnPreUpdate()
        {
            if(prevTotalCount != TotalCount) {
                tableContent.Refresh();
                prevTotalCount = TotalCount;
            }
        }
        
        public float CellSize(int index)
        {
            return logRecords.Value.logs[index].EstimatedHeight();
        }

        public void SetUpCell(int index, GameObject cell)
        {
            var data = logRecords.Value.logs[index];
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
        }

        void Awake()
        {
            tableContent = GetComponent<JaggedTableContent>();
            tableContent.DataSource = this;
            tableContent.OnPreUpdate += OnPreUpdate;
            StartLogging();
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
