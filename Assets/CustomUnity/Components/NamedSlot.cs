using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace CustomUnity
{
    /// <summary>
    /// map string to unityevent, and fire by AnimationEvent or other.
    /// </summary>
    [AddComponentMenu("CustomUnity/Named Slot")]
    public class NamedSlot : MonoBehaviour
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
