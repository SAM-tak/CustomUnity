using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YourProjectNamespace
{
    public class LogViewTest : MonoBehaviour
    {
        public TMPro.TMP_InputField inputField;
        public TMPro.TMP_Dropdown dropdown;

        public void AddLog()
        {
            switch(dropdown.value) {
            case 0:
                LogInfo(inputField.text);
                break;
            case 1:
                LogWarning(inputField.text);
                break;
            case 2:
                LogError(inputField.text);
                break;
            }
        }
    }
}
