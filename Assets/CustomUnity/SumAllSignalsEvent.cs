using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace CustomUnity
{
    [AddComponentMenu("CustomUnity/Signal/SumAllSignalsEvent")]
    public class SumAllSignalsEvent : MonoBehaviour
    {
        public Animator[] animators;
        public UnityEvent @event;

        public readonly Dictionary<Object, bool> signals = new Dictionary<Object, bool>();

        void OnEnable()
        {
            foreach(var i in signals.Keys.Where(x => x is SignalOnEnterState).ToArray()) signals.Remove(i);
            foreach(var i in animators) {
                var behaviours = i.GetBehaviours<SignalOnEnterState>();
                foreach(var j in behaviours) DefineSignal(j);
            }
        }
        
        void Reset()
        {
            animators = GetComponentsInChildren<Animator>();
        }

        public void DefineSignal(Object key)
        {
            signals[key] = false;
        }

        public void UndefineSignal(Object key)
        {
            if(signals.ContainsKey(key)) signals.Remove(key);
        }

        public void EmitSignal(Object key)
        {
            if(signals.ContainsKey(key)) {
                signals[key] = true;
                if(signals.All(x => !x.Key || x.Value == true)) {
                    @event?.Invoke();
                    ClearSignals();
                }
            }
            else {
                LogError("Object {0} emit signal without define signal.", key.name);
            }
        }

        public void ClearSignals()
        {
            foreach(var k in signals.Keys.ToArray()) signals[k] = false;
        }
    }
}
