using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace CustomUnity
{
    [AddComponentMenu("CustomUnity/Named Event")]
    public class NamedEvent : MonoBehaviour
    {
        [System.Serializable]
        public class NamedUnityEvent
        {
            public string name;
            public UnityEvent @event;
        }
        public NamedUnityEvent[] events;
        
        public void Fire(string name)
        {
            events.FirstOrDefault(x => x.name == name)?.@event.Invoke();
        }
    }
}
