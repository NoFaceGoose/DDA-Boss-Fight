using UnityEngine;
using NPBehave;
using System.Collections.Generic;

public class Boss : MonoBehaviour
{
    public GameObject player;
    public Rigidbody2D rb;

    public bool isFlipped;
    public bool isEnraged;

    public float nearAttackRange;
    public float nearEnragedAttackRange;
    public float farAttackRange;
    public int dangerHealth;

    public int AI;
    public float initalWalkTime;
    public float DDAReactionRate;

    public Root tree; // The boss's behaviour tree
    private Blackboard blackboard; // The boss's behaviour blackboard

    private BossHealth bossHealth; // Reference to boss's health script, used by the AI to deal with health.

    private Dictionary<string, AttackInfo> attackData;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bossHealth = GetComponent<BossHealth>();

        attackData = new Dictionary<string, AttackInfo>();
        attackData.Add("Slash", new AttackInfo(GetComponent<BossWeapon>().attackDamage));
        attackData.Add("Fire", new AttackInfo(GetComponent<BossWeapon>().swordWind.GetComponent<SwordWind>().damage));
        attackData.Add("ThrowPotion", new AttackInfo(GetComponent<BossWeapon>().potion.GetComponent<Potion>().damage));
        attackData.Add("Stab", new AttackInfo(GetComponent<BossWeapon>().enragedAttackDamage));
        attackData.Add("Spell", new AttackInfo(GetComponent<BossWeapon>().explosive.GetComponent<Explosive>().damage));

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
                MoveToPlayer();
                InvokeRepeating("RuleBasedDDA", initalWalkTime, DDAReactionRate);
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
        // phase two
        if (isEnraged)
        {
            // near attack decision
            if (Vector2.Distance(player.transform.position, rb.position) <= nearEnragedAttackRange)
            {
                // each attack has to be executed once to run the DDA system
                bool useDDA = true;

                foreach (var val in attackData.Values)
                {
                    if (val.total == 0)
                    {
                        useDDA = false;
                        break;
                    }
                }

                if (!useDDA)
                {
                    int value = UnityEngine.Random.Range(0, 6);

                    switch (value)
                    {
                        case 0: Fire(); break;
                        case 1: ThrowPotion(); break;
                        case 2: Slash(); break;
                        case 3: Spell(); break;
                        default: Stab(); break;
                    }
                }
                else
                {
                    float totalWeights = 0f;

                    // calculate weights
                    foreach (var val in attackData.Values)
                    {
                        totalWeights += val.GetWeight(player.GetComponent<PlayerHealth>(), bossHealth);
                    }

                    float value = UnityEngine.Random.Range(0f, totalWeights);

                    // log attack info
                    Debug.Log("Slash hit/total: " + attackData["Slash"].hit + "/" + attackData["Slash"].total +
                        "; Fire hit/total: " + attackData["Fire"].hit + "/" + attackData["Fire"].total +
                        "; ThrowPotion hit/total: " + attackData["ThrowPotion"].hit + "/" + attackData["ThrowPotion"].total +
                        "; Stab hit/total: " + attackData["Stab"].hit + "/" + attackData["Stab"].total +
                        "; Spell hit/total: " + attackData["Spell"].hit + "/" + attackData["Spell"].total);
                    Debug.Log("Slash expectation: " + attackData["Slash"].expectation +
                        "; Fire expectation: " + attackData["Fire"].expectation +
                        "; ThrowPotion expectation: " + attackData["ThrowPotion"].expectation +
                        "; Stab expectation: " + attackData["Stab"].expectation +
                        "; Spell expectation: " + attackData["Spell"].expectation);
                    Debug.Log("Slash weight: " + attackData["Slash"].weight +
                        "; Fire weight: " + attackData["Fire"].weight +
                        "; ThrowPotion weight: " + attackData["ThrowPotion"].weight +
                        "; Stab weight: " + attackData["Stab"].weight +
                        "; Spell weight: " + attackData["Spell"].weight);
                    Debug.Log("Random Value: " + value);

                    if (value >= 0f
                        && value < attackData["Slash"].weight)
                    {
                        Slash();
                    }
                    else if (value >= attackData["Slash"].weight
                        && value < attackData["Slash"].weight + attackData["Fire"].weight)
                    {
                        Fire();
                    }
                    else if (value >= attackData["Slash"].weight + attackData["Fire"].weight
                        && value < attackData["Slash"].weight + attackData["Fire"].weight + attackData["ThrowPotion"].weight)
                    {
                        ThrowPotion();
                    }
                    else if (value >= attackData["Slash"].weight + attackData["Fire"].weight + attackData["ThrowPotion"].weight
                        && value < attackData["Slash"].weight + attackData["Fire"].weight + attackData["ThrowPotion"].weight + attackData["Stab"].weight)
                    {
                        Stab();
                    }
                    else
                    {
                        Spell();
                    }
                }
            }
            // far attack decision
            else
            {
                // each attack has to be executed once to run the DDA system
                bool useDDA = true;

                foreach (var item in attackData)
                {
                    if (item.Key != "Slash" && item.Key != "Stab" && item.Value.total == 0)
                    {
                        useDDA = false;
                        break;
                    }
                }

                if (!useDDA)
                {
                    int value = UnityEngine.Random.Range(0, 5);

                    switch (value)
                    {
                        case 0: Fire(); break;
                        case 1: ThrowPotion(); break;
                        case 2: Spell(); break;
                        default: RunToPlayer(); break;
                    }
                }
                else
                {
                    float totalWeights = 0f;

                    // calculate weights
                    foreach (var item in attackData)
                    {
                        if (item.Key != "Slash" && item.Key != "Stab")
                        {
                            totalWeights += item.Value.GetWeight(player.GetComponent<PlayerHealth>(), bossHealth);
                        }
                    }

                    // Assign 1/3 weights to run to player
                    float value = UnityEngine.Random.Range(0f, totalWeights * 1.5f);

                    // log attack info
                    Debug.Log("Fire hit/total: " + attackData["Fire"].hit + "/" + attackData["Fire"].total +
                        "; ThrowPotion hit/total: " + attackData["ThrowPotion"].hit + "/" + attackData["ThrowPotion"].total +
                        "; Spell hit/total: " + attackData["Spell"].hit + "/" + attackData["Spell"].total);
                    Debug.Log("Fire expectation: " + attackData["Fire"].expectation +
                        "; ThrowPotion expectation: " + attackData["ThrowPotion"].expectation +
                        "; Spell expectation: " + attackData["Spell"].expectation);
                    Debug.Log("Fire weight: " + attackData["Fire"].weight +
                        "; ThrowPotion weight: " + attackData["ThrowPotion"].weight +
                        "; Spell weight: " + attackData["Spell"].weight);
                    Debug.Log("Random Value: " + value);

                    if (value >= 0f
                        && value < attackData["Fire"].weight)
                    {
                        Fire();
                    }
                    else if (value >= attackData["Fire"].weight
                        && value < attackData["Fire"].weight + attackData["ThrowPotion"].weight)
                    {
                        ThrowPotion();
                    }
                    else if (value >= attackData["Fire"].weight + attackData["ThrowPotion"].weight
                        && value < attackData["Fire"].weight + attackData["ThrowPotion"].weight + attackData["Spell"].weight)
                    {
                        Spell();
                    }
                    else
                    {
                        RunToPlayer();
                    }
                }
            }
        }
        else
        {
            // enrage
            if (bossHealth.health <= dangerHealth)
            {
                Enrage();
            }
            // phase one
            else
            {
                // near attack decision
                if (Vector2.Distance(player.transform.position, rb.position) <= nearAttackRange)
                {
                    // each attack has to be executed once to run the DDA system
                    bool useDDA = true;

                    foreach (var item in attackData)
                    {
                        if (item.Key != "Stab" && item.Key != "Spell" && item.Value.total == 0)
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
                        foreach (var item in attackData)
                        {
                            if (item.Key != "Stab" && item.Key != "Spell")
                            {
                                totalWeights += item.Value.GetWeight(player.GetComponent<PlayerHealth>(), bossHealth);
                            }
                        }

                        float value = UnityEngine.Random.Range(0f, totalWeights);

                        // log attack info
                        Debug.Log("Slash hit/total: " + attackData["Slash"].hit + "/" + attackData["Slash"].total +
                            "; Fire hit/total: " + attackData["Fire"].hit + "/" + attackData["Fire"].total +
                            "; ThrowPotion hit/total: " + attackData["ThrowPotion"].hit + "/" + attackData["ThrowPotion"].total);
                        Debug.Log("Slash expectation: " + attackData["Slash"].expectation +
                            "; Fire expectation: " + attackData["Fire"].expectation +
                            "; ThrowPotion expectation: " + attackData["ThrowPotion"].expectation);
                        Debug.Log("Slash weight: " + attackData["Slash"].weight +
                            "; Fire weight: " + attackData["Fire"].weight +
                            "; ThrowPotion weight: " + attackData["ThrowPotion"].weight);
                        Debug.Log("Random Value: " + value);

                        if (value >= 0f
                            && value < attackData["Slash"].weight)
                        {
                            Slash();
                        }
                        else if (value >= attackData["Slash"].weight
                            && value < attackData["Slash"].weight + attackData["Fire"].weight)
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
                        if ((item.Key == "Fire" || item.Key == "ThrowPotion") && item.Value.total == 0)
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
                            if (item.Key == "Fire" || item.Key == "ThrowPotion")
                            {
                                totalWeights += item.Value.GetWeight(player.GetComponent<PlayerHealth>(), bossHealth); ;
                            }
                        }

                        // assign 1/3 weight value to moveToPlayer when making far attack decision
                        float value = UnityEngine.Random.Range(0f, totalWeights * 1.5f);

                        // log attack info
                        Debug.Log("Fire hit/total: " + attackData["Fire"].hit + "/" + attackData["Fire"].total +
                            "; ThrowPotion hit/total: " + attackData["ThrowPotion"].hit + "/" + attackData["ThrowPotion"].total);
                        Debug.Log("Fire expectation: " + attackData["Fire"].expectation +
                            "; ThrowPotion expectation: " + attackData["ThrowPotion"].expectation);
                        Debug.Log("Fire weight: " + attackData["Fire"].weight +
                            "; ThrowPotion weight: " + attackData["ThrowPotion"].weight);
                        Debug.Log("Random Value: " + value);

                        if (value >= 0f
                            && value < attackData["Fire"].weight)
                        {
                            Fire();
                        }
                        else if (value >= attackData["Fire"].weight
                            && value < totalWeights)
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
        blackboard["health"] = bossHealth.health;
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
        Node rndSel1 = new RandomSelector(new Action(() => Spell()),
            new Action(() => Fire()), new Action(() => Fire()),
            new Action(() => ThrowPotion()), new Action(() => ThrowPotion()),
            new Action(() => Slash()), new Action(() => Slash()),
            new Action(() => Stab()), new Action(() => Stab()), new Action(() => Stab()));
        // Attack the player if player is in attack range
        Node bb = new BlackboardCondition("playerDistance", Operator.IS_SMALLER_OR_EQUAL, nearEnragedAttackRange, Stops.IMMEDIATE_RESTART, rndSel1);

        // Select between far attacks and run to the player
        Node rndSel2 = new RandomSelector(new Action(() => Spell()),
            new Action(() => Fire()), new Action(() => Fire()),
            new Action(() => ThrowPotion()), new Action(() => ThrowPotion()),
            new Action(() => RunToPlayer()), new Action(() => RunToPlayer()), new Action(() => RunToPlayer()));

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

    private void Spell()
    {
        GetComponent<Animator>().SetTrigger("Spell");
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

        public float GetWeight(PlayerHealth playerHealth, BossHealth bossHealth)
        {
            weight = 1.0f / Mathf.Abs((playerHealth.health - expectation) / playerHealth.maxHealth - (bossHealth.health + bossHealth.defense) / (bossHealth.maxHealth + bossHealth.maxDefense));
            return weight;
        }
    }
}