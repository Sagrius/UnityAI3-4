using UnityEngine;

[CreateAssetMenu(fileName = "ChaseTargetAction", menuName = "GOAP/Actions/Chase Target")]
public class ChaseTargetAction : GoapAction
{
    void OnEnable()
    {
        ActionName = "Chase Target";
        Preconditions.Clear();
        Effects.Clear();
        AddPrecondition(WorldStateKeys.HasTarget, true);
        AddEffect(WorldStateKeys.TargetInAttackRange, true);
    }
    public override void OnReset() { }
    public override bool RequiresInRange() => true;
    public override bool CheckProceduralPrecondition(GoapAgent agent) => true;

    public override bool SetupAction(GoapAgent agent)
    {
        Target = agent.CurrentTarget;
        return Target != null;
    }

    public override bool Perform(GoapAgent agent)
    {
        SetDone(true);
        return true;
    }
}