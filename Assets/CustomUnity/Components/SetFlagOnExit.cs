using UnityEngine;

namespace CustomUnity
{
    public class SetFlagOnExit : StateMachineBehaviour
    {
        public SumSomeFlagsEvent.Flags flag;

        // OnStateMachineExit is called when exiting a statemachine via its Exit Node
        override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
        {
            var sumSomeFlagsEvent = animator.GetComponent<SumSomeFlagsEvent>();
            if(sumSomeFlagsEvent) sumSomeFlagsEvent.SetFlag(flag);
        }
    }
}
