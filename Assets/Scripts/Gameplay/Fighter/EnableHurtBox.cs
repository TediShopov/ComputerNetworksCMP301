using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableHurtBox : StateMachineBehaviour
{

    public int HurtBoxIndex;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ///if (!animator.gameObject.GetComponent<AttackScript>().HurtBoxes[HurtBoxIndex].activeSelf)
        //{
        animator.gameObject.GetComponent<AttackScript>().IsHitting=true;
      //  animator.gameObject.GetComponent<AttackScript>().HurtBoxes[HurtBoxIndex].SetActive(true);

        //animator.gameObject.GetComponent<AttackScript>().HurtBoxes[HurtBoxIndex].SetActive(true);
        //}
    }

    //// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    ////override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    ////{
    ////    
    ////}

    //// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.gameObject.GetComponent<AttackScript>().IsHitting=false;
        animator.gameObject.GetComponent<AttackScript>().HurtBoxes[HurtBoxIndex].SetActive(false);
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
