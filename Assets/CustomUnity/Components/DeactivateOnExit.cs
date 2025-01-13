using UnityEngine;

namespace CustomUnity
{
    /// <summary>
    /// Deactivate own GameObject on StateMachine exit
    /// </summary>
    /// <see cref="DelayedDeactivationHandler"/>
    public class DeactivateOnExit : StateMachineBehaviour
    {
        public bool allowDelayedDeactivation = false;
        public bool warnDelayedDeactivationHandlerAddedInRuntime = false;

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
                if(!animator.TryGetComponent<DelayedDeactivationHandler>(out var delayedDeactivationHandler)) {
                    if(warnDelayedDeactivationHandlerAddedInRuntime) {
                        Log.Warning(animator, $"DelayedDeactivationHandler was added in runtime.");
                    }
                    delayedDeactivationHandler = animator.gameObject.AddComponent<DelayedDeactivationHandler>();
                }
                delayedDeactivationHandler.Execute();
            }
            else animator.gameObject.SetActive(false);
        }
    }
}
