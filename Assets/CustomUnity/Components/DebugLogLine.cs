using UnityEngine;
using UnityEngine.UI;

namespace CustomUnity
{
    /// <summary>
    /// DebugLog View Controller
    /// </summary>
    public class DebugLogLine : MonoBehaviour
    {
        public GameObject infoIcon;
        public GameObject warningIcon;
        public GameObject errorIcon;
        public GameObject countPanel;
        public GameObject background;
        public GameObject altBackground;
        public Text dateTime;
        public Text message;
        public Text count;
        public string stackTrace;

        DebugLogView _debugLogView;

        void Awake()
        {
            _debugLogView = GetComponentInParent<DebugLogView>();
        }

        public void ShowDetail()
        {
            _debugLogView.ShowDetail(this);
        }

        public void SetUp(DebugLogDataSource.Log data, bool isCollapsed, bool isAltBackground)
        {
            dateTime.text = data.dateTime.ToString("u");
            infoIcon.SetActive(data.type == LogType.Log);
            warningIcon.SetActive(data.type == LogType.Warning);
            errorIcon.SetActive(data.type == LogType.Error || data.type == LogType.Exception || data.type == LogType.Assert);
            if(isCollapsed) {
                countPanel.SetActive(true);
                count.text = $"{data.count}";
            }
            else {
                countPanel.SetActive(false);
            }
            message.text = data.message;
            background.SetActive(!isAltBackground);
            altBackground.SetActive(isAltBackground);

            stackTrace = data.stackTrace;
        }
    }
}
