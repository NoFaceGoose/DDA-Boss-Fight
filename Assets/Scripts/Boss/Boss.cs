using UnityEngine;
using NPBehave;
using System.Collections.Generic;

public class Boss : MonoBehaviour
{
    public GameObject player;
    public Rigidbody2D rb;

    public bool isFlipped;
    public bool isEnraged;

    public float nearAttackRange, nearEnragedAttackRange, farAttackRange;
    public float initalWalkTime;
    public float DDAReactionRate;

    public int dangerHealth;
    public int AI;

    public Root tree; // the boss's behaviour tree

    private Blackboard blackboard; // the boss's behaviour blackboard

    private BossHealth bossHealth; // reference to boss's health script, used by the AI to deal with health.

    private Dictionary<string, ActionData> actionDict; // action data

    // valid actions for each case
    private readonly string[] phaseOneFarActions = { "Move", "Fire", "ThrowPotion" };
    private readonly string[] phaseOneNearActions = { "Slash", "Fire", "ThrowPotion" };
    private readonly string[] phaseTwoFarActions = { "Fire", "ThrowPotion", "Spell" };
    private readonly string[] phaseTwoNearActions = { "Slash", "Fire", "ThrowPotion", "Stab", "Spell" };

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bossHealth = GetComponent<BossHealth>();

        actionDict = new Dictionary<string, ActionData>();
        actionDict.Add("Move", new ActionData(0));
        actionDict["Move"].count++;
        actionDict.Add("Slash", new ActionData(GetComponent<BossWeapon>().attackDamage));
        actionDict.Add("Fire", new ActionData(GetComponent<BossWeapon>().swordWind.GetComponent<SwordWind>().damage));
        actionDict.Add("ThrowPotion", new ActionData(GetComponent<BossWeapon>().potion.GetComponent<Potion>().damage));
        actionDict.Add("Stab", new ActionData(GetComponent<BossWeapon>().enragedAttackDamage));
        actionDict.Add("Spell", new ActionData(GetComponent<BossWeapon>().explosive.GetComponent<Explosive>().damage));

