using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace CustomUnity
{
    /// <summary>
    /// Fire unityevent after all recognized signals fired.
    /// </summary>
    [AddComponentMenu("CustomUnity/Signal/SumAllSignalsSlot")]
    public class SumAllSignalsSlot : MonoBehaviour
    {
        public Animator[] animators;
        public UnityEvent @event;

        public readonly Dictionary<Object, bool> signals = new();
        readonly List<Object> _keyCache = new(10);

        void OnEnable()
        {
            _keyCache.Clear();
            foreach(var i in signals.Keys.Where(x => x is SignalOnEnterState)) _keyCache.Add(i);
            for(int i = 0; i < _keyCache.Count; ++i) signals.Remove(_keyCache[i]);
            foreach(var i in animators) {
                var behaviours = i.GetBehaviours<SignalOnEnterState>();
                foreach(var j in behaviours) DefineSignal(j);
            }
        }
        
        void Reset()
        {
            CollectAnimators();
        }

        [ContextMenu("Collect Animators")]
        void CollectAnimators()
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
                LogError($"Object {key.name} emit signal without define signal.");
            }
        }

        public void ClearSignals()
        {
            _keyCache.Clear();
            foreach(var k in signals.Keys) _keyCache.Add(k);
            for(int i = 0; i < _keyCache.Count; ++i) signals[_keyCache[i]] = false;
        }
    }
}
