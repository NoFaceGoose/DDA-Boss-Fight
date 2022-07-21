using UnityEngine;

public class Boss_Intro : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Start the behaviour tree when the intro animation is done
        animator.GetComponent<Boss>().tree.Start();
    }
}