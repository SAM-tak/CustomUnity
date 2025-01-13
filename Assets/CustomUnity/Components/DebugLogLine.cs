using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomUnity
{
    /// <summary>
    /// DebugLog View Controller
    /// </summary>
    public class DebugLogLine : MonoBehaviour, IPointerClickHandler
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

        public static string dateTimeFormatString = "u";
        public static bool universalTime = false;

        DebugLogView _debugLogView;

        void Awake()
        {
            _debugLogView = GetComponentInParent<DebugLogView>();
        }

        public void SetUp(DebugLogDataSource.Log data, bool isCollapsed, bool isAltBackground)
        {
            dateTime.text = (universalTime ? data.dateTime : data.dateTime.ToLocalTime()).ToString(dateTimeFormatString);
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

        public void OnPointerClick(PointerEventData eventData)
        {
            _debugLogView.ShowDetail(this);
        }
    }
}
