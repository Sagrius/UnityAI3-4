using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    public List<ResourceSource> ResourceSources { get; private set; } = new List<ResourceSource>();
    public BuildLocation BuildLocation { get; private set; }
    public GameObject pickupPrefab;

    // (FIX) A new set to keep track of which nodes are "claimed" by an agent.
    private HashSet<ResourceSource> claimedSources = new HashSet<ResourceSource>();

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        ResourceSources = FindObjectsOfType<ResourceSource>().ToList();
        BuildLocation = FindObjectOfType<BuildLocation>();
        Debug.Log($"ResourceManager Initialized: Found {ResourceSources.Count} total resource sources.");
    }

    public ResourceSource GetClosestResource(List<ResourceSource> sources, Vector3 position)
    {
        // (FIX) Now filters out any sources that are already claimed.
        return sources
            .Where(s => !claimedSources.Contains(s))
            .OrderBy(s => Vector3.Distance(position, s.transform.position))
            .FirstOrDefault();
    }

    // (FIX) New methods to allow agents to claim and release resource nodes.
    public void ClaimResource(ResourceSource source)
    {
        claimedSources.Add(source);
    }

    public void ReleaseResource(ResourceSource source)
    {
        claimedSources.Remove(source);
    }
}