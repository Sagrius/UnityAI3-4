using UnityEngine;

public class MageAgent : GoapAgent
{
    protected override void BuildWorldState()
    {
        worldState.Clear();

        worldState["AtBuildSite"] = IsNearTagged("BuildLocation");
        worldState["CanBuildEnchantedStaff"] =
            ResourceManager.Instance.Has("OakLog", 5) &&
            ResourceManager.Instance.Has("IronIngot", 3);
        worldState["CanBuildRunedShield"] =
            ResourceManager.Instance.Has("CrystalShard", 4) &&
            ResourceManager.Instance.Has("IronIngot", 2);
        worldState["EnchantedStaffBuilt"] = ResourceManager.Instance.GetFact("EnchantedStaffBuilt");
        worldState["RunedShieldBuilt"] = ResourceManager.Instance.GetFact("RunedShieldBuilt");

        DebugLogWorldState();
    }

    protected override void SetGoal()
    {
        goal = new()
        {
            new("CombinedMagicalArtifactBuilt", true)
        };

    }
}
