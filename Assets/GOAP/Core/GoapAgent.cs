using UnityEngine.AI;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AgentBlackboard))]
public class GoapAgent : MonoBehaviour
{
    public NavMeshAgent NavMeshAgent { get; private set; }
    public AgentBlackboard Blackboard { get; private set; }
    private GoapPlanner planner;
    private Queue<GoapAction> currentPlan;
    private GoapAction currentAction;

    [Tooltip("The set of action types this agent is capable of performing.")]
    [SerializeField] private List<string> availableActionNames = new List<string>();
    private List<GoapAction> availableActions;

    public GoapGoal CurrentGoal { get; private set; }
    private float planCooldown = 0f;
    private const float PLAN_RATE = 1f;
    private Vector3 startingPosition;

    public List<GoapAction> GetAvailableActions() => availableActions;

    void Awake()
    {
        NavMeshAgent = GetComponent<NavMeshAgent>();
        Blackboard = GetComponent<AgentBlackboard>();
        planner = new GoapPlanner();
        currentPlan = new Queue<GoapAction>();
        startingPosition = transform.position;

        availableActions = new List<GoapAction>();
        foreach (string actionName in availableActionNames)
        {
            if (string.IsNullOrEmpty(actionName)) continue;
            Type type = Type.GetType(actionName);
            if (type != null && typeof(GoapAction).IsAssignableFrom(type))
            {
                availableActions.Add((GoapAction)Activator.CreateInstance(type));
            }
            else
            {
                Debug.LogWarning($"Could not find GoapAction type: {actionName} on agent {gameObject.name}");
            }
        }
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
                Debug.Log($"[{gameObject.name}] No tasks available. Returning home.");
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
        var agentState = Blackboard.GetAgentState();
        var currentState = new HashSet<KeyValuePair<string, object>>(worldState);
        currentState.UnionWith(agentState);

        foreach (var action in availableActions)
        {
            action.DoReset();
        }

        var usableActions = availableActions.Where(a => a.CheckProceduralPrecondition(this)).ToList();
        string usableActionsStr = string.Join(", ", usableActions.Select(a => a.ActionName));
        Debug.Log($"[{gameObject.name}] Finding plan for '{CurrentGoal.GoalName}'. Usable actions: [{usableActionsStr}]");

        Queue<GoapAction> plan = planner.Plan(this, usableActions, currentState, goalState);

        if (plan != null && plan.Count > 0)
        {
            Debug.Log($"[{gameObject.name}] Found plan with {plan.Count} steps.");
            currentPlan = plan;
            currentAction = currentPlan.Peek();
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
            currentPlan.Dequeue();
            if (currentPlan.Count > 0)
            {
                currentAction = currentPlan.Peek();
                currentAction.DoReset();
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
        currentPlan.Clear();
        currentAction = null;
        if (CurrentGoal != null)
        {
            // (FIX) Inform the task manager that the goal failed so it can clear any in-progress counts.
            TaskManager.Instance.FailTask(CurrentGoal);
            CurrentGoal = null;
        }
        NavMeshAgent.isStopped = true;
    }
}