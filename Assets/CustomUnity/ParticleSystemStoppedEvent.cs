using UnityEngine.Events;

namespace CustomUnity
{
    public class ParticleSystemStoppedEvent : MonoBehaviour
    {
        public UnityEvent @event;

        void OnParticleSystemStopped()
        {
            @event?.Invoke();
        }
    }
}
