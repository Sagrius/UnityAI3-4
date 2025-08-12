using UnityEngine;
public class CombineArtifactsAction : GoapAction
{
    public CombineArtifactsAction()
    {
        ActionName = "Combine Artifacts";
        AddPrecondition("enchantedStaffBuilt", true);
        AddPrecondition("runedShieldBuilt", true);
        AddEffect("combinedArtifactBuilt", true);
    }

    public override void OnReset() { Target = null; }
    public override bool RequiresInRange() => true;

    public override bool CheckProceduralPrecondition(GoapAgent agent)
    {
        Target = ResourceManager.Instance.BuildLocation.gameObject;
        return Target != null;
    }

    public override bool Perform(GoapAgent agent)
    {
        Debug.Log($"<color=green>[{agent.name}] is combining the artifacts... The kingdom is saved!</color>");
        SetDone(true);
        return true;
    }
}