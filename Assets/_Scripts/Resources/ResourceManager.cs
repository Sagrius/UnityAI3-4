using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    public List<ResourceSource> ResourceSources { get; private set; } = new List<ResourceSource>();
    public BuildLocation BuildLocation { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // Proactively find all world objects. This is robust against script execution order.
        ResourceSources = FindObjectsByType<ResourceSource>(FindObjectsSortMode.None).ToList();
        BuildLocation = FindObjectOfType<BuildLocation>();
        Debug.Log($"ResourceManager Initialized: Found {ResourceSources.Count} resource sources and a build location.");
    }

    public ResourceSource GetClosestResource(ResourceSource.ResourceType type, Vector3 position)
    {
        return ResourceSources
            .Where(s => s.Type == type)
            .OrderBy(s => Vector3.Distance(position, s.transform.position))
            .FirstOrDefault();
    }
}