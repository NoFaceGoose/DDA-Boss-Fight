using UnityEngine;
using NPBehave;
using System.Collections.Generic;

public class Boss : MonoBehaviour
{
    public GameObject player;
    public Rigidbody2D rb;

    public bool isFlipped = false;
    public bool isEnraged = false;

    public float attackRange = 3f;
    public float enragedAttackRange = 3.5f;
    public float fireRange = 15.0f;
    public int dangerHealth = 200;

    public int AI = 1;

    public Root tree; // The boss's behaviour tree
    private Blackboard blackboard; // The boss's behaviour blackboard

    private BossHealth b_health; // Reference to boss's health script, used by the AI to deal with health.
    private int playerTotalHealth;
    private int bossTotalHealth;

    private Dictionary<string, AttackInfo> attackData;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        b_health = GetComponent<BossHealth>();
        playerTotalHealth = player.GetComponent<PlayerHealth>().health;
        bossTotalHealth = b_health.maxHealth + b_health.maxDefense;

        attackData = new Dictionary<string, AttackInfo>();
        attackData.Add("Attack", new AttackInfo(GetComponent<BossWeapon>().attackDamage));
        attackData.Add("Fire", new AttackInfo(GetComponent<BossWeapon>().swordWind.GetComponent<SwordWind>().damage));
        attackData.Add("ThrowPotion", new AttackInfo(GetComponent<BossWeapon>().potion.GetComponent<Potion>().damage));

        // Default BT
        if (AI == 0)
        {
            // Start behaviour tree
            tree = BehaviourTree();
            blackboard = tree.Blackboard;

#if UNITY_EDITOR
            Debugger debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
            debugger.BehaviorTree = tree;
#endif
        }

