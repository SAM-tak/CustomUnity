using UnityEngine;
using UnityEngine.Events;

namespace CustomUnity
{
    public class AfterSecondsEvent : LateFixedUpdateBehaviour
    {
        public float time;
        public bool unsacled;
        public UnityEvent @event;

        float startTime;

        protected override void OnEnable()
        {
            base.OnEnable();
            startTime = Time.fixedTime;
        }

        protected override void LateFixedUpdate()
        {
            if(startTime + time < (unsacled ? Time.fixedTime : Time.fixedUnscaledTime)) @event?.Invoke();
        }
    }
}
