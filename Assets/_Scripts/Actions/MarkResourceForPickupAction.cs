using UnityEngine;

public class MarkResourceForPickupAction : GoapAction
{
    private bool marked = false;
    public MarkResourceForPickupAction()
    {
        AddPrecondition("hasRawResource", true);
        AddEffect("resourceReadyForPickup", true);
        AddEffect("hasRawResource", false);
    }
    public override void Reset() => marked = false;
    public override bool IsDone() => marked;
    public override bool RequiresInRange() => false;
    public override bool CheckProceduralPrecondition(GameObject agent) => true;
    public override bool Perform(GameObject agent)
    {
        Debug.Log("Villager marked resource for pickup!");
        marked = true;
        return true;
    }
}