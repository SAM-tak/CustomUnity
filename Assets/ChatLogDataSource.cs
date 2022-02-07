using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CustomUnity;
using Bogus;
using Random = UnityEngine.Random;
using UnityEngine.Networking;
using System.Collections;

namespace YourProjectNamespace
{
    [RequireComponent(typeof(JaggedTableContent))]
    public class ChatLogDataSource : MonoBehaviour, JaggedTableContent.IDataSource
    {
        struct Log
        {
            public DateTime dateTime;
            public int userId;
            public string message;
        }

        struct User
        {
            public string name;
            public Sprite icon;
            public Faker faker;
        }

        static readonly Dictionary<int, User> userAccounts = new() {
            [1] = new User { name = "You", icon = null, faker = new("ja"), },
        };

        class LogData
        {
            public HashSet<string> texts;
            public List<Log> logs;

            public void Add(Log log)
            {
                if(texts.TryGetValue(log.message, out var existing)) log.message = existing;
                else texts.Add(log.message);
                logs.Add(log);
            }

            public void Clear()
            {
                logs.Clear();
                texts.Clear();
            }
        }

        static readonly Lazy<LogData> logRecords = new(() => new() { texts = new(256), logs = new(256) });
        static event Action OnLogCleared;

        static readonly Faker[] fakers = {
            new Faker("en"),
            new Faker("fr"),
            new Faker("de"),
            new Faker("it"),
            new Faker("es"),
            new Faker("ja"),
            new Faker("zh_CN"),
            new Faker("zh_TW"),
            new Faker("ko"),
            new Faker("af_ZA"),
            new Faker("ar"),
        };

        const int userCount = 20;

        public static void SetUpDummyUsers()
        {
            for(int i = 2; i < userCount; ++i) {
                var faker = fakers[Random.Range(0, fakers.Length)];
                var person = faker.Person;
                userAccounts[i] = new() { name = person.FirstName, icon = null, faker = faker };
            }
        }

        public static void AddRandomChat(int maxSentence)
        {
            var userId = Random.Range(1, userCount);
            logRecords.Value.Add(new() {
                dateTime = DateTime.Now,
                userId = userId,
                message = userAccounts[userId].faker.Lorem.Sentence(Random.Range(3, Mathf.Max(3, maxSentence)))
            });
        }

        public static void Say(string message)
        {
            logRecords.Value.Add(new() {
                dateTime = DateTime.Now,
                userId = 1,
                message = message
            });
        }

        public static void ClearLog()
        {
            if(logRecords.IsValueCreated) {
                logRecords.Value.Clear();
                OnLogCleared();
            }
        }

        public int TotalCount => logRecords.IsValueCreated ? logRecords.Value.logs.Count : 0;

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
            if(index < 0 || logRecords.Value.logs.Count <= index) return minHeight;

            if(preferredHeightCache.ContainsKey(index)) return preferredHeightCache[index];

            var log = logRecords.Value.logs[index];
            var cell = tableContent.GetActiveCell(index);
            if(cell && cell.activeInHierarchy) {
                var text = cell.transform.Find("Self/Message").GetComponent<Text>();
                if(!text.gameObject.activeInHierarchy) text = cell.transform.Find("Other/Message").GetComponent<Text>();
                if(text.text.Equals(log.message)) {
                    var preferredHeight = Mathf.Max(minHeight, text.preferredHeight + mergin * 2f);
                    preferredHeightCache[index] = preferredHeight;
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
            var data = logRecords.Value.logs[index];

            var root = cell.transform.Find(data.userId == 1 ? "Self" : "Other");
            root.gameObject.SetActive(true);
            root.transform.Find("Profile/DateTime").GetComponent<Text>().text = data.dateTime.ToString("g");
            root.transform.Find("Profile/Name").GetComponent<Text>().text = userAccounts[data.userId].name;
            //root.transform.Find("Profile/Icon").GetComponent<Image>().sprite = userAccounts[data.userId].icon;
            root.transform.Find("Message").GetComponent<Text>().text = data.message;

            root = cell.transform.Find(data.userId != 1 ? "Self" : "Other");
            root.gameObject.SetActive(false);
            root.transform.Find("Profile/DateTime").GetComponent<Text>().text = null;
            root.transform.Find("Profile/Name").GetComponent<Text>().text = null;
            //root.transform.Find("Profile/Icon").GetComponent<Image>().sprite = null;
            root.transform.Find("Message").GetComponent<Text>().text = null;
        }

        float minHeight;
        int wrapColumnCount;
        int fontSize;
        float lineSpacing;
        float mergin;

        void Awake()
        {
            tableContent = GetComponent<JaggedTableContent>();
            OnLogCleared += ClearPreferredHeightCache;

            var firstChild = transform.GetChild(0);

            minHeight = tableContent.orientaion switch {
                Orientaion.Horizontal => firstChild.GetComponent<RectTransform>().rect.width,
                _ => firstChild.GetComponent<RectTransform>().rect.height
            };

            var text = firstChild.Find("Self/Message").GetComponent<Text>();
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
