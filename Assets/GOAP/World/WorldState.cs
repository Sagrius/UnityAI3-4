using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WorldState : MonoBehaviour
{
    public static WorldState Instance { get; private set; }
    private Dictionary<string, object> _worldState = new Dictionary<string, object>();
    private List<PickupLocation> availablePickups = new List<PickupLocation>();

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        SetState("oakLogsInStockpile", 0);
        SetState("ironIngotsInStockpile", 0);
        SetState("crystalShardsInStockpile", 0);
        SetState("enchantedStaffBuilt", false);
        SetState("runedShieldBuilt", false);
        SetState("combinedArtifactBuilt", false);
    }

    public HashSet<KeyValuePair<string, object>> GetWorldState()
    {
        var stateSet = new HashSet<KeyValuePair<string, object>>();
        foreach (var kvp in _worldState) stateSet.Add(kvp);
        return stateSet;
    }

    public object GetState(string key)
    {
        _worldState.TryGetValue(key, out object value);
        return value;
    }

    public void SetState(string key, object value) => _worldState[key] = value;

    public void ModifyState(string key, int value)
    {
        if (_worldState.TryGetValue(key, out object currentValue) && currentValue is int)
            _worldState[key] = (int)currentValue + value;
        else
            SetState(key, value);
    }

    public void AddPickup(PickupLocation pickup) => availablePickups.Add(pickup);
    public void RemovePickup(PickupLocation pickup) => availablePickups.Remove(pickup);
    public PickupLocation GetClosestPickup(Vector3 position) => availablePickups.OrderBy(p => Vector3.Distance(position, p.transform.position)).FirstOrDefault();

    public int CountPickupsOfType(PickupLocation.ResourceType type)
    {
        return availablePickups.Count(p => p.Type == type);
    }
}
