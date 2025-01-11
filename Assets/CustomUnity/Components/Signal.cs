using UnityEngine;
using UnityEngine.Serialization;

namespace CustomUnity
{
    /// <summary>
    /// A signal for SumAllSignalsEvent
    /// </summary>
    [AddComponentMenu("CustomUnity/Signal/Signal")]
    public class Signal : MonoBehaviour
    {
        [FormerlySerializedAs("event")]
        public SumAllSignalsSlot slot;

        void Start()
        {
            if(slot) slot.DefineSignal(this);
        }

        public void Emit()
        {
            if(slot) slot.EmitSignal(this);
        }
    }
}
