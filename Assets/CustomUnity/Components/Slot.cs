using UnityEngine;
using UnityEngine.Events;

namespace CustomUnity
{
    /// <summary>
    /// UnityEvent holder.
    /// </summary>
    /// <remarks>
    /// for AnimationEvent.
    /// </remarks>
    [AddComponentMenu("CustomUnity/Slot")]
    public class Slot : MonoBehaviour
    {
        public UnityEvent @event;
        
        public void Fire()
        {
            @event?.Invoke();
        }
    }
}
