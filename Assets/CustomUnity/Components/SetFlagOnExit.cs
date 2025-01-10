using UnityEngine;

namespace CustomUnity
{
    /// <summary>
    /// Call SumSomeFlagsEvent.SetFlag when StateMachine enter exit state.
    /// </summary>
    public class SetFlagOnExit : StateMachineBehaviour
    {
        public SumSomeFlagsSlot.Flags flag;

        // OnStateMachineExit is called when exiting a statemachine via its Exit Node
        override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
        {
            var sumSomeFlagsSlot = animator.GetComponent<SumSomeFlagsSlot>();
            if(sumSomeFlagsSlot) sumSomeFlagsSlot.SetFlag(flag);
        }
    }
}
