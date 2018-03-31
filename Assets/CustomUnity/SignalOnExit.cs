using UnityEngine;

namespace CustomUnity
{
    public class SignalOnExit : StateMachineBehaviour
    {
        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
        {
            if(animator.GetComponent<SumAllSignalsEvent>()) animator.GetComponent<SumAllSignalsEvent>().DefineSignal(this);
        }

        // OnStateMachineExit is called when exiting a statemachine via its Exit Node
        override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
        {
            animator.GetComponent<SumAllSignalsEvent>()?.EmitSignal(this);
        }
    }
}
