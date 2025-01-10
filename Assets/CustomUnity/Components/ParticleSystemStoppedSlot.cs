using UnityEngine;
using UnityEngine.Events;

namespace CustomUnity
{
    /// <summary>
    /// unity event holder for OnParticleSystemStopped event.
    /// </summary>
    [AddComponentMenu("CustomUnity/ParticleSystemStoppedSlot")]
    public class ParticleSystemStoppedSlot : MonoBehaviour
    {
        public UnityEvent @event;

        void OnValidate()
        {
            var main = GetComponent<ParticleSystem>().main;
            main.stopAction = ParticleSystemStopAction.Callback;
        }

        void OnParticleSystemStopped()
        {
            @event?.Invoke();
        }
    }
}
