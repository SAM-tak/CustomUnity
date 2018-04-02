using UnityEngine;

namespace CustomUnity
{
    [RequireComponent(typeof(ParticleSystem))]
    public class SignalParticleSystemStopped : MonoBehaviour
    {
        public SumAllSignalsEvent @event;

        void OnValidate()
        {
            var particleSystem = GetComponent<ParticleSystem>();
            var main = particleSystem.main;
            main.stopAction = ParticleSystemStopAction.Callback;
        }

        void Start()
        {
            @event?.DefineSignal(this);
        }

        void OnParticleSystemStopped()
        {
            @event?.EmitSignal(this);
        }
    }
}