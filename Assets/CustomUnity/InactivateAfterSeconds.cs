using UnityEngine;

namespace CustomUnity
{
    public class InactivateAfterSeconds : LateFixedUpdateBehaviour
    {
        public float time;

        float startTime;

        protected override void OnEnable()
        {
            base.OnEnable();
            startTime = Time.fixedTime;
        }

        protected override void LateFixedUpdate()
        {
            if(startTime + time < Time.fixedTime) gameObject.SetActive(false);
        }
    }

    public class InactivateAfterUnscaledSeconds : LateFixedUpdateBehaviour
    {
        public float time;

        float startTime;

        protected override void OnEnable()
        {
            base.OnEnable();
            startTime = Time.fixedUnscaledTime;
        }

        protected override void LateFixedUpdate()
        {
            if(startTime + time < Time.fixedUnscaledTime) gameObject.SetActive(false);
        }
    }
}
