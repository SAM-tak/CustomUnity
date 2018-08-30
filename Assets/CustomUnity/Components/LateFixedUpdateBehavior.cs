using System.Collections;
using UnityEngine;

namespace CustomUnity
{
    public abstract class LateFixedUpdateBehaviour : MonoBehaviour
    {
        protected virtual void OnEnable()
        {
            lateFixedUpdate = StartCoroutine(RunLateFixedUpdate());
        }

        protected virtual void OnDisable()
        {
            StopCoroutine(lateFixedUpdate);
        }

        Coroutine lateFixedUpdate;
        static readonly WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

        IEnumerator RunLateFixedUpdate()
        {
            while(true) {
                yield return waitForFixedUpdate;
                LateFixedUpdate();
            }
        }

        protected abstract void LateFixedUpdate();
    }
}
