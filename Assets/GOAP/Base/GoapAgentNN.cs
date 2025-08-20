using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[RequireComponent(typeof(NavMeshAgent))]
public class GoapAgentNN : AbstractNeuralNetworkAgent, IGoapAgent
{
    public NavMeshAgent NavMeshAgent { get; private set; }
    private GoapPlanner planner;
    private Queue<GoapAction> currentPlan;
    private GoapAction currentAction;

    [Tooltip("The set of actions this agent can perform. Assign action assets here.")]
    [SerializeField] private List<GoapAction> availableActions;

    public GoapGoal CurrentGoal { get; private set; }
    private float planCooldown = 0f;
    private const float PLAN_RATE = 1f;
    private Vector3 startingPosition;


    [SerializeField] private float MaxHealth = 100;
    [SerializeField] private float currenthealth = 100;
    private bool IsAlive = true;

    private Transform closestEnemyLocation;
    private Transform closestPotionLocation;

    public List<GoapAction> GetAvailableActions() => availableActions;

    void Awake()
    {
        NavMeshAgent = GetComponent<NavMeshAgent>();
        planner = new GoapPlanner();
        currentPlan = new Queue<GoapAction>();
        startingPosition = transform.position;
        InputValues = new float[5];
        OutputValues = new float[5];
        currenthealth = MaxHealth;
    }

    public void CalculateNN()
    {
        SetInput(new float[] { GetHealth(), AsessThreats(), GetDistanceToEnemy(),GetHealingPotionAmount(), DistanceToHealing()});
        Evaluate();
        
    }

    public float GetSpecificOutput(string Name)
    {
        if (Name == EnemyDataConsts.SEARCHING) return OutputValues[0];
        else if (Name == EnemyDataConsts.CHASETARGET) return OutputValues[1];
        else if (Name == EnemyDataConsts.ATTACKTARGET) return OutputValues[2];
        else if (Name == EnemyDataConsts.RETREAT) return OutputValues[3];
        else if (Name == EnemyDataConsts.GETHEAL) return OutputValues[4];
        return 0;
    }

    private float DistanceToHealing()
    {
        if (closestPotionLocation == null) return Mathf.Infinity;
        return (closestPotionLocation.position - transform.position).magnitude;
    }

    private float GetHealingPotionAmount()
    {
        int count = 0;
        Collider[] vision = Physics.OverlapSphere(transform.position, 15);

        foreach (Collider c in vision)
        {
            if (c.tag == "Potion")
            {
                if ((c.transform.position - transform.position).magnitude < (closestPotionLocation.position - transform.position).magnitude)
                {
                    closestPotionLocation = c.transform;
                }
                count++;
            }
        }
        return count;
    }

    private float GetDistanceToEnemy()
    {
        if (closestEnemyLocation == null) return Mathf.Infinity;
        return (closestEnemyLocation.position - transform.position).magnitude;
    }

    private float AsessThreats()
    {
        int count = 0;
        Collider[] vision = Physics.OverlapSphere(transform.position,15);

        foreach (Collider c in vision)
        {
            if (c.tag == "Falcon" || c.tag == "Villager" || c.tag == "Mage")
            {
                if ((c.transform.position-transform.position).magnitude < (closestEnemyLocation.position - transform.position).magnitude)
                {
                    closestEnemyLocation = c.transform;
                }
                count++;
            }
        }
        return count;
    }

    void Update()
    {

        if (planCooldown > 0) planCooldown -= Time.deltaTime;

        if (CurrentGoal == null && planCooldown <= 0)
        {
            CurrentGoal = TaskManager.Instance.RequestTask(this);
            if (CurrentGoal != null)
            {
                Debug.Log($"[{gameObject.name}] received new goal: {CurrentGoal.GoalName}");
                FindPlan();
            }
            else
            {
                NavMeshAgent.SetDestination(startingPosition);
            }
            planCooldown = PLAN_RATE;
        }

        if (currentAction != null) ExecutePlan();
    }

