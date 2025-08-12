using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    public List<ResourceSource> ResourceSources { get; private set; } = new List<ResourceSource>();
    public BuildLocation BuildLocation { get; private set; }

    // (FIX) Add a public field for the prefab, to be assigned in the Inspector.
    public GameObject pickupPrefab;

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
        return sources.OrderBy(s => Vector3.Distance(position, s.transform.position)).FirstOrDefault();
    }
}