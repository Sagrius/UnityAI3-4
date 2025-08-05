using UnityEngine;

public class FalconAgent : GoapAgent
{
    protected override void BuildWorldState()
    {
        var falcon = GetComponent<FalconGoapAgent>();

        if (falcon.pickupTarget == null && TaskManager.Instance.TryGetFalconJob(out var pickup, out var build))
        {
            falcon.pickupTarget = pickup;
            falcon.buildTarget = build;
            ResourceManager.Instance.SetFact("FalconAvailable", false);
        }

        worldState.Clear();

        worldState["PickupTaskAssigned"] = falcon.pickupTarget != null;
        worldState["CarryingResource"] = falcon.isCarryingResource;
        worldState["AtPickupLocation"] = IsNearTransform(falcon.pickupTarget);
        worldState["AtBuildSite"] = IsNearTagged("BuildLocation");
        worldState["TaskComplete"] = falcon.isCarryingResource && IsNearTagged("BuildLocation");

        DebugLogWorldState();
    }

    protected override void SetGoal()
    {
        goal = new()
        {
            new("ResourceDelivered", true)
        };

    }
}