        // Rule-based DDA
        if (AI == 1)
        {
            MoveToPlayer();
            InvokeRepeating("RuleBasedDDA", 3f, 2f);
        }
    }

    private void FixedUpdate()
    {
        if (AI == 1)
        {
            LookAtPlayer();
        }
    }

    private void RuleBasedDDA()
    {
        if (isEnraged)
        {
            RunToPlayer();

            if (Vector2.Distance(player.transform.position, rb.position) <= enragedAttackRange)
            {
                Attack();
            }
        }
        else
        {
            if (b_health.health <= dangerHealth)
            {
                Enrage();
            }
            else
            {
                // near attack decision
                if (Vector2.Distance(player.transform.position, rb.position) <= attackRange)
                {
                    // each attack has to be executed once to run the DDA system
                    bool useDDA = true;

                    foreach (var item in attackData)
                    {
                        if (item.Value.total == 0)
                        {
                            useDDA = false;
                            break;
                        }
                    }

                    if (!useDDA)
                    {
                        int value = UnityEngine.Random.Range(0, 3);

                        switch (value)
                        {
                            case 0: Fire(); break;
                            case 1: ThrowPotion(); break;
                            default: Attack(); break;
                        }
                    }
                    else
                    {
                        float totalWeights = 0f;

                        // calculate weights
                        foreach (var info in attackData.Values)
                        {
                            // if the difference between the player's and boss's health condition is less, the attack will be assigned with more weight value
                            // weight = 1 / | (playerCurrentHP - attackExpectation) / playerTotalHP - bossCurrentHP / bossTotalHP |
                            info.weight = 1.0f / Mathf.Abs((player.GetComponent<PlayerHealth>().health - info.expectation) / playerTotalHealth - (b_health.health + b_health.defense) / bossTotalHealth);
                            totalWeights += info.weight;
                        }

                        float value = UnityEngine.Random.Range(0f, totalWeights);

                        // log attack info
                        Debug.Log("Attack hit/total: " + attackData["Attack"].hit + "/" + attackData["Attack"].total +
                            "; Fire hit/total: " + attackData["Fire"].hit + "/" + attackData["Fire"].total +
                            "; ThrowPotion hit/total: " + attackData["ThrowPotion"].hit + "/" + attackData["ThrowPotion"].total);
                        Debug.Log("Attack expectation: " + attackData["Attack"].expectation + "; Fire expectation: " + attackData["Fire"].expectation + "; ThrowPotion expectation: " + attackData["ThrowPotion"].expectation);
                        Debug.Log("Attack weight: " + attackData["Attack"].weight + "; Fire weight: " + attackData["Fire"].weight + "; ThrowPotion weight: " + attackData["ThrowPotion"].weight);
                        Debug.Log("Random Value: " + value);

                        if (value >= 0f && value < attackData["Attack"].weight)
                        {
                            Attack();
                        }
                        else if (value >= attackData["Attack"].weight && value < attackData["Attack"].weight + attackData["Fire"].weight)
                        {
                            Fire();
                        }
                        else
                        {
                            ThrowPotion();
                        }
                    }
                }

                // far attack decision
                else if (Vector2.Distance(player.transform.position, rb.position) <= fireRange)
                {
                    // each attack has to be executed once to run the DDA system
                    bool useDDA = true;

                    foreach (var item in attackData)
                    {
                        if (item.Key != "Attack" && item.Value.total == 0)
                        {
                            useDDA = false;
                            break;
                        }
                    }

                    if (!useDDA)
                    {
                        int value = UnityEngine.Random.Range(0, 4);

                        switch (value)
                        {
                            case 0: Fire(); break;
                            case 1: ThrowPotion(); break;
                            default: MoveToPlayer(); break;
                        }
                    }
                    else
                    {
                        float totalWeights = 0f;

                        // calculate weights
                        foreach (var item in attackData)
                        {
                            if (item.Key != "Attack")
                            {
                                // if the difference between the player's and boss's health condition is less, the attack will be assigned with more weight value
                                // weight = 1 / | (playerCurrentHP - attackExpectation) / playerTotalHP - bossCurrentHP / bossTotalHP |
                                item.Value.weight = 1.0f / Mathf.Abs((player.GetComponent<PlayerHealth>().health - item.Value.expectation) / playerTotalHealth - (b_health.health + b_health.defense) / bossTotalHealth);
                                totalWeights += item.Value.weight;
                            }
                        }

                        // assign 1/3 weight value to moveToPlayer when making far attack decision
                        float value = UnityEngine.Random.Range(0f, totalWeights * 1.5f);

                        // log attack info
                        Debug.Log("Fire hit/total: " + attackData["Fire"].hit + "/" + attackData["Fire"].total +
                            "; ThrowPotion hit/total: " + attackData["ThrowPotion"].hit + "/" + attackData["ThrowPotion"].total);
                        Debug.Log("Fire expectation: " + attackData["Fire"].expectation + "; ThrowPotion expectation: " + attackData["ThrowPotion"].expectation);
                        Debug.Log("Fire weight: " + attackData["Fire"].weight + "; ThrowPotion weight: " + attackData["ThrowPotion"].weight);
                        Debug.Log("Random Value: " + value);

                        if (value >= 0f && value < attackData["Fire"].weight)
                        {
                            Fire();
                        }
                        else if (value >= attackData["Fire"].weight && value < totalWeights)
                        {
                            ThrowPotion();
                        }
                        else
                        {
                            MoveToPlayer();
                        }
                    }

                }
            }
        }
    }

    private void UpdatePerception()
    {
        blackboard["playerDistance"] = Vector2.Distance(player.transform.position, rb.position);
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

        Node bb0 = new BlackboardCondition("playerDistance", Operator.IS_SMALLER_OR_EQUAL, fireRange, Stops.IMMEDIATE_RESTART, new Action(() => ThrowPotion()));

        // Select between move and fire when the player is not in attack range, more likely to move
        Node rndSel = new RandomSelector(bb0, bb1, new Action(() => MoveToPlayer()), new Action(() => MoveToPlayer()), new Action(() => MoveToPlayer()), new Action(() => MoveToPlayer()), new Action(() => MoveToPlayer()));
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
        GetComponent<Animator>().SetTrigger("Enrage");
    }

    private void Attack()
    {
        GetComponent<Animator>().SetBool("IsMoving", false);
        GetComponent<Animator>().SetTrigger("Attack");
    }

    private void ThrowPotion()
    {
        GetComponent<Animator>().SetBool("IsMoving", false);
        GetComponent<Animator>().SetTrigger("ThrowPotion");
    }

    private void Fire()
    {
        GetComponent<Animator>().SetBool("IsMoving", false);
        GetComponent<Animator>().SetTrigger("Fire");
    }

    private void LookAtPlayer()
    {
        Vector3 flipped = transform.localScale;
        flipped.z *= -1f;

        if (transform.position.x > player.transform.position.x && isFlipped)
        {
            transform.localScale = flipped;
            transform.Rotate(0f, 180f, 0f);
            isFlipped = false;
        }
        else if (transform.position.x < player.transform.position.x && !isFlipped)
        {
            transform.localScale = flipped;
            transform.Rotate(0f, 180f, 0f);
            isFlipped = true;
        }
    }

    private void MoveToPlayer()
    {
        GetComponent<Animator>().SetBool("IsMoving", true);
        GetComponent<Animator>().ResetTrigger("Attack");
        GetComponent<Animator>().ResetTrigger("Fire");
        GetComponent<Animator>().ResetTrigger("ThrowPotion");
    }

    private void RunToPlayer()
    {
        GetComponent<Animator>().ResetTrigger("Attack");
        GetComponent<Animator>().ResetTrigger("Fire");
    }

    public void UpdateAttackInfo(string attackType, bool hit = false)
    {
        if (hit)
        {
            attackData[attackType].hit++;
        }
        else
        {
            attackData[attackType].total++;
        }

        attackData[attackType].Update();
    }

    private class AttackInfo
    {
        public int total;
        public int hit;
        public int damage;
        public float expectation;
        public float weight;

        public AttackInfo(int damage)
        {
            total = 0;
            hit = 0;
            expectation = 0f;
            weight = 0;
            this.damage = damage;
        }

        // update the attack's expectation
        public void Update()
        {
            expectation = ((float)hit / total) * damage;
        }
    }
}