    public void FindPlan()
    {
        if (CurrentGoal == null) return;

        var goalState = CurrentGoal.GetGoalState();
        var worldState = WorldState.Instance.GetWorldState();
        var currentState = new HashSet<KeyValuePair<string, object>>(worldState);

        foreach (var action in availableActions)
        {
            action.DoReset();
        }

        var usableActions = availableActions.Where(a => a.CheckProceduralPrecondition(this)).ToList();

        Queue<GoapAction> plan = planner.Plan(this, usableActions, currentState, goalState);

        if (plan != null && plan.Count > 0)
        {
            currentPlan = plan;
            currentAction = currentPlan.Peek();

            if (!currentAction.SetupAction(this))
            {
                AbortPlan($"Failed to setup first action: {currentAction.ActionName}");
            }
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] Could not find a plan for goal '{CurrentGoal.GoalName}'.");
            TaskManager.Instance.FailTask(CurrentGoal);
            CurrentGoal = null;
        }
    }

    public void ExecutePlan()
    {
        if (currentAction == null) return;

        if (currentAction.IsDone())
        {
            Destroy(currentPlan.Dequeue());

            if (currentPlan.Count > 0)
            {
                currentAction = currentPlan.Peek();

                if (!currentAction.SetupAction(this))
                {
                    AbortPlan($"Failed to setup action: {currentAction.ActionName}");
                    return; 
                }
            }
            else
            {
                Debug.Log($"<color=cyan>[{gameObject.name}] finished plan for goal: {CurrentGoal.GoalName}.</color>");
                TaskManager.Instance.CompleteTask(CurrentGoal);
                CurrentGoal = null;
                currentAction = null;
                return;
            }
        }

        bool inRange = !currentAction.RequiresInRange() || (currentAction.Target != null && Vector3.Distance(transform.position, currentAction.Target.transform.position) < 5f);

        if (inRange)
        {
            if (!currentAction.Perform(this)) AbortPlan("Action failed to perform.");
        }
        else if (currentAction.Target != null)
        {
            NavMeshAgent.isStopped = false;
            NavMeshAgent.SetDestination(currentAction.Target.transform.position);
        }
        else
        {
            AbortPlan("Action requires a target, but target is null.");
        }
    }

    public void AbortPlan(string reason)
    {
        Debug.LogError($"[{gameObject.name}] Plan Aborted: {reason}. Returning task '{CurrentGoal?.GoalName ?? "None"}'.");

        foreach (var action in currentPlan)
        {
            Destroy(action);
        }

        currentPlan.Clear();
        currentAction = null;
        if (CurrentGoal != null)
        {
            TaskManager.Instance.FailTask(CurrentGoal);
            CurrentGoal = null;
        }
        NavMeshAgent.isStopped = true;
    }


    public override float EvaluateScore()
    {
        float hp = GetHealth(); 
        float enemies = Mathf.Max(0f, AsessThreats()); 
        float distEnemy = GetDistanceToEnemy();  
        float potionsNear = GetHealingPotionAmount(); 
        float distHeal = DistanceToHealing();   

        SetInput(new float[] { hp, enemies, distEnemy, potionsNear, distHeal });
        Evaluate(desiredOutputSize: 5); 

        float outSearch = OutputValues[0];
        float outChase = OutputValues[1];
        float outAttack = OutputValues[2];
        float outRetreat = OutputValues[3];
        float outHeal = OutputValues[4];

        const float meleeRange = 2.0f;
        const float distNormMax = 20f;
        bool hasEnemy = (enemies > 0f) && !float.IsInfinity(distEnemy);
        bool inMelee = hasEnemy && (distEnemy <= meleeRange);
        bool lowHP = hp < 0.30f;
        bool outnumbered = enemies >= 3f;
        bool healAvailableOrNearby = (potionsNear > 0f) || !float.IsInfinity(distHeal);

        float normCloseEnemy = Mathf.Clamp01((distNormMax - Mathf.Min(distEnemy, distNormMax)) / distNormMax);
        float normCloseHeal = Mathf.Clamp01((distNormMax - Mathf.Min(distHeal, distNormMax)) / distNormMax);

        float score = 0f;

        score += 2.0f * hp;

        if (!hasEnemy) score += 0.30f * outSearch;  
        if (hasEnemy && !inMelee) score += 0.20f * outChase * normCloseEnemy;  
        if (inMelee) score += 0.20f * outAttack;         
        if (lowHP || outnumbered) score += 0.50f * Mathf.Clamp01(outRetreat); 
        if (lowHP && healAvailableOrNearby) score += 0.50f * outHeal * normCloseHeal;   
        score -= 0.10f * Mathf.Clamp(enemies, 0f, 5f); 
        score -= 0.01f;                

        score += IsAlive ? 3 : 0;
        
            
        

        return Mathf.Clamp(score, -10f, 10f);
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public string GetAgentName()
    {
        return name;
    }

    public NavMeshAgent getNavAgent()
    {
        return NavMeshAgent;
    }


    public float GetHealth()
    {
        return currenthealth / MaxHealth;
    }

    public void ModifyHealth(float modification)
    {
        currenthealth += modification;
        if (currenthealth > MaxHealth)
        {
            currenthealth = MaxHealth;
        }
        else if (currenthealth < 0) { Die(); }
    }

    public void Die()
    {
        IsAlive = false;
        gameObject.SetActive(false);
    }


}