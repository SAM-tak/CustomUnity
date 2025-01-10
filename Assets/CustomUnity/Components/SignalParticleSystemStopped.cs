using UnityEngine;

namespace CustomUnity
{
    /// <summary>
    /// A signal for SumAllSignalsEvent.
    /// </summary>
    [AddComponentMenu("CustomUnity/Signal/SignalParticleSystemStopped"), RequireComponent(typeof(ParticleSystem))]
    public class SignalParticleSystemStopped : MonoBehaviour
    {
        public SumAllSignalsSlot @event;

        void OnValidate()
        {
            var main = GetComponent<ParticleSystem>().main;
            main.stopAction = ParticleSystemStopAction.Callback;
        }

        void Start()
        {
            if(@event) @event.DefineSignal(this);
        }

        void OnParticleSystemStopped()
        {
            if(@event) @event.EmitSignal(this);
        }
    }
}