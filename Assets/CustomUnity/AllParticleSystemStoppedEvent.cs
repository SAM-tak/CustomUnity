using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace CustomUnity
{
    public class AllParticleSystemStoppedEvent : MonoBehaviour
    {
        public UnityEvent @event;

        ParticleSystem[] particleSystems;

        void Start()
        {
            particleSystems = GetComponentsInChildren<ParticleSystem>();
        }

        void LateUpdate()
        {
            if(particleSystems.All(x => x.isStopped)) @event?.Invoke();
        }
    }
}
