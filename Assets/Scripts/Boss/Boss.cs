using UnityEngine;
using NPBehave;
using System.Collections.Generic;
using System.Linq;

public class Boss : MonoBehaviour
{
    public GameObject player, bgm;
    public Rigidbody2D rb;

    public bool isFlipped;
    public bool isEnraged;

    public float nearAttackRange, nearEnragedAttackRange, farAttackRange;
    public float maxWaitingTime, minWaitingTime;

    public int dangerHealth;
    public int AI;

    public Root tree; // the boss's behaviour tree

    private Blackboard blackboard; // the boss's behaviour blackboard

    private BossHealth bossHealth; // reference to boss's health script, used by the AI to deal with health.

    private Dictionary<string, ActionData> actionDict; // action data

    // valid actions for each state of the boss
    private readonly string[] phaseOneFarActions = { "Move", "Fire", "ThrowPotion" }, phaseOneNearActions = { "Slash", "Fire", "ThrowPotion" },
        phaseTwoFarActions = { "Fire", "ThrowPotion", "Summon" }, phaseTwoNearActions = { "Slash", "Fire", "ThrowPotion", "Stab", "Summon" };

    private void Awake()
    {
        AI = MainMenu.bossAI;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bossHealth = GetComponent<BossHealth>();

        // action dictionary initialization
        actionDict = new Dictionary<string, ActionData>();
        actionDict.Add("Move", new ActionData(0));
        actionDict.Add("Slash", new ActionData(GetComponent<BossWeapon>().attackDamage));
        actionDict.Add("Fire", new ActionData(GetComponent<BossWeapon>().shockWave.GetComponent<ShockWave>().damage));
        actionDict.Add("ThrowPotion", new ActionData(GetComponent<BossWeapon>().potion.GetComponent<Potion>().damage));
        actionDict.Add("Stab", new ActionData(GetComponent<BossWeapon>().enragedAttackDamage));
        actionDict.Add("Summon", new ActionData(GetComponent<BossWeapon>().orb.GetComponent<Orb>().damage));

        // start behaviour tree
        tree = BehaviourTree();
        blackboard = tree.Blackboard;

#if UNITY_EDITOR
        Debugger debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
        debugger.BehaviorTree = tree;
#endif
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
        // Execute far actions if the player is in far attack range
        Node bb1 = new BlackboardCondition("playerDistance", Operator.IS_SMALLER_OR_EQUAL, farAttackRange, Stops.IMMEDIATE_RESTART,
            AI == 0 ? new Action(() => RandomAction(State.PhaseOneFar)) : new Action(() => DDAAction(State.PhaseOneFar)));

        // Move to the player if the player is not in far attack range
        Node sel = new Selector(bb1, new Action(() => MoveToPlayer()));

        // Execute near attacks the player if the player is in near attack range
        Node bb2 = new BlackboardCondition("playerDistance", Operator.IS_SMALLER_OR_EQUAL, nearAttackRange, Stops.IMMEDIATE_RESTART,
             AI == 0 ? new Action(() => RandomAction(State.PhaseOneNear)) : new Action(() => DDAAction(State.PhaseOneNear)));

        // Look at the player at first, then wait for 1 second, let the last state continue for a while
        return new Sequence(new Action(() => LookAtPlayer()), AI == 0 ? new Wait(0.75f) : new Wait(DDAWait), new Selector(bb2, sel));
    }

    // always run to the player
    private Node PhaseTwoBehaviour()
    {
        //  Select between near attacks if player is in near attack range
        Node bb = new BlackboardCondition("playerDistance", Operator.IS_SMALLER_OR_EQUAL, nearEnragedAttackRange, Stops.IMMEDIATE_RESTART,
            AI == 0 ? new Action(() => RandomAction(State.PhaseTwoNear)) : new Action(() => DDAAction(State.PhaseTwoNear)));

        // Look at the player first and wait for 1 second, then check attack range, choose far attacks if the player is not in near attack range
        Node seq = new Sequence(new Action(() => LookAtPlayer()), AI == 0 ? new Wait(0.5f) : new Wait(DDAWait),
            new Selector(bb, AI == 0 ? new Action(() => RandomAction(State.PhaseTwoFar)) : new Action(() => DDAAction(State.PhaseTwoFar))));

        // Enter phase two when enraged
        return new BlackboardCondition("isEnraged", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, seq);
    }

