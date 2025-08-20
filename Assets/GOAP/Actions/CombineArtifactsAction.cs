using UnityEngine;

[CreateAssetMenu(fileName = "CombineArtifactsAction", menuName = "GOAP/Actions/Combine Artifacts")]
public class CombineArtifactsAction : GoapAction
{
    void OnEnable()
    {
        ActionName = "Combine Artifacts";
        Preconditions.Clear();
        Effects.Clear();
        AddPrecondition(WorldStateKeys.EnchantedStaffBuilt, true);
        AddPrecondition(WorldStateKeys.RunedShieldBuilt, true);
        AddEffect(WorldStateKeys.CombinedArtifactBuilt, true);
    }

    public override void OnReset() { Target = null; }
    public override bool RequiresInRange() => true;

    public override bool CheckProceduralPrecondition(GoapAgent agent)
    {
        return ResourceManager.Instance.BuildLocation != null;
    }

    public override bool SetupAction(GoapAgent agent)
    {
        Target = ResourceManager.Instance.BuildLocation.gameObject;
        return Target != null;
    }

    public override bool Perform(GoapAgent agent)
    {
        Debug.Log($"<color=green>[{agent.name}] is combining the artifacts... The kingdom is saved!</color>");
        WorldState.Instance.SetState(WorldStateKeys.CombinedArtifactBuilt, true);
        SetDone(true);
        return true;
    }
}