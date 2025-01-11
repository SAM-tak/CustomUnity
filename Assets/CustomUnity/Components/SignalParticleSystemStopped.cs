using UnityEngine;
using UnityEngine.Serialization;

namespace CustomUnity
{
    /// <summary>
    /// A signal for SumAllSignalsEvent.
    /// </summary>
    [AddComponentMenu("CustomUnity/Signal/SignalParticleSystemStopped"), RequireComponent(typeof(ParticleSystem))]
    public class SignalParticleSystemStopped : MonoBehaviour
    {
        [FormerlySerializedAs("event")]
        public SumAllSignalsSlot slot;

        void OnValidate()
        {
            var main = GetComponent<ParticleSystem>().main;
            main.stopAction = ParticleSystemStopAction.Callback;
        }

        void Start()
        {
            if(slot) slot.DefineSignal(this);
        }

        void OnParticleSystemStopped()
        {
            if(slot) slot.EmitSignal(this);
        }
    }
}