    private float DDAWait()
    {
        return (maxWaitingTime + minWaitingTime) / 2f - (maxWaitingTime - minWaitingTime) / 2f *
            ((float)player.GetComponent<PlayerHealth>().health / (float)player.GetComponent<PlayerHealth>().maxHealth - (float)(bossHealth.health + bossHealth.shield) / (float)(bossHealth.maxHealth + bossHealth.maxShield));
    }

    // provide action selected by DDA system
    private void DDAAction(State state)
    {
        // set each action's validity according to the boss's current state
        setActionValidity(state);

        // execute the first action which has not been done once
        foreach (var item in actionDict)
        {
            if (item.Value.isValid && item.Value.count == 0)
            {
                ExecuteAction(item.Key);
                return;
            }
        }

        // execute the fittest action 
        if (AI == 1)
        {
            float maxFitness = 0f;
            string action = null;

            // calculate each action's fitness
            foreach (var item in actionDict)
            {
                if (item.Value.isValid)
                {
                    item.Value.UpdateFitness(player.GetComponent<PlayerHealth>(), bossHealth);
                    if (maxFitness <= item.Value.fitness)
                    {
                        maxFitness = item.Value.fitness;
                        action = item.Key;
                    }
                }
            }

            ExecuteAction(action);
            return;
        }

        // sum up each action's fitness for random float generation
        float sumFitness = 0f;

        // Roulette selection DDA
        if (AI == 2)
        {
            // calculate each action's fitness
            foreach (var val in actionDict.Values)
            {
                if (val.isValid)
                {
                    sumFitness += val.UpdateFitness(player.GetComponent<PlayerHealth>(), bossHealth);
                }
            }
        }
        // Ranking selection DDA
        else
        {
            Dictionary<string, float> actionFit = new Dictionary<string, float>();

            // calculate each action's fitness
            foreach (var val in actionDict.Values)
            {
                if (val.isValid)
                {
                    val.UpdateFitness(player.GetComponent<PlayerHealth>(), bossHealth);
                }
            }

            actionDict.OrderBy(item => item.Value.fitness);

            float tempFitness = 1f;

            foreach (var val in actionDict.Values)
            {
                if (val.isValid)
                {
                    val.fitness = (tempFitness /= 2f);
                    sumFitness += tempFitness;
                }
            }
        }

        // random selection with by the fitness
        float value = UnityEngine.Random.Range(0f, sumFitness);

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

    private void setActionValidity(State state)
    {
        string[] validActions = GetValidActions(state);

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

    private void RandomAction(State state)
    {
        string[] validActions = GetValidActions(state);

        int index = UnityEngine.Random.Range(0, validActions.Length);

        ExecuteAction(validActions[index]);
    }

    // get valid actions according to boss's current state
    private string[] GetValidActions(State state)
    {
        switch (state)
        {
            case State.PhaseOneFar: return phaseOneFarActions;
            case State.PhaseOneNear: return phaseOneNearActions;
            case State.PhaseTwoFar: return phaseTwoFarActions;
            case State.PhaseTwoNear: return phaseTwoNearActions;
            default: return new string[] { };
        }
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
            case "Summon": Summon(); break;
            default: break;
        }
    }

    public void UpdateAction(string actionName, bool hit = false)
    {
        if (hit)
        {
            actionDict[actionName].hit++;
        }
        else
        {
            actionDict[actionName].count++;
        }

        actionDict[actionName].UpdateExpectedDamage();
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
        UpdateAction("Move");
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

    private void Summon()
    {
        GetComponent<Animator>().SetTrigger("Summon");
    }

    private class ActionData
    {
        public int count, hit, damage;
        public float expectedDamage, fitness;
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
        public float UpdateFitness(PlayerHealth playerHealth, BossHealth bossHealth)
        {
            float playerExpectedHP = playerHealth.health - expectedDamage >= 0 ? (float)(playerHealth.health - expectedDamage) : 0f;
            fitness = 1f - Mathf.Sqrt(Mathf.Abs(playerExpectedHP / (float)playerHealth.maxHealth - (float)(bossHealth.health + bossHealth.shield) / (float)(bossHealth.maxHealth + bossHealth.maxShield)));
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