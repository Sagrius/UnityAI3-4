using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public abstract class GoapAgent : MonoBehaviour
{
    public List<GoapAction> availableActions = new(); // Inspector-visible

    public Dictionary<string, object> worldState = new();
    public HashSet<KeyValuePair<string, object>> goal;

    protected Queue<GoapAction> actionQueue;

    public string currentGoalText;
    public string currentActionText;
    public string[] currentPlanActions;


    protected abstract void SetGoal();
    void Awake()
    {
        SetGoal();

        // Optional sanity check
        foreach (var action in availableActions)
        {
            if (action == null) Debug.LogWarning($"Missing GOAP action on {gameObject.name}");
        }

    }

    void Update()
    {
        if (actionQueue == null || actionQueue.Count == 0)
        {
            BuildWorldState();

            if (goal == null || worldState == null)
            {
                Debug.LogError($"{name}: GOAP plan failed — goal or world state is null.");
                return;
            }

            Plan();
            return;
        }

        var currentAction = actionQueue.Peek();
        currentActionText = currentAction.GetType().Name;

        if (!currentAction.IsDone())
            currentAction.Perform(gameObject);
        else
        {
            currentAction.Reset();
            actionQueue.Dequeue();
        }
    }

    void Plan()
    {
        //Debug.Log($"{name} planning...");
        Debug.Log($"{name}: World state before planning:");
        foreach (var kv in worldState)
        Debug.Log($" _World state   {kv.Key} = {kv.Value}");

        GoapPlanner planner = new();
        var usableActions = new HashSet<GoapAction>(availableActions.Where(a => a.IsValid(gameObject)));

        //Debug.Log($"{name}: Usable actions = {usableActions.Count}");

        actionQueue = planner.Plan(gameObject, usableActions, worldState, goal);

        if (actionQueue == null)
        {
            Debug.LogWarning($"{name}: NO PLAN FOUND for goal: {string.Join(", ", goal.Select(g => $"{g.Key}={g.Value}"))}");
            currentPlanActions = new[] { "No plan found." };
            return;
        }

        currentPlanActions = actionQueue.Select(a => a.GetType().Name).ToArray();
        currentGoalText = string.Join(", ", goal.Select(g => $"{g.Key}={g.Value}"));
       
    }


    protected abstract void BuildWorldState();

    protected bool IsNearTagged(string tag, float range = 2f)
    {
        var target = GameObject.FindGameObjectWithTag(tag);
        return target && Vector3.Distance(transform.position, target.transform.position) <= range;
    }

    protected bool IsNearTransform(Transform t, float range = 2f)
    {
        return t && Vector3.Distance(transform.position, t.position) <= range;
    }

    protected void DebugLogWorldState()
    {
        foreach (var kv in worldState)
        {
            Debug.Log($"{name} worldState: {kv.Key} = {kv.Value}");
        }
    }
}



