using UnityEngine;
using System.Collections.Generic;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    public Dictionary<string, int> resources;
    public Dictionary<string, bool> globalFacts = new();

   
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);

        resources = new Dictionary<string, int>();
    }

    public bool GetFact(string key)
    {
        return globalFacts.ContainsKey(key) && globalFacts[key];
    }

    public void SetFact(string key, bool value)
    {
        globalFacts[key] = value;
    }


    public void Add(string resourceName, int amount)
    {
        if (!resources.ContainsKey(resourceName)) resources.Add(resourceName, amount);
        else resources[resourceName] += amount;
    }
    public bool Has(string resourceName, int amount) 
    {
        bool containsMaterial = resources.ContainsKey(resourceName);
        if (!containsMaterial) return false;
        Debug.Log("You don't have this material");

        bool hasRequiredAmount = resources[resourceName] >= amount;
        return hasRequiredAmount;
            
    }
    public void Remove(string resourceName, int amount)
    {
        if (!Has(resourceName, amount))
        {
            Debug.Log("Attempting to remove a resource that you lack the required amount of, or perhaps the resource itself");
            return;
        }
        resources[resourceName] -= amount;
        resources[resourceName] = Mathf.Clamp(resources[resourceName], 0, 100);
    }
}
