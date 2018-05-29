using UnityEngine;

namespace CustomUnity
{
    public class SignalOnEnterState : StateMachineBehaviour
    {
        [Tooltip("Specify layer no. If set less than 0, it means ignore layer no.")]
        public int layer;
        
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if(layer < 0 || layerIndex == layer) animator.GetComponent<SumAllSignalsEvent>()?.EmitSignal(this);
        }
    }
}