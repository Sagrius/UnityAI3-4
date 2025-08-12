using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGoapGoal", menuName = "GOAP/Goap Goal", order = 0)]
public class GoapGoal : ScriptableObject
{
    public string GoalName;
    public int Priority = 10;

    [Tooltip("Conditions that must be true in the world before this goal can be pursued.")]
    [SerializeField] private List<WorldStateKVP> preconditions = new List<WorldStateKVP>();

    [Tooltip("The state of the world that satisfies this goal.")]
    [SerializeField] private List<WorldStateKVP> goalState = new List<WorldStateKVP>();

    private HashSet<KeyValuePair<string, object>> _goalStateSet;
    private HashSet<KeyValuePair<string, object>> _preconditionSet;

    public HashSet<KeyValuePair<string, object>> GetGoalState()
    {
        if (_goalStateSet == null)
        {
            _goalStateSet = new HashSet<KeyValuePair<string, object>>();
            foreach (var kvp in goalState) _goalStateSet.Add(new KeyValuePair<string, object>(kvp.key, kvp.value));
        }
        return _goalStateSet;
    }

    public HashSet<KeyValuePair<string, object>> GetPreconditions()
    {
        if (_preconditionSet == null)
        {
            _preconditionSet = new HashSet<KeyValuePair<string, object>>();
            foreach (var kvp in preconditions) _preconditionSet.Add(new KeyValuePair<string, object>(kvp.key, kvp.value));
        }
        return _preconditionSet;
    }
}