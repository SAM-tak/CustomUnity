using UnityEngine;

namespace CustomUnity
{
    /// <summary>
    /// Call SumSomeFlagsEvent.SetFlag when StateMachine enter 'enter' state.
    /// </summary>
    public class SetFlasgOnEnterState : StateMachineBehaviour
    {
        public SumSomeFlagsSlot.Flags flag;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var component = animator.GetComponent<SumSomeFlagsSlot>();
            if(component) component.SetFlag(flag);
        }
    }
}