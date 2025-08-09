using UnityEngine;

public class GoToMineAction : GoapAction
{
    public GoToMineAction() => AddEffect("atMine", true);
    public override void Reset() => Target = null;
    public override bool RequiresInRange() => true;
    public override bool CheckProceduralPrecondition(GameObject agent)
    {
        Target = ResourceManager.Instance.GetClosestResource(ResourceSource.ResourceType.Mine, agent.transform.position)?.gameObject;
        return Target != null;
    }
    public override bool Perform(GameObject agent) { SetDone(true); return true; }
}