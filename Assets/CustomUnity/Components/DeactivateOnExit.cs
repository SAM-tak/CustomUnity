using System.Collections;
using UnityEngine;

namespace CustomUnity
{
    /// <summary>
    /// Deactivate own GameObject on StateMachine exit
    /// </summary>
    public class DeactivateOnExit : StateMachineBehaviour
    {
        public bool allowDelayedDeactivation = false;

        // OnStateMachineExit is called when exiting a statemachine via its Exit Node
        override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
        {
#if UNITY_EDITOR
            if(!allowDelayedDeactivation) {
                for(int layerno = 0; layerno < animator.layerCount; ++layerno) {
                    var stateInfo = animator.GetCurrentAnimatorStateInfo(layerno);
                    foreach(var i in animator.GetCurrentAnimatorClipInfo(layerno)) {
                        if(i.clip && i.clip.events != null) {
                            var toTime = stateInfo.length * stateInfo.normalizedTime;
                            var fromTime = Mathf.Max(0, toTime - (animator.updateMode == AnimatorUpdateMode.UnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime));
                            foreach(var j in i.clip.events) {
                                if(fromTime < j.time && j.time <= toTime) {
                                    LogWarning($"clip '{i.clip.name}' has animation event on last frame. this may not execute.");
                                }
                            }
                        }
                    }
                }
            }
#endif
            if(allowDelayedDeactivation) {
                var delayedDeactivationHandler = animator.GetComponent<DelayedDeactivationHandler>();
                if(!delayedDeactivationHandler) delayedDeactivationHandler = animator.gameObject.AddComponent<DelayedDeactivationHandler>();
                delayedDeactivationHandler.StartCoroutine(DeactivateOnEndOfFrame(animator));
            }
            else animator.gameObject.SetActive(false);
        }

        // use coroutine for avoid depend to external library (via. UniTask)
        internal class DelayedDeactivationHandler : MonoBehaviour { }
        static readonly WaitForEndOfFrame waitForEndOfFrame = new ();
        IEnumerator DeactivateOnEndOfFrame(Animator animator)
        {
            yield return waitForEndOfFrame;
            animator.gameObject.SetActive(false);
        }
    }
}
