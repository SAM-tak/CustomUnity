using Bogus;
using UnityEngine;

namespace YourProjectNamespace
{
    public class DebugLogViewTest : MonoBehaviour
    {
        public TMPro.TMP_InputField inputField;
        public TMPro.TMP_Dropdown dropdown;

        public int maxSentence = 80;

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
