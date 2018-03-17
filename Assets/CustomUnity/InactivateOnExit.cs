#if UNITY_2017_1
#define DELAYED_DISABLING
#endif
using UnityEngine;

namespace CustomUnity
{
    public class InactivateOnExit : StateMachineBehaviour
    {
        // OnStateMachineExit is called when exiting a statemachine via its Exit Node
        override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
        {
#if DELAYED_DISABLING
            if(animator.GetComponent<EmptyBehaviour>()) {
                animator.GetComponent<EmptyBehaviour>().StartCoroutine(DelayedInactive(animator));
            }
            else {
                animator.gameObject.AddComponent<EmptyBehaviour>().StartCoroutine(DelayedInactive(animator));
            }
#else
            animator.gameObject.SetActive(false);
#endif
        }

#if DELAYED_DISABLING
        internal class EmptyBehaviour : MonoBehaviour { }
        static WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
        System.Collections.IEnumerator DelayedInactive(Animator animator)
        {
            yield return waitForEndOfFrame;
            animator.gameObject.SetActive(false);
        }
#endif
    }
}
