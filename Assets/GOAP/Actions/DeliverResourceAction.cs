using UnityEngine;

public class DeliverResourceAction : GoapAction
{
    private PickupLocation targetPickup;
    private bool hasPickedUp = false;

    // Add these two lines to store the resource data
    private PickupLocation.ResourceType resourceTypeToDeliver;
    private int resourceAmountToDeliver;

    public DeliverResourceAction()
    {
        ActionName = "Deliver Resources";

        AddEffect("resourceDelivered", true);
    }

    public override void OnReset()
    {
        // If we had a target but failed to pick it up, release the claim so someone else can get it.
        if (targetPickup != null && !hasPickedUp)
        {
            targetPickup.isClaimed = false;
        }
        Target = null;
        targetPickup = null;
        hasPickedUp = false;
    }

    public override bool RequiresInRange() => true;

    public override bool CheckProceduralPrecondition(GoapAgent agent)
    {
        // Find the closest UNCLAIMED pickup
        targetPickup = WorldState.Instance.GetClosestPickup(agent.transform.position);

        if (targetPickup != null)
        {
            Target = targetPickup.gameObject;
            targetPickup.isClaimed = true; // IMPORTANT: Claim the target immediately!
            return true;
        }
        return false;
    }

    // In DeliverResourceAction.cs, replace the existing Perform method

    public override bool Perform(GoapAgent agent)
    {
        // This can happen if the target was destroyed by something else after being claimed.
        if (hasPickedUp == false && targetPickup == null)
        {
            return false; // Abort the action.
        }

        // --- PICKUP PHASE ---
        // If we haven't picked up the resource yet, we are at the pickup location.
        if (!hasPickedUp)
        {
            Debug.Log($"<color=orange>[{agent.name}] picked up {targetPickup.Amount} {targetPickup.Type}.</color>");

            // 1. Remember the resource details before destroying the object.
            resourceTypeToDeliver = targetPickup.Type;
            resourceAmountToDeliver = targetPickup.Amount;

            // 2. Remove the pickup from the world state list.
            WorldState.Instance.RemovePickup(targetPickup);

            // 3. Destroy the physical GameObject from the scene immediately.
            GameObject.Destroy(targetPickup.gameObject);

            // 4. Set our status to "has picked up".
            hasPickedUp = true;
        }

        // --- DELIVERY PHASE ---
        // Now, fly to the build location.
        Target = ResourceManager.Instance.BuildLocation.gameObject;
        agent.NavMeshAgent.SetDestination(Target.transform.position);

        // Check if we have arrived at the build location.
        if (Vector3.Distance(agent.transform.position, Target.transform.position) < 3f)
        {
            // THIS IS THE MISSING PIECE YOU NEED TO ADD
            switch (resourceTypeToDeliver)
            {
                case PickupLocation.ResourceType.Logs:
                    WorldState.Instance.ModifyState("oakLogsInStockpile", resourceAmountToDeliver);
                    break;
                case PickupLocation.ResourceType.Iron:
                    WorldState.Instance.ModifyState("ironIngotsInStockpile", resourceAmountToDeliver);
                    break;
                case PickupLocation.ResourceType.Crystals:
                    WorldState.Instance.ModifyState("crystalShardsInStockpile", resourceAmountToDeliver);
                    break;
            }

            Debug.Log($"<color=orange>[{agent.name}] delivered {resourceAmountToDeliver} {resourceTypeToDeliver}.</color>");

            TaskManager.Instance.NotifyResourceDelivered(resourceTypeToDeliver);

            SetDone(true);
        }

        return true; // Action is still in progress until delivery is complete.
    }
}