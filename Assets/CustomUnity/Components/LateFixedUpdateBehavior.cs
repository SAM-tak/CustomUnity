using System.Collections;
using UnityEngine;

namespace CustomUnity
{
    /// <summary>
    /// MonoBehaviour has 'LateFixedUpdate'.
    /// </summary>
    public abstract class LateFixedUpdateBehaviour : MonoBehaviour
    {
        protected virtual void OnEnable()
        {
            lateFixedUpdate = StartCoroutine(RunLateFixedUpdate());
        }

        protected virtual void OnDisable()
        {
            if(lateFixedUpdate != null) {
                StopCoroutine(lateFixedUpdate);
                lateFixedUpdate = null;
            }
        }

        Coroutine lateFixedUpdate;
        static readonly WaitForFixedUpdate waitForFixedUpdate = new ();

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
