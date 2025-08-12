using UnityEngine;
public class CraftRunedShieldAction : GoapAction
{
    public CraftRunedShieldAction()
    {
        ActionName = "Craft Runed Shield";
        AddEffect("runedShieldBuilt", true);
    }

    public override void OnReset() { Target = null; }
    public override bool RequiresInRange() => true;

    public override bool CheckProceduralPrecondition(GoapAgent agent)
    {
        Target = ResourceManager.Instance.BuildLocation.gameObject;
        int crystals = (int)WorldState.Instance.GetState("crystalShardsInStockpile");
        int iron = (int)WorldState.Instance.GetState("ironIngotsInStockpile");
        return Target != null && crystals >= 4 && iron >= 2;
    }

    public override bool Perform(GoapAgent agent)
    {
        Debug.Log($"[{agent.name}] is crafting the Runed Shield!");
        WorldState.Instance.ModifyState("crystalShardsInStockpile", -4);
        WorldState.Instance.ModifyState("ironIngotsInStockpile", -2);

        // This is the missing line that tells the world the shield is done.
        WorldState.Instance.SetState("runedShieldBuilt", true);

        SetDone(true);
        return true;
    }
}