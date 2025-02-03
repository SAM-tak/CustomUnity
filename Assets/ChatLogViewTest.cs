using TMPro;

namespace YourProjectNamespace
{
    public class ChatLogViewTest : MonoBehaviour
    {
        public TMP_InputField inputField;

        public int maxSentence = 80;

        private void Awake()
        {
            ChatLogDataSource.SetUpDummyUsers();
        }

        public void Say()
        {
            ChatLogDataSource.Say(inputField.text);
        }

        public void AddRandomChat(int count)
        {
            for(int i = 0; i < count; ++i) ChatLogDataSource.AddRandomChat(maxSentence);
        }
    }
}
