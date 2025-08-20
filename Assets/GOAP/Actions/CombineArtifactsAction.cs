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

    public override bool CheckProceduralPrecondition(IGoapAgent agent)
    {
        // This action's preconditions are handled by the planner, so we just need to know
        // if the build location exists.
        return ResourceManager.Instance.BuildLocation != null;
    }

    public override bool SetupAction(IGoapAgent agent)
    {
        // For execution, set the target to the build location.
        Target = ResourceManager.Instance.BuildLocation.gameObject;
        return Target != null;
    }

    public override bool Perform(IGoapAgent agent)
    {
        Debug.Log($"<color=green>[{agent.GetAgentName()}] is combining the artifacts... The kingdom is saved!</color>");
        WorldState.Instance.SetState(WorldStateKeys.CombinedArtifactBuilt, true);
        SetDone(true);
        return true;
    }
}