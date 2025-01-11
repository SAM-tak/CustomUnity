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
            _lateFixedUpdate = StartCoroutine(RunLateFixedUpdate());
        }

        protected virtual void OnDisable()
        {
            if(_lateFixedUpdate != null) {
                StopCoroutine(_lateFixedUpdate);
                _lateFixedUpdate = null;
            }
        }

        Coroutine _lateFixedUpdate;
        static readonly WaitForFixedUpdate _waitForFixedUpdate = new();

        IEnumerator RunLateFixedUpdate()
        {
            while(true) {
                yield return _waitForFixedUpdate;
                LateFixedUpdate();
            }
        }

        protected abstract void LateFixedUpdate();
    }
}
