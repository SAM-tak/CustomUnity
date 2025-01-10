#if UNITY_EDITOR
#define USE_BOGUS
#endif
using UnityEngine;
#if USE_BOGUS
using Bogus;
#else
using System.Text;
#endif

namespace YourProjectNamespace
{
    public class DebugLogViewTest : MonoBehaviour
    {
        public TMPro.TMP_InputField inputField;
        public TMPro.TMP_Dropdown dropdown;

        public int maxSentence = 80;

        void Test()
        {
            LogInfo("Test");
        }

#if USE_BOGUS
        event System.Action TestEvent;
#else
        event global::System.Action TestEvent;
#endif
        private void Awake()
        {
            TestEvent += Test;
            TestEvent();
            TestEvent -= Test;
            LogInfo($"test == null {TestEvent == null}");
        }

        public void AddLog()
        {
            AddLog(dropdown.value, inputField.text);
        }

        public void AddLog(int type, string text)
        {
            switch(type) {
            case 0:
                LogInfo(text);
                break;
            case 1:
                LogWarning(text);
                break;
            case 2:
                LogError(text);
                break;
            }
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

        class System
        {
            public global::System.Exception Exception() => new global::System.SystemException(GenRandomString(12));
        }

        class Faker
        {
            public Faker(string _)
            {
            }

            public Lorem Lorem { get; set; } = new();
            public System System { get; set; } = new();
        }
#endif

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

        public void AddRandomLog()
        {
            var faker = fakers[Random.Range(0, fakers.Length)];
            var type = Random.Range(0, 4);
            if(type == 3) LogException(faker.System.Exception());
            else AddLog(type, faker.Lorem.Sentence(Random.Range(3, Mathf.Max(3, maxSentence))));
        }

        public void AddRandomLog(int count)
        {
            for(int i = 0; i < count; ++i) AddRandomLog();
        }
    }
}
