using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CombatStats))]
public class GoapAgent : MonoBehaviour
{
    public NavMeshAgent NavMeshAgent { get; private set; }
    public CombatStats CombatStats { get; private set; }
    public GameObject CurrentTarget { get; set; }

    private GoapPlanner planner;
    private Queue<GoapAction> currentPlan;
    private GoapAction currentAction;

    [Tooltip("The set of actions this agent can perform. Assign action assets here.")]
    [SerializeField] private List<GoapAction> availableActions;

    public GoapGoal CurrentGoal { get; private set; }
    private float planCooldown = 0f;
    private const float PLAN_RATE = 1f;
    private Vector3 startingPosition;

    public List<GoapAction> GetAvailableActions() => availableActions;

    void Awake()
    {
        NavMeshAgent = GetComponent<NavMeshAgent>();
        CombatStats = GetComponent<CombatStats>();
        planner = new GoapPlanner();
        currentPlan = new Queue<GoapAction>();
        startingPosition = transform.position;
    }

    void Update()
    {
        // *** THE FIX: High-Priority Interrupt ***
        // If the agent is in danger, it should drop everything and react.
        if (CombatStats.IsUnderAttack || CombatStats.currentHealth < CombatStats.healingThreshold)
        {
            // Check if the current plan is already a survival plan.
            bool isSurvivalPlan = currentAction is AttackAgentAction || currentAction is RetreatAction || currentAction is GetHealingPotionAction;

            if (currentAction != null && !isSurvivalPlan)
            {
                Debug.LogWarning($"[{gameObject.name}] is under duress and aborting current task: {currentAction.ActionName}");
                AbortPlan("Emergency override: Under attack or low health.");
            }
        }

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

    private void FindPlan()
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

    private void ExecutePlan()
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
}