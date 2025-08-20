using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PrepareResourceAction", menuName = "GOAP/Actions/Prepare Resource")]
public class PrepareAndDropResourceAction : GoapAction
{
    public enum ResourceToPrepare { Logs, Iron, Crystals }
    [Tooltip("The type of resource this action will gather and prepare.")]
    public ResourceToPrepare resource;

    private ResourceSource.ResourceType resourceSourceType;
    private PickupLocation.ResourceType pickupType;
    private string effectKey;

    private float collectionTimer;
    private bool isCollecting;
    [System.NonSerialized] private ResourceSource targetResourceSource;

    void OnEnable()
    {
        switch (resource)
        {
            case ResourceToPrepare.Logs:
                resourceSourceType = ResourceSource.ResourceType.Tree;
                pickupType = PickupLocation.ResourceType.Logs;
                effectKey = WorldStateKeys.LogsReadyForPickup;
                break;
            case ResourceToPrepare.Iron:
                resourceSourceType = ResourceSource.ResourceType.Mine;
                pickupType = PickupLocation.ResourceType.Iron;
                effectKey = WorldStateKeys.IronReadyForPickup;
                break;
            case ResourceToPrepare.Crystals:
                resourceSourceType = ResourceSource.ResourceType.CrystalCavern;
                pickupType = PickupLocation.ResourceType.Crystals;
                effectKey = WorldStateKeys.CrystalsReadyForPickup;
                break;
        }
        ActionName = $"Prepare {pickupType}";
        Effects.Clear();
        AddEffect(effectKey, true);
    }

    public override void OnReset()
    {
        if (targetResourceSource != null) ResourceManager.Instance.ReleaseResource(targetResourceSource);
        Target = null;
        targetResourceSource = null;
        collectionTimer = 0.5f;
        isCollecting = false;
    }
    public override bool RequiresInRange() => true;

    public override bool CheckProceduralPrecondition(GoapAgent agent)
    {
        // For planning, just check if any resource of this type exists.
        var sources = ResourceManager.Instance.ResourceSources.Where(s => s.Type == resourceSourceType && s.quantity >= 1).ToList();
        return sources.Count > 0;
    }

    public override bool SetupAction(GoapAgent agent)
    {
        // For execution, find the closest resource and claim it.
        var sources = ResourceManager.Instance.ResourceSources.Where(s => s.Type == resourceSourceType && s.quantity >= 1).ToList();
        targetResourceSource = ResourceManager.Instance.FindAndClaimClosestResource(sources, agent.transform.position);

        if (targetResourceSource != null)
        {
            Target = targetResourceSource.gameObject;
            return true;
        }
        // If we fail to find and claim a resource here, the action setup fails.
        return false;
    }

    public override bool Perform(GoapAgent agent)
    {
        if (targetResourceSource == null) return false;

        if (agent.NavMeshAgent.pathPending || agent.NavMeshAgent.remainingDistance > agent.NavMeshAgent.stoppingDistance) return true;

        if (!isCollecting) isCollecting = true;

        collectionTimer -= Time.deltaTime;
        if (collectionTimer > 0) return true;

        targetResourceSource.quantity -= 1;

        GameObject pickup = GameObject.Instantiate(ResourceManager.Instance.pickupPrefab, agent.transform.position, Quaternion.identity);
        var pickupData = pickup.GetComponent<PickupLocation>();
        pickupData.Type = pickupType;
        pickupData.Amount = 1;
        WorldState.Instance.AddPickup(pickupData);

        // *** THE FIX ***
        // Check if the resource is now empty.
        if (targetResourceSource.quantity <= 0)
        {
            Debug.Log($"[{agent.name}] depleted {targetResourceSource.name}. Removing from world.");
            // Remove it from the manager and destroy the object.
            ResourceManager.Instance.RemoveResourceSource(targetResourceSource);
            Destroy(targetResourceSource.gameObject);
        }
        else
        {
            // If it's not empty, just release the claim so another agent can use it.
            ResourceManager.Instance.ReleaseResource(targetResourceSource);
        }

        SetDone(true);
        return true;
    }
}