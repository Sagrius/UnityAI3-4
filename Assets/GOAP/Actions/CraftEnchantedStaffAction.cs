using UnityEngine;
public class CraftEnchantedStaffAction : GoapAction
{
    public CraftEnchantedStaffAction()
    {
        ActionName = "Craft Enchanted Staff";
        AddEffect("enchantedStaffBuilt", true);

    }

    public override void OnReset() { Target = null; }
    public override bool RequiresInRange() => true;

    // In CraftEnchantedStaffAction.cs

    public override bool CheckProceduralPrecondition(GoapAgent agent)
    {
        Target = ResourceManager.Instance.BuildLocation.gameObject;
        int logs = (int)WorldState.Instance.GetState("oakLogsInStockpile");
        int iron = (int)WorldState.Instance.GetState("ironIngotsInStockpile");

        // Add or uncomment this Debug line
        Debug.Log($"[Mage Craft Staff Check] Has Target: {Target != null}, Logs: {logs}/5, Iron: {iron}/3");

        return Target != null && logs >= 5 && iron >= 3;
    }

    public override bool Perform(GoapAgent agent)
    {
        Debug.Log($"[{agent.name}] is crafting the Enchanted Staff!");
        WorldState.Instance.ModifyState("oakLogsInStockpile", -5);
        WorldState.Instance.ModifyState("ironIngotsInStockpile", -3);

        // This is the missing line that tells the world the staff is done.
        WorldState.Instance.SetState("enchantedStaffBuilt", true);

        SetDone(true);
        return true;
    }
}