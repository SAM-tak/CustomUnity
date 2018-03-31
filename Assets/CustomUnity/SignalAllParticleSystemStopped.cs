using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace CustomUnity
{
    public class SignalAllParticleSystemStopped : MonoBehaviour
    {
        public SumAllSignalsEvent @event;

        ParticleSystem[] particleSystems;

        void Start()
        {
            particleSystems = GetComponentsInChildren<ParticleSystem>();
            @event?.DefineSignal(this);
        }

        void LateUpdate()
        {
            if(particleSystems.All(x => x.isStopped)) @event?.EmitSignal(this);
        }
    }
}
