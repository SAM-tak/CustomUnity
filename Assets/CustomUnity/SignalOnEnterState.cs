using UnityEngine;

namespace CustomUnity
{
    public class SignalOnEnterState : StateMachineBehaviour
    {
        public int layer;

        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
        {
            if(animator.GetComponent<SumAllSignalsEvent>()) animator.GetComponent<SumAllSignalsEvent>().DefineSignal(this);
        }

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if(layerIndex == layer) animator.GetComponent<SumAllSignalsEvent>()?.EmitSignal(this);
        }
    }
}