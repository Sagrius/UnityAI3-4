using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GoapAgent : MonoBehaviour
{
    private Queue<GoapAction> actionQueue;
    public Dictionary<string, object> worldState = new();
    public HashSet<GoapAction> availableActions = new();
    public HashSet<KeyValuePair<string, object>> goal;

    void Start() => availableActions = new(GetComponents<GoapAction>());

    void Update()
    {
        if (actionQueue == null || actionQueue.Count == 0)
        {
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
        actionQueue = planner.Plan(gameObject, availableActions, worldState, goal);
    }
}


