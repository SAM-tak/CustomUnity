using UnityEngine.Events;

namespace CustomUnity
{
    public class Event : MonoBehaviour
    {
        public UnityEvent @event;
        
        public void Fire()
        {
            @event?.Invoke();
        }
    }
}
