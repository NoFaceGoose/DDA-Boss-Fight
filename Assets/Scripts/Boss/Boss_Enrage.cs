using UnityEngine;

public class Boss_Enrage : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Invulnerable when boss is getting enraged
        animator.GetComponent<BossHealth>().isInvulnerable = true;
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<BossHealth>().isInvulnerable = false;
        animator.GetComponent<Boss>().isEnraged = true;
        animator.ResetTrigger("Enrage");
    }
}