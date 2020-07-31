using System.Collections;
using UnityEngine;

namespace CustomUnity
{
    public class InactivateOnExit : StateMachineBehaviour
    {
        public bool allowDelayedInactivation = false;

        // OnStateMachineExit is called when exiting a statemachine via its Exit Node
        override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
        {
#if UNITY_EDITOR
            if(!allowDelayedInactivation) {
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
            if(allowDelayedInactivation) {
                var delayedInactivationHandler = animator.GetComponent<DelayedInactivationHandler>();
                if(!delayedInactivationHandler) delayedInactivationHandler = animator.gameObject.AddComponent<DelayedInactivationHandler>();
                delayedInactivationHandler.StartCoroutine(InactiveOnEndOfFrame(animator));
            }
            else animator.gameObject.SetActive(false);
        }

        // use coroutine for avoid depend to external library (via. UniTask)
        internal class DelayedInactivationHandler : MonoBehaviour { }
        static readonly WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
        IEnumerator InactiveOnEndOfFrame(Animator animator)
        {
            yield return waitForEndOfFrame;
            animator.gameObject.SetActive(false);
        }
    }
}
