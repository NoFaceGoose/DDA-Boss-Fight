using UnityEngine;
using NPBehave;
using System.Collections.Generic;

public class Boss : MonoBehaviour
{
    public GameObject player;
    public Rigidbody2D rb;

    public bool isFlipped = false;
    public bool isEnraged = false;

    public float nearAttackRange = 3f;
    public float nearEnragedAttackRange = 3.5f;
    public float farAttackRange = 15.0f;
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

        attackData = new Dictionary<string, AttackInfo>();
        attackData.Add("Slash", new AttackInfo(GetComponent<BossWeapon>().attackDamage));
        attackData.Add("Fire", new AttackInfo(GetComponent<BossWeapon>().swordWind.GetComponent<SwordWind>().damage));
        attackData.Add("ThrowPotion", new AttackInfo(GetComponent<BossWeapon>().potion.GetComponent<Potion>().damage));
        attackData.Add("Stab", new AttackInfo(GetComponent<BossWeapon>().enragedAttackDamage));

        switch (AI)
        {
            // Default BT
            case 0:
                // Start behaviour tree
                tree = BehaviourTree();
                blackboard = tree.Blackboard;

#if UNITY_EDITOR
                Debugger debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
                debugger.BehaviorTree = tree;
#endif
                break;

            // Rule-based DDA
            case 1:
                playerTotalHealth = player.GetComponent<PlayerHealth>().health;
                bossTotalHealth = b_health.maxHealth + b_health.maxDefense;

                MoveToPlayer();
                InvokeRepeating("RuleBasedDDA", 3f, 2f);
                break;

            default: break;
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

            if (Vector2.Distance(player.transform.position, rb.position) <= nearEnragedAttackRange)
            {
                Stab();
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
                if (Vector2.Distance(player.transform.position, rb.position) <= nearAttackRange)
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
                            default: Slash(); break;
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
                        Debug.Log("Slash hit/total: " + attackData["Slash"].hit + "/" + attackData["Slash"].total +
                            "; Fire hit/total: " + attackData["Fire"].hit + "/" + attackData["Fire"].total +
                            "; ThrowPotion hit/total: " + attackData["ThrowPotion"].hit + "/" + attackData["ThrowPotion"].total);
                        Debug.Log("Slash expectation: " + attackData["Slash"].expectation + "; Fire expectation: " + attackData["Fire"].expectation + "; ThrowPotion expectation: " + attackData["ThrowPotion"].expectation);
                        Debug.Log("Slash weight: " + attackData["Slash"].weight + "; Fire weight: " + attackData["Fire"].weight + "; ThrowPotion weight: " + attackData["ThrowPotion"].weight);
                        Debug.Log("Random Value: " + value);

                        if (value >= 0f && value < attackData["Slash"].weight)
                        {
                            Slash();
                        }
                        else if (value >= attackData["Slash"].weight && value < attackData["Slash"].weight + attackData["Fire"].weight)
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
                else if (Vector2.Distance(player.transform.position, rb.position) <= farAttackRange)
                {
                    // each attack has to be executed once to run the DDA system
                    bool useDDA = true;

                    foreach (var item in attackData)
                    {
                        if (item.Key != "Slash" && item.Value.total == 0)
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
                            if (item.Key != "Slash")
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
        // Select between fire and throw potion
        Node rndSel1 = new RandomSelector(new Action(() => Fire()), new Action(() => ThrowPotion()));
        // Use far attacks if the player is in far attack range
        Node bb1 = new BlackboardCondition("playerDistance", Operator.IS_SMALLER_OR_EQUAL, farAttackRange, Stops.IMMEDIATE_RESTART, rndSel1);
        // Select between far attacks and move when the player is not in attack range, more likely to move
        Node rndSel2 = new RandomSelector(bb1, new Action(() => MoveToPlayer()), new Action(() => MoveToPlayer()));

        // Select between fire, throw potion and slash
        Node rndSel3 = new RandomSelector(new Action(() => Fire()), new Action(() => ThrowPotion()), new Action(() => Slash()), new Action(() => Slash()));
        // Use all attacks the player if the player is in near attack range
        Node bb2 = new BlackboardCondition("playerDistance", Operator.IS_SMALLER_OR_EQUAL, nearAttackRange, Stops.IMMEDIATE_RESTART, rndSel3);

        // Look at the player at first, then wait for 0.5 second, let the last state continue for a while
        return new Sequence(new Action(() => LookAtPlayer()), new Wait(0.5f), new Selector(bb2, rndSel2));
    }

    private Node PhaseTwoBehaviour()
    {
        // Select among far attacks, more likely to stab
        Node rndSel1 = new RandomSelector(new Action(() => Fire()), new Action(() => ThrowPotion()), new Action(() => Slash()), new Action(() => Stab()), new Action(() => Stab()));
        // Attack the player if player is in attack range
        Node bb = new BlackboardCondition("playerDistance", Operator.IS_SMALLER_OR_EQUAL, nearEnragedAttackRange, Stops.IMMEDIATE_RESTART, rndSel1);

        // Select between far attacks and run to the player
        Node rndSel2 = new RandomSelector(new Action(() => Fire()), new Action(() => ThrowPotion()), new Action(() => RunToPlayer()), new Action(() => RunToPlayer()));

        // Look at the player first, then check attacks
        Node seq = new Sequence(new Action(() => LookAtPlayer()), new Selector(bb, rndSel2));

        // Enter phase two when enraged
        return new BlackboardCondition("isEnraged", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, seq);
    }

    private void Enrage()
    {
        GetComponent<Animator>().SetTrigger("Enrage");
    }

    private void Slash()
    {
        GetComponent<Animator>().SetBool("IsMoving", false);
        GetComponent<Animator>().SetTrigger("Slash");
    }

    private void Stab()
    {
        GetComponent<Animator>().SetTrigger("Stab");
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
        GetComponent<Animator>().ResetTrigger("Slash");
        GetComponent<Animator>().ResetTrigger("Fire");
        GetComponent<Animator>().ResetTrigger("ThrowPotion");
    }

    private void RunToPlayer()
    {
        GetComponent<Animator>().ResetTrigger("Slash");
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