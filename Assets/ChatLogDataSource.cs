#if UNITY_EDITOR
#define USE_BOGUS
#endif
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CustomUnity;
using Random = UnityEngine.Random;
#if USE_BOGUS
using Bogus;
#else
using System.Text;
#endif

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

#if !USE_BOGUS
        static public string GenRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringBuilder = new StringBuilder();
            for(int i = 0; i < length; i++) {
                int index = Random.Range(0, chars.Length);
                stringBuilder.Append(chars[index]);
            }
            return stringBuilder.ToString();
        }

        class Lorem
        {
            public string Sentence(int length) => GenRandomString(length);
        }

        class Person
        {
            public Person()
            {
                FirstName = GenRandomString(Random.Range(3, 8));
            }

            public string FirstName { get; set; }
        }

        class Faker {
            public Faker(string _)
            {
            }

            public Lorem Lorem { get; set; } = new();
            public Person Person { get; set; } = new();
        }
#endif

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

        const int userCount = 20;

        static readonly Faker[] fakers = {
            new("en"),
            new("fr"),
            new("de"),
            new("it"),
            new("es"),
            new("ja"),
            new("zh_CN"),
            new("zh_TW"),
            new("ko"),
            new("af_ZA"),
            new("ar"),
        };

#if USE_BOGUS
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
#else
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
#endif

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

        int _prevTotalCount = 0;

        JaggedTableContent _tableContent;

        void JaggedTableContent.IDataSource.OnPreUpdate()
        {
            if(_prevTotalCount != TotalCount) {
                _tableContent.Refresh();
                _prevTotalCount = TotalCount;
            }
        }

        float EstimateHeight(int lineCount)
        {
            return Mathf.Max(_minHeight, lineCount * (_fontSize + 2) + Mathf.Max(0, lineCount - 1) * _lineSpacing + _mergin * 2f);
        }

        readonly Dictionary<int, float> preferredHeightCache = new(256);

        public void ClearPreferredHeightCache()
        {
            preferredHeightCache.Clear();
        }

        float JaggedTableContent.IDataSource.CellSize(int index)
        {
            if(index < 0 || logRecords.Value.logs.Count <= index) return _minHeight;

            if(preferredHeightCache.ContainsKey(index)) return preferredHeightCache[index];

            var log = logRecords.Value.logs[index];
            var cell = _tableContent.GetActiveCell(index);
            if(cell && cell.activeInHierarchy) {
                var text = cell.transform.Find("Self/Message").GetComponent<Text>();
                if(!text.gameObject.activeInHierarchy) text = cell.transform.Find("Other/Message").GetComponent<Text>();
                if(text.text.Equals(log.message)) {
                    var preferredHeight = Mathf.Max(_minHeight, text.preferredHeight + _mergin * 2f);
                    preferredHeightCache[index] = preferredHeight;
                    _tableContent.NeedsRelayout();
                    return preferredHeight;
                }
            }

            var lineCount = _wrapColumnCount > 0 ? log.message.LineCount(_wrapColumnCount) : log.message.LineCount();
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
            _tableContent = GetComponent<JaggedTableContent>();
            OnLogCleared += ClearPreferredHeightCache;

            var firstChild = transform.GetChild(0);

            _minHeight = _tableContent.orientaion switch {
                TableOrientaion.Horizontal => firstChild.GetComponent<RectTransform>().rect.width,
                TableOrientaion.Vertical => firstChild.GetComponent<RectTransform>().rect.height,
                _ => 0f
            };

            var text = firstChild.Find("Self/Message").GetComponent<Text>();
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
            OnLogCleared -= ClearPreferredHeightCache;
        }
    }
}
