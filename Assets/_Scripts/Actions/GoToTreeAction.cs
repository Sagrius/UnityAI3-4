using UnityEngine;

public class GoToTreeAction : GoapAction
{
    public GoToTreeAction() => AddEffect("atTree", true);
    public override void Reset() => Target = null;
    public override bool RequiresInRange() => true;
    public override bool CheckProceduralPrecondition(GameObject agent)
    {
        Target = ResourceManager.Instance.GetClosestResource(ResourceSource.ResourceType.Tree, agent.transform.position)?.gameObject;
        return Target != null;
    }
    // ### FIX ### The action is only "done" after it has been performed (i.e., upon arrival).
    public override bool Perform(GameObject agent) { SetDone(true); return true; }
}