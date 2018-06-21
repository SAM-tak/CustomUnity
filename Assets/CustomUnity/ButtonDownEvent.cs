using UnityEngine;
using UnityEngine.Events;

namespace CustomUnity
{
    public class ButtonDownEvent : MonoBehaviour
    {
        public string actionName;
        public UnityEvent @event;

        void Update()
        {
            if(Input.GetButtonDown(actionName)) @event.Invoke();
        }
    }
}
