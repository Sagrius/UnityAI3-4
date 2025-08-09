using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GoapPlanner
{
    public Queue<GoapAction> Plan(GameObject agent, List<GoapAction> availableActions, HashSet<KeyValuePair<string, object>> worldState, HashSet<KeyValuePair<string, object>> goal)
    {
        foreach (var action in availableActions) action.DoReset();

        List<GoapAction> usableActions = availableActions.Where(action => action.CheckProceduralPrecondition(agent)).ToList();
        List<Node> leaves = new List<Node>();
        Node start = new Node(null, 0, worldState, null);

        if (!BuildGraph(start, leaves, usableActions, goal))
        {
            return null;
        }

        Node cheapest = leaves.OrderBy(leaf => leaf.runningCost).FirstOrDefault();
        if (cheapest == null) return null;

        List<GoapAction> result = new List<GoapAction>();
        Node n = cheapest;
        while (n != null)
        {
            if (n.action != null) result.Insert(0, n.action);
            n = n.parent;
        }
        return new Queue<GoapAction>(result);
    }

    private bool BuildGraph(Node parent, List<Node> leaves, List<GoapAction> usableActions, HashSet<KeyValuePair<string, object>> goal)
    {
        bool foundOne = false;
        foreach (var action in usableActions)
        {
            if (InState(action.Preconditions, parent.state))
            {
                HashSet<KeyValuePair<string, object>> currentState = ApplyState(parent.state, action.Effects);
                Node node = new Node(parent, parent.runningCost + action.Cost, currentState, action);

                if (InState(goal, currentState))
                {
                    leaves.Add(node);
                    foundOne = true;
                }
                else
                {
                    List<GoapAction> subset = ActionSubset(usableActions, action);
                    if (BuildGraph(node, leaves, subset, goal)) foundOne = true;
                }
            }
        }
        return foundOne;
    }

    private bool InState(HashSet<KeyValuePair<string, object>> test, HashSet<KeyValuePair<string, object>> state) => test.All(t => state.Contains(t));

    private HashSet<KeyValuePair<string, object>> ApplyState(HashSet<KeyValuePair<string, object>> currentState, HashSet<KeyValuePair<string, object>> effects)
    {
        var newState = new HashSet<KeyValuePair<string, object>>(currentState);
        foreach (var effect in effects)
        {
            newState.RemoveWhere(s => s.Key == effect.Key);
            newState.Add(effect);
        }
        return newState;
    }

    private List<GoapAction> ActionSubset(List<GoapAction> actions, GoapAction removeMe) => actions.Where(a => !a.Equals(removeMe)).ToList();

    private class Node
    {
        public Node parent;
        public float runningCost;
        public HashSet<KeyValuePair<string, object>> state;
        public GoapAction action;

        public Node(Node parent, float runningCost, HashSet<KeyValuePair<string, object>> state, GoapAction action)
        {
            this.parent = parent; this.runningCost = runningCost; this.state = state; this.action = action;
        }
    }
}