using UnityEngine;
using NPBehave;

public class Boss : MonoBehaviour
{
    public Transform player;
    public Rigidbody2D rb;

    public bool isFlipped = false;
    public bool isEnraged = false;

    public float attackRange = 3f;
    public float enragedAttackRange = 3.5f;
    public float fireRange = 15.0f;
    public int dangerHealth = 200;

    private BossHealth b_health; // Reference to boss's health script, used by the AI to deal with health.
    public Root tree; // The boss's behaviour tree
    private Blackboard blackboard; // The boss's behaviour blackboard

    // Start behaviour tree
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        b_health = GetComponent<BossHealth>();

        tree = BehaviourTree();
        blackboard = tree.Blackboard;

#if UNITY_EDITOR
        Debugger debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
        debugger.BehaviorTree = tree;
#endif
    }

    private void UpdatePerception()
    {
        blackboard["playerDistance"] = Vector2.Distance(player.position, rb.position);
        blackboard["health"] = b_health.health;
        blackboard["isEnraged"] = isEnraged;
    }

    private Root BehaviourTree()
    {
        Node sel = new Selector(PhaseTwoBehaviour(), EnrageBehaviour(), PhaseOneBehaviour());
        Node service = new Service(0.2f, UpdatePerception, sel);
        return new Root(service);
    }

    private Node EnrageBehaviour()
    {
        // Enrange when health is under the danger value
        return new BlackboardCondition("health", Operator.IS_SMALLER_OR_EQUAL, dangerHealth, Stops.IMMEDIATE_RESTART, new Action(() => Enrage()));
    }

    private Node PhaseOneBehaviour()
    {
        // Fire (sword wind) if the player is in fire range
        Node bb1 = new BlackboardCondition("playerDistance", Operator.IS_SMALLER_OR_EQUAL, fireRange, Stops.IMMEDIATE_RESTART, new Action(() => Fire()));
        // Select between move and fire when the player is not in attack range, more likely to move
        Node rndSel = new RandomSelector(bb1, new Action(() => MoveToPlayer()), new Action(() => MoveToPlayer()));
        // Attack the player if the player is in attacking range
        Node bb2 = new BlackboardCondition("playerDistance", Operator.IS_SMALLER_OR_EQUAL, attackRange, Stops.IMMEDIATE_RESTART, new Action(() => Attack()));
        // Look at the player at first, then wait for 0.5 second, let the last state continue for a while
        return new Sequence(new Action(() => LookAtPlayer()), new Wait(0.5f), new Selector(bb2, rndSel));
    }

    private Node PhaseTwoBehaviour()
    {
        // Attack the player if player is in attack range
        Node bb1 = new BlackboardCondition("playerDistance", Operator.IS_SMALLER_OR_EQUAL, enragedAttackRange, Stops.IMMEDIATE_RESTART, new Action(() => Attack()));
        // Run to the player
        Node sel = new Selector(bb1, new Action(() => RunToPlayer()));
        // Look at the player first
        Node seq = new Sequence(new Action(() => LookAtPlayer()), bb1);
        // Enter phase two when enraged
        return new BlackboardCondition("isEnraged", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, seq);
    }

    private void Enrage()
    {
        GetComponent<Animator>().SetBool("IsEnraged", true);
    }

    private void Attack()
    {
        GetComponent<Animator>().SetBool("IsMoving", false);
        GetComponent<Animator>().SetTrigger("Attack");
    }

    private void Fire()
    {
        GetComponent<Animator>().SetBool("IsMoving", false);
        GetComponent<Animator>().SetTrigger("Fire");
    }

    public void LookAtPlayer()
    {
        Vector3 flipped = transform.localScale;
        flipped.z *= -1f;

        if (transform.position.x > player.position.x && isFlipped)
        {
            transform.localScale = flipped;
            transform.Rotate(0f, 180f, 0f);
            isFlipped = false;
        }
        else if (transform.position.x < player.position.x && !isFlipped)
        {
            transform.localScale = flipped;
            transform.Rotate(0f, 180f, 0f);
            isFlipped = true;
        }
    }

    public void MoveToPlayer()
    {
        GetComponent<Animator>().SetBool("IsMoving", true);
        GetComponent<Animator>().ResetTrigger("Attack");
        GetComponent<Animator>().ResetTrigger("Fire");
    }

    public void RunToPlayer()
    {
        GetComponent<Animator>().ResetTrigger("Attack");
        GetComponent<Animator>().ResetTrigger("Fire");
    }
}