        switch (AI)
        {
            // Default BT
            case 0:
                // start behaviour tree
                tree = BehaviourTree();
                blackboard = tree.Blackboard;

#if UNITY_EDITOR
                Debugger debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
                debugger.BehaviorTree = tree;
#endif
                break;

            // DDA
            default:
                MoveToPlayer();
                InvokeRepeating("DDA", initalWalkTime, DDAReactionRate);
                break;
        }
    }

    private void FixedUpdate()
    {
        if (AI != 0)
        {
            LookAtPlayer();
        }
    }

    // AI 1 or 2
    private void DDA()
    {
        // phase two
        if (isEnraged)
        {
            // near attack decision
            if (Vector2.Distance(player.transform.position, rb.position) <= nearEnragedAttackRange)
            {
                // each attack has to be executed once to run the DDA system
                bool useDDA = true;

                foreach (var val in actionDict.Values)
                {
                    if (val.count == 0)
                    {
                        useDDA = false;
                        break;
                    }
                }

                // disable DDA until each attack is executed once
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
                // enable DDA
                else
                {
                    // random selection with weights
                    if (AI == 1)
                    {
                        float totalFitness = 0f;

                        // calculate fitnesses
                        foreach (var val in actionDict.Values)
                        {
                            totalFitness += val.GetFitness(player.GetComponent<PlayerHealth>(), bossHealth);
                        }

                        float value = UnityEngine.Random.Range(0f, totalFitness);

                        if (value >= 0f
                            && value < actionDict["Slash"].fitness)
                        {
                            Slash();
                        }
                        else if (value >= actionDict["Slash"].fitness
                            && value < actionDict["Slash"].fitness + actionDict["Fire"].fitness)
                        {
                            Fire();
                        }
                        else if (value >= actionDict["Slash"].fitness + actionDict["Fire"].fitness
                            && value < actionDict["Slash"].fitness + actionDict["Fire"].fitness + actionDict["ThrowPotion"].fitness)
                        {
                            ThrowPotion();
                        }
                        else if (value >= actionDict["Slash"].fitness + actionDict["Fire"].fitness + actionDict["ThrowPotion"].fitness
                            && value < actionDict["Slash"].fitness + actionDict["Fire"].fitness + actionDict["ThrowPotion"].fitness + actionDict["Stab"].fitness)
                        {
                            Stab();
                        }
                        else
                        {
                            Spell();
                        }
                    }
                    // choose the attack with the max fitness
                    else
                    {
                        // get the max fitness
                        float maxFitness = 0f;
                        string action = "";

                        foreach (var item in actionDict)
                        {
                            float temp = maxFitness;
                            maxFitness = Mathf.Max(maxFitness, item.Value.GetFitness(player.GetComponent<PlayerHealth>(), bossHealth));

                            // Debug.Log(item.Key + ": " + item.Value.weight);

                            if (temp < maxFitness)
                            {
                                action = item.Key;
                            }
                        }

                        ExecuteAction(action);
                    }
                }
            }
            // far attack decision
            else
            {
                // each attack has to be executed once to run the DDA system
                bool useDDA = true;

                foreach (var item in actionDict)
                {
                    if (item.Key != "Slash" && item.Key != "Stab" && item.Value.count == 0)
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
                        default: break;
                    }
                }
                else
                {
                    // Assign 1/3 weights to run to player
                    int rndValue = UnityEngine.Random.Range(0, 3);

                    if (rndValue != 0)
                    {
                        // random selection with weights
                        if (AI == 1)
                        {
                            float totalFitness = 0f;

                            // calculate fitnesses
                            foreach (var item in actionDict)
                            {
                                if (item.Key != "Slash" && item.Key != "Stab")
                                {
                                    totalFitness += item.Value.GetFitness(player.GetComponent<PlayerHealth>(), bossHealth);
                                }
                            }

                            // Assign 1/3 weights to run to player
                            float value = UnityEngine.Random.Range(0f, totalFitness);

                            if (value >= 0f
                                && value < actionDict["Fire"].fitness)
                            {
                                Fire();
                            }
                            else if (value >= actionDict["Fire"].fitness
                                && value < actionDict["Fire"].fitness + actionDict["ThrowPotion"].fitness)
                            {
                                ThrowPotion();
                            }
                            else
                            {
                                Spell();
                            }
                        }
                        // choose the attack with the max fitness
                        else
                        {
                            // get the max fitness
                            float maxFitness = 0f;
                            string action = "";

                            foreach (var item in actionDict)
                            {
                                if (item.Key != "Slash" && item.Key != "Stab")
                                {
                                    float temp = maxFitness;
                                    maxFitness = Mathf.Max(maxFitness, item.Value.GetFitness(player.GetComponent<PlayerHealth>(), bossHealth));

                                    if (temp < maxFitness)
                                    {
                                        action = item.Key;
                                    }
                                }
                            }

                            ExecuteAction(action);
                        }
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

                    foreach (var item in actionDict)
                    {
                        if (item.Key != "Stab" && item.Key != "Spell" && item.Value.count == 0)
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
                        // random selection with weights
                        if (AI == 1)
                        {
                            float totalFitness = 0f;

                            // calculate fitnesses
                            foreach (var item in actionDict)
                            {
                                if (item.Key != "Stab" && item.Key != "Spell")
                                {
                                    totalFitness += item.Value.GetFitness(player.GetComponent<PlayerHealth>(), bossHealth);
                                }
                            }

                            float value = UnityEngine.Random.Range(0f, totalFitness);

                            if (value >= 0f
                                && value < actionDict["Slash"].fitness)
                            {
                                Slash();
                            }
                            else if (value >= actionDict["Slash"].fitness
                                && value < actionDict["Slash"].fitness + actionDict["Fire"].fitness)
                            {
                                Fire();
                            }
                            else
                            {
                                ThrowPotion();
                            }
                        }
                        // choose the attack with the max fitness
                        else
                        {
                            // get the max fitness
                            float maxFitness = 0f;
                            string action = "";

                            foreach (var item in actionDict)
                            {
                                if (item.Key != "Stab" && item.Key != "Spell")
                                {
                                    float temp = maxFitness;
                                    maxFitness = Mathf.Max(maxFitness, item.Value.GetFitness(player.GetComponent<PlayerHealth>(), bossHealth));

                                    if (temp < maxFitness)
                                    {
                                        action = item.Key;
                                    }
                                }
                            }

                            ExecuteAction(action);
                        }
                    }
                }

                // far attack decision
                else if (Vector2.Distance(player.transform.position, rb.position) <= farAttackRange)
                {
                    // each attack has to be executed once to run the DDA system
                    bool useDDA = true;

                    foreach (var item in actionDict)
                    {
                        if ((item.Key == "Fire" || item.Key == "ThrowPotion") && item.Value.count == 0)
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
                        // Assign 1/3 weights to run to player
                        int rndValue = UnityEngine.Random.Range(0, 3);

                        if (rndValue != 0)
                        {
                            // random selection with weights
                            if (AI == 1)
                            {
                                float totalFitness = 0f;

                                // calculate fitnesses
                                foreach (var item in actionDict)
                                {
                                    if (item.Key == "Fire" || item.Key == "ThrowPotion")
                                    {
                                        totalFitness += item.Value.GetFitness(player.GetComponent<PlayerHealth>(), bossHealth); ;
                                    }
                                }

                                // assign 1/3 weight value to moveToPlayer when making far attack decision
                                float value = UnityEngine.Random.Range(0f, totalFitness);

                                if (value >= 0f && value < actionDict["Fire"].fitness)
                                {
                                    Fire();
                                }
                                else
                                {
                                    ThrowPotion();
                                }
                            }
                            // choose the attack with the max fitness
                            else
                            {
                                // get the max fitness
                                float maxFitness = 0f;
                                string action = "";

                                foreach (var item in actionDict)
                                {
                                    if (item.Key == "Fire" || item.Key == "ThrowPotion")
                                    {
                                        float temp = maxFitness;
                                        maxFitness = Mathf.Max(maxFitness, item.Value.GetFitness(player.GetComponent<PlayerHealth>(), bossHealth));

                                        if (temp < maxFitness)
                                        {
                                            action = item.Key;
                                        }
                                    }
                                }

                                ExecuteAction(action);
                            }
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
            new Action(() => ThrowPotion()), new Action(() => ThrowPotion()));

        // Look at the player first, then check attacks
        Node seq = new Sequence(new Action(() => LookAtPlayer()), new Wait(0.5f), new Selector(bb, rndSel2));

        // Enter phase two when enraged
        return new BlackboardCondition("isEnraged", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, seq);
    }

    private void DDAAction(State state)
    {
        // set actions validity according to the boss's current state
        setActionValidity(state);

        // randomly select an action if any action in the scope have not been chosen once
        foreach (var val in actionDict.Values)
        {
            if (val.count == 0)
            {
                ExecuteRandomAction(state);
                return;
            }
        }

        // Roulette selection DDA
        if (AI == 1)
        {
            float totalFitness = 0f;

            // calculate each action's fitness
            foreach (var val in actionDict.Values)
            {
                if (val.isValid)
                {
                    totalFitness += val.GetFitness(player.GetComponent<PlayerHealth>(), bossHealth);
                }
            }

            float value = UnityEngine.Random.Range(0f, totalFitness);

            float lowerBound = 0f, upperBound = 0f;

            foreach (var item in actionDict)
            {
                if (item.Value.isValid)
                {
                    upperBound += item.Value.fitness;

                    if (value >= lowerBound && value < upperBound)
                    {
                        ExecuteAction(item.Key);
                        return;
                    }
                    else
                    {
                        lowerBound = upperBound;
                    }

                }
            }
        }
        // Ranking selection DDA
        else
        {

        }
    }

    private void setActionValidity(State state)
    {
        string[] validActions = { };

        switch (state)
        {
            case State.PhaseOneFar: validActions = phaseOneFarActions; break;
            case State.PhaseOneNear: validActions = phaseOneNearActions; break;
            case State.PhaseTwoFar: validActions = phaseTwoFarActions; break;
            case State.PhaseTwoNear: validActions = phaseTwoNearActions; break;
            default: break;
        }

        foreach (var item in actionDict)
        {
            if (System.Array.Exists(validActions, element => element == item.Key))
            {
                item.Value.isValid = true;
            }
            else
            {
                item.Value.isValid = false;
            }
        }
    }

    private void ExecuteRandomAction(State state)
    {

    }

    private void ExecuteAction(string key)
    {
        switch (key)
        {
            case "Move": MoveToPlayer(); break;
            case "Slash": Slash(); break;
            case "Fire": Fire(); break;
            case "ThrowPotion": ThrowPotion(); break;
            case "Stab": Stab(); break;
            case "Spell": Spell(); break;
            default: break;
        }
    }

    public void UpdateAction(string attackType, bool hit = false)
    {
        if (hit)
        {
            actionDict[attackType].hit++;
        }
        else
        {
            actionDict[attackType].count++;
        }

        actionDict[attackType].UpdateExpectedDamage();
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

    private class ActionData
    {
        public int count;
        public int hit;
        public int damage;
        public float expectedDamage;
        public float fitness;
        public bool isValid;

        public ActionData(int damage)
        {
            count = 0;
            hit = 0;
            this.damage = damage;
            expectedDamage = 0f;
            fitness = 0;
            isValid = true;
        }

        // update the action's expected damage
        public void UpdateExpectedDamage()
        {
            expectedDamage = ((float)hit / count) * damage;
        }

        // calculate and return the action's fitness
        public float GetFitness(PlayerHealth playerHealth, BossHealth bossHealth)
        {
            fitness = 1f - Mathf.Sqrt(Mathf.Abs((float)(playerHealth.health - expectedDamage) / (float)playerHealth.maxHealth - (float)(bossHealth.health + bossHealth.defense) / (float)(bossHealth.maxHealth + bossHealth.maxDefense)));
            return fitness;
        }
    }

    private enum State
    {
        PhaseOneFar,
        PhaseOneNear,
        PhaseTwoFar,
        PhaseTwoNear
    }
}