using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoapPlanner
{
    public Queue<GoapAction> Plan(
        GameObject agent,
        HashSet<GoapAction> availableActions,
        Dictionary<string, object> worldState,
        HashSet<KeyValuePair<string, object>> goal)
    {
        List<Node> leaves = new();
        Node start = new(null, 0, worldState, null);

        bool success = BuildGraph(start, leaves, availableActions, goal);

        if (!success)
        {
           // Debug.Log("No plan found.");
            return null;
        }

        // Choose cheapest plan
        Node cheapest = leaves.OrderBy(n => n.Cost).First();

        // Rebuild path
        Queue<GoapAction> result = new();
        while (cheapest != null && cheapest.Action != null)
        {
            result.Enqueue(cheapest.Action);
            cheapest = cheapest.Parent;
        }

        return new Queue<GoapAction>(result.Reverse());
    }

    private bool BuildGraph(
        Node parent,
        List<Node> leaves,
        HashSet<GoapAction> usableActions,
        HashSet<KeyValuePair<string, object>> goal)
    {
        bool foundOne = false;

        foreach (GoapAction action in usableActions)
        {
            if (!InState(action.Preconditions, parent.State)) continue;

            Dictionary<string, object> newState = new(parent.State);

            foreach (var effect in action.Effects)
            {
                newState[effect.Key] = effect.Value;
            }

            Node node = new(parent, parent.Cost + action.Cost, newState, action);

            if (GoalAchieved(goal, newState))
            {
                leaves.Add(node);
                foundOne = true;
            }
            else
            {
                HashSet<GoapAction> remaining = new(usableActions);
                remaining.Remove(action);
                if (BuildGraph(node, leaves, remaining, goal))
                    foundOne = true;
            }
        }

        return foundOne;
    }

    private bool InState(HashSet<KeyValuePair<string, object>> test, Dictionary<string, object> state)
    {
        foreach (var pair in test)
        {
            if (!state.ContainsKey(pair.Key)) return false;
            if (!state[pair.Key].Equals(pair.Value)) return false;
        }
        return true;
    }

    private bool GoalAchieved(HashSet<KeyValuePair<string, object>> goal, Dictionary<string, object> state)
    {
        return InState(goal, state);
    }

    private class Node
    {
        public Node Parent;
        public float Cost;
        public Dictionary<string, object> State;
        public GoapAction Action;

        public Node(Node parent, float cost, Dictionary<string, object> state, GoapAction action)
        {
            Parent = parent;
            Cost = cost;
            State = state;
            Action = action;
        }
    }
}

