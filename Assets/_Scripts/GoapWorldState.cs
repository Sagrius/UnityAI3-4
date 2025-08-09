using UnityEngine;
using System.Collections.Generic;

public class GoapWorldState : MonoBehaviour, IGoapDataProvider
{
    public static GoapWorldState Instance { get; private set; }
    private HashSet<KeyValuePair<string, object>> worldState = new HashSet<KeyValuePair<string, object>>();

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;

        // Initialize world state
        worldState.Add(new KeyValuePair<string, object>("resourceDelivered", false));
        worldState.Add(new KeyValuePair<string, object>("hasRawResource", false));
        worldState.Add(new KeyValuePair<string, object>("resourceReadyForPickup", false));
    }

    public HashSet<KeyValuePair<string, object>> GetWorldState() => worldState;
}