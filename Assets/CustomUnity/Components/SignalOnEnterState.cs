using UnityEngine;

namespace CustomUnity
{
    /// <summary>
    /// A signal for SumAllSignalsEvent. StateMachineBehaviour variation.
    /// </summary>
    public class SignalOnEnterState : StateMachineBehaviour
    {
        [Tooltip("Specify layer no. If set less than 0, it means ignore layer no.")]
        public int layer;
        
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if(layer < 0 || layerIndex == layer) {
                var sumAllSignalsSlot = animator.GetComponent<SumAllSignalsSlot>();
                if(sumAllSignalsSlot) sumAllSignalsSlot.EmitSignal(this);
            }
        }
    }
}