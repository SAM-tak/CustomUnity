using UnityEngine;
using UnityEngine.Events;

namespace CustomUnity
{
    [AddComponentMenu("CustomUnity/Event")]
    public class Event : MonoBehaviour
    {
        public UnityEvent @event;
        
        public void Fire()
        {
            @event?.Invoke();
        }
    }
}
