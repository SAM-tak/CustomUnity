using UnityEngine;

namespace CustomUnity
{
    /// <summary>
    /// A signal for SumAllSignalsEvent
    /// </summary>
    [AddComponentMenu("CustomUnity/Signal/Signal")]
    public class Signal : MonoBehaviour
    {
        public SumAllSignalsEvent @event;

        void Start()
        {
            if(@event) @event.DefineSignal(this);
        }

        public void Emit()
        {
            if(@event) @event.EmitSignal(this);
        }
    }
}
