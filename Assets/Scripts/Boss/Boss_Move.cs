using UnityEngine;

public class Boss_Move : StateMachineBehaviour
{
    public Rigidbody2D rb;
    public Transform player;
    public float speed;
    public bool isRunning;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        rb = animator.GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (isRunning)
        {
            FindObjectOfType<AudioManager>().Play("BossRunning");
        }
        else
        {
            FindObjectOfType<AudioManager>().Play("BossWalking");
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Move to the player with a specific speed
        Vector2 target = new Vector2(player.position.x, rb.position.y);
        Vector2 newPos = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        FindObjectOfType<AudioManager>().Stop("BossWalking");
        FindObjectOfType<AudioManager>().Stop("BossRunning");
    }
}