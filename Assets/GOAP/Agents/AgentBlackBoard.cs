using System.Collections.Generic;
using UnityEngine;

public class AgentBlackboard : MonoBehaviour
{
    private Dictionary<string, object> _agentState = new Dictionary<string, object>();

    void Start()
    {
        SetState("hasPreparedResource", false);
        SetState("numLogs", 0);
        SetState("numIron", 0);
        SetState("numCrystals", 0);
    }

    public HashSet<KeyValuePair<string, object>> GetAgentState()
    {
        var stateSet = new HashSet<KeyValuePair<string, object>>();
        foreach (var kvp in _agentState) stateSet.Add(kvp);
        return stateSet;
    }

    public object GetState(string key)
    {
        _agentState.TryGetValue(key, out object value);
        return value;
    }

    public void SetState(string key, object value) => _agentState[key] = value;
}