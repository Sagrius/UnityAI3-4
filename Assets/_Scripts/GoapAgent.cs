using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GoapAgent : MonoBehaviour
{
    public List<GoapAction> availableActions = new(); // Inspector-visible

    public Dictionary<string, object> worldState = new();
    public HashSet<KeyValuePair<string, object>> goal;

    private Queue<GoapAction> actionQueue;

    void Awake()
    {
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
            Plan();
            return;
        }

        var currentAction = actionQueue.Peek();
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
        GoapPlanner planner = new();
        var usableActions = new HashSet<GoapAction>(availableActions.Where(a => a.IsValid(gameObject)));

        actionQueue = planner.Plan(gameObject, usableActions, worldState, goal);
    }

    void BuildWorldState()
    {
        worldState.Clear();
        // Add facts like "AtBuildSite", "FalconAvailable", etc.
    }
}



