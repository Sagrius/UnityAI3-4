using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(NavMeshAgent))]
public class GoapAgent : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    // ### NEW: Assign a goal asset in the Inspector ###
    public GoapGoal agentGoal;

    private Queue<GoapAction> actionQueue;
    private GoapAction currentAction;
    private IGoapDataProvider dataProvider;
    private GoapPlanner planner;

    private float planCooldown = 0f;
    private const float PLAN_RATE = 1f;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        actionQueue = new Queue<GoapAction>();
        dataProvider = GoapWorldState.Instance;
        planner = new GoapPlanner();
    }

    void Update()
    {
        if (actionQueue.Count == 0)
        {
            if (planCooldown <= 0)
            {
                FindPlan();
                planCooldown = PLAN_RATE;
            }
            else
            {
                planCooldown -= Time.deltaTime;
            }
        }

        if (actionQueue.Count == 0) return;

        if (currentAction == null)
        {
            currentAction = actionQueue.Peek();
        }

        if (currentAction.IsDone())
        {
            actionQueue.Dequeue();
            currentAction = (actionQueue.Count > 0) ? actionQueue.Peek() : null;
            if (currentAction != null)
            {
                currentAction.DoReset();
            }
            else
            {
                Debug.Log($"Plan finished for {gameObject.name}!");
            }
            return;
        }

        if (currentAction != null)
        {
            if (currentAction.RequiresInRange() && currentAction.Target != null)
            {
                navMeshAgent.SetDestination(currentAction.Target.transform.position);
                if (Vector3.Distance(transform.position, currentAction.Target.transform.position) < 2f)
                {
                    if (!currentAction.Perform(gameObject)) AbortPlan();
                }
            }
            else
            {
                if (!currentAction.Perform(gameObject)) AbortPlan();
            }
        }
    }

    void AbortPlan()
    {
        Debug.Log($"Action failed, aborting plan for {gameObject.name}.");
        actionQueue.Clear();
        currentAction = null;
    }

    public void FindPlan()
    {
        if (agentGoal == null)
        {
            Debug.LogWarning($"No goal assigned to {gameObject.name}.");
            return;
        }

        // ### EDITED: The goal is now fetched from the assigned ScriptableObject ###
        var goal = agentGoal.GetGoalState();

        var allActions = GetComponents<GoapAction>();
        var usableActions = allActions.Where(action => action.CheckProceduralPrecondition(gameObject)).ToList();
        string usableActionsStr = string.Join(", ", usableActions.Select(a => a.GetType().Name));
        Debug.Log($"[{gameObject.name}] Trying to find a plan. Goal: {goal.First().Key}. Usable actions: {usableActionsStr}");

        Queue<GoapAction> plan = planner.Plan(gameObject, allActions.ToList(), dataProvider.GetWorldState(), goal);

        if (plan != null)
        {
            Debug.Log($"Found a plan for {gameObject.name}! Steps: {plan.Count}");
            actionQueue = plan;
        }
        else
        {
            Debug.Log($"Could not find a plan for {gameObject.name}.");
        }
    }
}