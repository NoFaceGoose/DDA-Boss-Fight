using UnityEngine;

public class Boss_ThrowPotion : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger("ThrowPotion");
    }
}
