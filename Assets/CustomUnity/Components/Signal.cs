using UnityEngine;

namespace CustomUnity
{
    [AddComponentMenu("CustomUnity/Signal/Signal")]
    public class Signal : MonoBehaviour
    {
        public SumAllSignalsEvent @event;

        void Start()
        {
            @event?.DefineSignal(this);
        }

        public void Emit()
        {
            @event?.EmitSignal(this);
        }
    }
}
