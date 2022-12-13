using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnComplete : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ClientData.Pause = true;
        ClientData.Finished=true;
        Destroy(animator.gameObject, stateInfo.length);

    }

   
}
