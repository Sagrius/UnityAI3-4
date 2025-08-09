using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(NavMeshAgent))]
public class GoapAgent : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    private Queue<GoapAction> actionQueue;
    private GoapAction currentAction;
    private IGoapDataProvider dataProvider;
    private GoapPlanner planner;

    // --- NEW: Cooldown for planning to prevent spamming ---
    private float planCooldown = 0f;
    private const float PLAN_RATE = 1f; // Try to plan every 1 second if no plan exists

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        actionQueue = new Queue<GoapAction>();
        dataProvider = GoapWorldState.Instance; // Use the singleton instance
        planner = new GoapPlanner();
    }

    void Update()
    {
        if (actionQueue.Count == 0)
        {
            // If we have no plan, try to find one on a cooldown.
            if (planCooldown <= 0)
            {
                FindPlan();
                planCooldown = PLAN_RATE;
            }
            else
            {
                planCooldown -= Time.deltaTime;
            }
            // --- EDITED: No return statement here, allows execution on the same frame a plan is found ---
        }

        // If we don't have a plan after trying, do nothing.
        if (actionQueue.Count == 0) return;


        // If we have a plan, check on the current action.
        if (currentAction == null)
        {
            currentAction = actionQueue.Peek();
        }

        if (currentAction.IsDone())
        {
            // The action is done, remove it and move to the next.
            actionQueue.Dequeue();
            currentAction = (actionQueue.Count > 0) ? actionQueue.Peek() : null;
            if (currentAction != null)
            {
                currentAction.DoReset();
            }
            else
            {
                Debug.Log("Plan finished!");
            }
            return;
        }

        // If we have a current action, execute it.
        if (currentAction != null)
        {
            if (currentAction.RequiresInRange() && currentAction.Target != null)
            {
                // Move towards the target.
                navMeshAgent.SetDestination(currentAction.Target.transform.position);
                if (Vector3.Distance(transform.position, currentAction.Target.transform.position) < 2f)
                {
                    // We are in range, perform the action.
                    if (!currentAction.Perform(gameObject))
                    {
                        AbortPlan();
                    }
                }
            }
            else
            {
                // Action does not require being in range.
                if (!currentAction.Perform(gameObject))
                {
                    AbortPlan();
                }
            }
        }
    }

    void AbortPlan()
    {
        Debug.Log("Action failed, aborting plan.");
        actionQueue.Clear();
        currentAction = null;
    }

    public void FindPlan()
    {
        var goal = new HashSet<KeyValuePair<string, object>>
        {
            new KeyValuePair<string, object>("resourceReadyForPickup", true)
        };

        // --- NEW: Better debugging ---
        var allActions = GetComponents<GoapAction>();
        var usableActions = allActions.Where(action => action.CheckProceduralPrecondition(gameObject)).ToList();
        string usableActionsStr = string.Join(", ", usableActions.Select(a => a.GetType().Name));
        Debug.Log($"[GoapAgent] Trying to find a plan. Usable actions: {usableActionsStr}");


        Queue<GoapAction> plan = planner.Plan(gameObject, allActions.ToList(), dataProvider.GetWorldState(), goal);

        if (plan != null)
        {
            Debug.Log("Found a plan! Steps: " + plan.Count);
            actionQueue = plan;
        }
        else
        {
            Debug.Log("Could not find a plan.");
        }
    }
}