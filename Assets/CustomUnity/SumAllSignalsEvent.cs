using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace CustomUnity
{
    public class SumAllSignalsEvent : MonoBehaviour
    {
        public UnityEvent @event;

        public readonly Dictionary<UnityEngine.Object, bool> signals = new Dictionary<UnityEngine.Object, bool>();
        
        public void DefineSignal(UnityEngine.Object key)
        {
            signals[key] = false;
        }

        public void UndefineSignal(UnityEngine.Object key)
        {
            if(signals.ContainsKey(key)) signals.Remove(key);
        }

        public void EmitSignal(UnityEngine.Object key)
        {
            if(signals.ContainsKey(key)) {
                signals[key] = true;
                if(signals.All(x => !x.Key || x.Value == true)) {
                    @event?.Invoke();
                    ClearSignals();
                }
            }
        }

        public void ClearSignals()
        {
            foreach(var k in signals.Keys.ToArray()) signals[k] = false;
        }
    }
}
