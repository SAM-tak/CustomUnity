using UnityEngine;
using UnityEngine.Events;

namespace CustomUnity
{
    /// <summary>
    /// Fire unity event after specified seconds later
    /// </summary>
    [AddComponentMenu("CustomUnity/AfterSecondsSlot")]
    public class AfterSecondsSlot : LateFixedUpdateBehaviour
    {
        public float time;
        public bool unsacled;
        public UnityEvent @event;

        float _startTime;

        protected override void OnEnable()
        {
            base.OnEnable();
            _startTime = Time.fixedTime;
        }

        protected override void LateFixedUpdate()
        {
            if(_startTime + time < (unsacled ? Time.fixedTime : Time.fixedUnscaledTime)) @event?.Invoke();
        }
    }
}
