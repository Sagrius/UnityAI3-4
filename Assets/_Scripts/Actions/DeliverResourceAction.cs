using UnityEngine;

public class DeliverResourceAction : GoapAction
{
    private bool delivered = false;
    public DeliverResourceAction()
    {
        AddPrecondition("resourceReadyForPickup", true);
        AddEffect("resourceDelivered", true);
        AddEffect("resourceReadyForPickup", false);
        Cost = 1f; // Falcon is fast
    }
    public override void Reset() { delivered = false; Target = null; }
    public override bool IsDone() => delivered;
    public override bool RequiresInRange() => true;
    public override bool CheckProceduralPrecondition(GameObject agent)
    {
        Target = ResourceManager.Instance.BuildLocation?.gameObject;
        return Target != null;
    }
    public override bool Perform(GameObject agent)
    {
        Debug.Log("Falcon delivered the resource!");
        delivered = true;
        // Here you would update the BuildLocation's inventory
        return true;
    }
}