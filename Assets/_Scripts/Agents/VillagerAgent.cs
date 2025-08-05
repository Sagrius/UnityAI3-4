using UnityEngine;

public class VillagerAgent : GoapAgent
{
    protected override void BuildWorldState()
    {
        worldState.Clear();

        worldState["IsIdle"] = actionQueue == null || actionQueue.Count == 0;
        worldState["AtForest"] = IsNearTagged("Tree");
        worldState["AtMine"] = IsNearTagged("Mine");
        worldState["AtCrystalCave"] = IsNearTagged("Crystal");

        worldState["FalconAvailable"] = ResourceManager.Instance.GetFact("FalconAvailable");
        worldState["ResourceReady"] = ResourceManager.Instance.GetFact("ResourceReady");

        DebugLogWorldState();
    }

    protected override void SetGoal()
    {
        goal = new()
        {
            new("ResourceReady", true)
        };

    }
}


