using UnityEngine;
using NPBehave;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class Boss : MonoBehaviour
{
    public GameObject player, bgm;
    public Rigidbody2D rb;

    public bool isFlipped;
    public bool isEnraged;

    public float nearAttackRange, nearEnragedAttackRange, farAttackRange;
    public float maxWaitingTime, minWaitingTime;

    public int dangerHealth;

    public string title;

    public SpriteRenderer helmet, Shield;
    public TextMeshProUGUI bossName;

    public Root tree; // the boss's behaviour tree

    private Blackboard blackboard; // the boss's behaviour blackboard

    private BossHealth bossHealth; // reference to boss's health script, used by the AI to deal with health.

    private Dictionary<string, ActionData> actionDict; // action data

    // valid actions for each state of the boss
    private readonly string[] phaseOneFarActions = { "Move", "Fire", "ThrowPotion" }, phaseOneNearActions = { "Slash", "Fire", "ThrowPotion" },
        phaseTwoFarActions = { "Fire", "ThrowPotion", "Summon" }, phaseTwoNearActions = { "Slash", "Fire", "ThrowPotion", "Stab", "Summon" };
    private enum State
    {
        PhaseOneFar,
        PhaseOneNear,
        PhaseTwoFar,
        PhaseTwoNear
    }

    private void Awake()
    {
        title = MainMenu.bossTitle;

        MainMenu.bossFights[MainMenu.index].isRevealed = true;
    }

    private void Start()
    {
        helmet.color = MainMenu.bossColor;
        Shield.color = MainMenu.bossColor;
        bossName.color = MainMenu.bossColor;
        bossName.text = title;

        rb = GetComponent<Rigidbody2D>();
        bossHealth = GetComponent<BossHealth>();

        // action dictionary initialization
        actionDict = new Dictionary<string, ActionData>
        {
            { "Move", new ActionData(0) },
            { "Slash", new ActionData(GetComponent<BossWeapon>().attackDamage) },
            { "Fire", new ActionData(GetComponent<BossWeapon>().shockWave.GetComponent<ShockWave>().damage) },
            { "ThrowPotion", new ActionData(GetComponent<BossWeapon>().potion.GetComponent<Potion>().damage) },
            { "Stab", new ActionData(GetComponent<BossWeapon>().enragedAttackDamage) },
            { "Summon", new ActionData(GetComponent<BossWeapon>().orb.GetComponent<Orb>().damage) }
        };

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
            title == "Grey Knight" ? new Action(() => RandomAction(State.PhaseOneFar)) : new Action(() => DDAAction(State.PhaseOneFar)));

        // Move to the player if the player is not in far attack range
        Node sel = new Selector(bb1, new Action(() => MoveToPlayer()));

        // Execute near attacks the player if the player is in near attack range
        Node bb2 = new BlackboardCondition("playerDistance", Operator.IS_SMALLER_OR_EQUAL, nearAttackRange, Stops.IMMEDIATE_RESTART,
             title == "Grey Knight" ? new Action(() => RandomAction(State.PhaseOneNear)) : new Action(() => DDAAction(State.PhaseOneNear)));

        // Look at the player at first, then wait for 1 second, let the last state continue for a while
        return new Sequence(new Action(() => LookAtPlayer()), title == "Grey Knight" ? new Wait(1.25f) : new Wait(DDAWait), new Selector(bb2, sel));
    }

    // always run to the player
    private Node PhaseTwoBehaviour()
    {
        //  Select between near attacks if player is in near attack range
        Node bb = new BlackboardCondition("playerDistance", Operator.IS_SMALLER_OR_EQUAL, nearEnragedAttackRange, Stops.IMMEDIATE_RESTART,
            title == "Grey Knight" ? new Action(() => RandomAction(State.PhaseTwoNear)) : new Action(() => DDAAction(State.PhaseTwoNear)));

        // Look at the player first and wait for 1 second, then check attack range, choose far attacks if the player is not in near attack range
        Node seq = new Sequence(new Action(() => LookAtPlayer()), title == "Grey Knight" ? new Wait(0.75f) : new Wait(DDAWait),
            new Selector(bb, title == "Grey Knight" ? new Action(() => RandomAction(State.PhaseTwoFar)) : new Action(() => DDAAction(State.PhaseTwoFar))));

        // Enter phase two when enraged
        return new BlackboardCondition("isEnraged", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, seq);
    }

    private float DDAWait()
    {
        return (maxWaitingTime + minWaitingTime) / 2f - (maxWaitingTime - minWaitingTime) / 2f *
            (player.GetComponent<PlayerHealth>().health / (float)player.GetComponent<PlayerHealth>().maxHealth - (bossHealth.health + bossHealth.shield) / (float)(bossHealth.maxHealth + bossHealth.maxShield));
    }

    // provide action selected by DDA system
    private void DDAAction(State state)
    {
        // set each action's validity according to the boss's current state
        SetActionValidity(state);

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
        if (title == "Golden Knight")
        {
            // low probability of random selection
            int breaker = UnityEngine.Random.Range(0, 5);
            if (breaker == 0)
            {
                RandomAction(state);
                return;
            }

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
        if (title == "Red Knight")
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
        // Rank selection DDA
        else
        {
            Dictionary<string, float> actionFit = new();

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

        // random selection weighted by fitness
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

    private void SetActionValidity(State state)
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
        return state switch
        {
            State.PhaseOneFar => phaseOneFarActions,
            State.PhaseOneNear => phaseOneNearActions,
            State.PhaseTwoFar => phaseTwoFarActions,
            State.PhaseTwoNear => phaseTwoNearActions,
            _ => new string[] { },
        };
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
}