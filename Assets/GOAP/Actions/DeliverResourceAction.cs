using UnityEngine;

[CreateAssetMenu(fileName = "DeliverResourceAction", menuName = "GOAP/Actions/Deliver Resource")]
public class DeliverResourceAction : GoapAction
{
    [System.NonSerialized] private PickupLocation targetPickup;
    private bool hasPickedUp = false;
    private PickupLocation.ResourceType resourceTypeToDeliver;
    private int resourceAmountToDeliver;

    void OnEnable()
    {
        ActionName = "Deliver Resources";
        Effects.Clear();
        AddEffect(WorldStateKeys.ResourceDelivered, true);
    }

    public override void OnReset()
    {
        if (targetPickup != null && !hasPickedUp) targetPickup.isClaimed = false;
        Target = null;
        targetPickup = null;
        hasPickedUp = false;
    }

    public override bool RequiresInRange() => true;

    public override bool CheckProceduralPrecondition(IGoapAgent agent)
    {
        // For planning, just check if any pickup exists.
        return WorldState.Instance.GetClosestPickup(agent.GetTransform().position) != null;
    }

    public override bool SetupAction(IGoapAgent agent)
    {
        // For execution, find the closest pickup and claim it.
        targetPickup = WorldState.Instance.GetClosestPickup(agent.GetTransform().position);
        if (targetPickup != null)
        {
            Target = targetPickup.gameObject;
            targetPickup.isClaimed = true;
            return true;
        }
        return false;
    }

    public override bool Perform(IGoapAgent agent)
    {
        if (hasPickedUp == false && targetPickup == null) return false;

        if (!hasPickedUp)
        {
            resourceTypeToDeliver = targetPickup.Type;
            resourceAmountToDeliver = targetPickup.Amount;
            WorldState.Instance.RemovePickup(targetPickup);
            GameObject.Destroy(targetPickup.gameObject);
            hasPickedUp = true;
        }

        Target = ResourceManager.Instance.BuildLocation.gameObject;
        agent.getNavAgent().SetDestination(Target.transform.position);

        if (Vector3.Distance(agent.GetTransform().position, Target.transform.position) < 3f)
        {
            switch (resourceTypeToDeliver)
            {
                case PickupLocation.ResourceType.Logs:
                    WorldState.Instance.ModifyState(WorldStateKeys.LogsInStockpile, resourceAmountToDeliver);
                    break;
                case PickupLocation.ResourceType.Iron:
                    WorldState.Instance.ModifyState(WorldStateKeys.IronInStockpile, resourceAmountToDeliver);
                    break;
                case PickupLocation.ResourceType.Crystals:
                    WorldState.Instance.ModifyState(WorldStateKeys.CrystalsInStockpile, resourceAmountToDeliver);
                    break;
            }
            Debug.Log($"<color=orange>[{agent.GetAgentName()}] delivered {resourceAmountToDeliver} {resourceTypeToDeliver}.</color>");
            TaskManager.Instance.NotifyResourceDelivered(resourceTypeToDeliver);
            SetDone(true);
        }
        return true;
    }
}