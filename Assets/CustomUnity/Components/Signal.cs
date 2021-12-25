using UnityEngine;

namespace CustomUnity
{
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
