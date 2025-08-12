using System.Linq;
using UnityEngine;

public abstract class PrepareAndDropResourceAction : GoapAction
{
    protected ResourceSource.ResourceType resourceType;
    protected PickupLocation.ResourceType pickupType;
    protected string goalEffectKey;
    private ResourceSource targetResourceSource;

    private float collectionTimer = 0.5f;
    private bool isCollecting = false;

    public PrepareAndDropResourceAction(ResourceSource.ResourceType resType, PickupLocation.ResourceType pickType, string effectKey)
    {
        resourceType = resType;
        pickupType = pickType;
        goalEffectKey = effectKey;
        ActionName = $"Prepare {pickType}";
        AddEffect(goalEffectKey, true);
    }

    public override void OnReset()
    {
        if (targetResourceSource != null)
        {
            ResourceManager.Instance.ReleaseResource(targetResourceSource);
        }
        Target = null;
        targetResourceSource = null;
        collectionTimer = 0.5f;
        isCollecting = false;
    }
    public override bool RequiresInRange() => true;

    public override bool CheckProceduralPrecondition(GoapAgent agent)
    {
        if (ResourceManager.Instance.pickupPrefab == null)
        {
            Debug.LogError("PickupPrefab has not been assigned in the ResourceManager!");
            return false;
        }
        var sources = ResourceManager.Instance.ResourceSources.Where(s => s.Type == resourceType && s.quantity >= 1).ToList();
        if (sources.Count == 0) return false;

        targetResourceSource = ResourceManager.Instance.GetClosestResource(sources, agent.transform.position);

        if (targetResourceSource != null)
        {
            Target = targetResourceSource.gameObject;
            ResourceManager.Instance.ClaimResource(targetResourceSource);
            return true;
        }
        return false;
    }

    public override bool Perform(GoapAgent agent)
    {
        if (targetResourceSource == null) return false;

        if (agent.NavMeshAgent.pathPending || agent.NavMeshAgent.remainingDistance > agent.NavMeshAgent.stoppingDistance)
        {
            return true;
        }

        if (!isCollecting)
        {
            isCollecting = true;
            Debug.Log($"[{agent.name}] started collecting 1 {pickupType}.");
        }

        collectionTimer -= Time.deltaTime;
        if (collectionTimer > 0)
        {
            return true;
        }

        targetResourceSource.quantity -= 1;
        GameObject pickup = GameObject.Instantiate(ResourceManager.Instance.pickupPrefab, agent.transform.position, Quaternion.identity);

        var pickupData = pickup.GetComponent<PickupLocation>();
        pickupData.Type = pickupType;
        pickupData.Amount = 1;
        WorldState.Instance.AddPickup(pickupData);

        Debug.Log($"[{agent.name}] finished preparing and dropped 1 {pickupType}.");

        ResourceManager.Instance.ReleaseResource(targetResourceSource);

        SetDone(true);
        return true;
    }
}

public class PrepareLogsAction : PrepareAndDropResourceAction
{
    public PrepareLogsAction() : base(ResourceSource.ResourceType.Tree, PickupLocation.ResourceType.Logs, "logsReadyForPickup") { }
}
public class PrepareIronAction : PrepareAndDropResourceAction
{
    public PrepareIronAction() : base(ResourceSource.ResourceType.Mine, PickupLocation.ResourceType.Iron, "ironReadyForPickup") { }
}
public class PrepareCrystalsAction : PrepareAndDropResourceAction
{
    public PrepareCrystalsAction() : base(ResourceSource.ResourceType.CrystalCavern, PickupLocation.ResourceType.Crystals, "crystalsReadyForPickup") { }
}