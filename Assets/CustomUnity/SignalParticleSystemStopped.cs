using UnityEngine.Events;

namespace CustomUnity
{
    public class SignalParticleSystemStopped : MonoBehaviour
    {
        public SumAllSignalsEvent @event;

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
