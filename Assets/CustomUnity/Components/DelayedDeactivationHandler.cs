// use coroutine for avoid depend to external library (via. UniTask)
using System.Collections;
using UnityEngine;

namespace CustomUnity
{
    /// <summary>
    /// Deactivate GameObject on end of frame
    /// </summary>
    /// <remarks>
    /// This component cooperates with DeactivateOnExit StateMachineBehaviour
    /// 
    /// If you want to avoid allocation in runtime, add this component manually.
    /// </remarks>
    /// <see cref="DeactivateOnExit"/>
    [AddComponentMenu("CustomUnity/DelayedDeactivationHandler")]
    public class DelayedDeactivationHandler : MonoBehaviour
    {
        public void Execute()
        {
            StartCoroutine(DeactivateOnEndOfFrame());
        }

        static readonly WaitForEndOfFrame _waitForEndOfFrame = new();

        IEnumerator DeactivateOnEndOfFrame()
        {
            yield return _waitForEndOfFrame;
            gameObject.SetActive(false);
        }
    }
}
