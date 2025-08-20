using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    public List<ResourceSource> ResourceSources { get; private set; } = new List<ResourceSource>();
    public BuildLocation BuildLocation { get; private set; }
    public GameObject pickupPrefab;

    private HashSet<ResourceSource> claimedSources = new HashSet<ResourceSource>();

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        ResourceSources = FindObjectsByType<ResourceSource>(FindObjectsSortMode.None).ToList();
        BuildLocation = FindFirstObjectByType<BuildLocation>();
    }

    public ResourceSource FindAndClaimClosestResource(List<ResourceSource> sources, Vector3 position)
    {
        ResourceSource closestSource = sources
            .Where(s => !claimedSources.Contains(s))
            .OrderBy(s => Vector3.Distance(position, s.transform.position))
            .FirstOrDefault();

        if (closestSource != null)
        {
            ClaimResource(closestSource);
        }
        return closestSource;
    }

    public void ClaimResource(ResourceSource source) => claimedSources.Add(source);
    public void ReleaseResource(ResourceSource source) => claimedSources.Remove(source);

    public void RemoveResourceSource(ResourceSource source)
    {
        if (ResourceSources.Contains(source))
        {
            ResourceSources.Remove(source);
        }
    }
}