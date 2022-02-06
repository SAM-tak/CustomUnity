using UnityEngine;
using UnityEngine.UI;

namespace CustomUnity
{
    public class DebugLogView : MonoBehaviour
    {
        const string prefKeyPrefix = "customunity.debuglogview.";
        const string prefIncludeInfoKey = prefKeyPrefix + "includeinfo";
        const string prefIncludeWarningKey = prefKeyPrefix + "includewaring";
        const string prefIncludeErrorKey = prefKeyPrefix + "includeerror";

        public Toggle infoToggle;
        public Toggle warningToggle;
        public Toggle errorToggle;

        void OnEnable()
        {
            infoToggle.isOn = PlayerPrefs.GetInt(prefIncludeInfoKey, 1) > 0;
            warningToggle.isOn = PlayerPrefs.GetInt(prefIncludeWarningKey, 1) > 0;
            errorToggle.isOn = PlayerPrefs.GetInt(prefIncludeErrorKey, 1) > 0;
        }

        void OnDisable()
        {
            PlayerPrefs.SetInt(prefIncludeInfoKey, infoToggle.isOn ? 1 : 0);
            PlayerPrefs.SetInt(prefIncludeWarningKey, warningToggle.isOn ? 1 : 0);
            PlayerPrefs.SetInt(prefIncludeErrorKey, errorToggle.isOn ? 1 : 0);
        }
    }
}
