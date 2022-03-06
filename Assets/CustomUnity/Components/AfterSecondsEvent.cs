using UnityEngine;
using UnityEngine.Events;

namespace CustomUnity
{
    /// <summary>
    /// Fire unity event after specified seconds later
    /// </summary>
    [AddComponentMenu("CustomUnity/AfterSecondsEvent")]
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
