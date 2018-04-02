using UnityEngine;

namespace CustomUnity
{
    public class SignalOnEnterState : StateMachineBehaviour
    {
        public int layer;
        
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if(layerIndex == layer) animator.GetComponent<SumAllSignalsEvent>()?.EmitSignal(this);
        }
    }
}