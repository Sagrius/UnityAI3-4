using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SearchForTargetAction", menuName = "GOAP/Actions/Search For Target")]
public class SearchForTargetAction : GoapAction
{
    void OnEnable()
    {
        ActionName = "Search For Target";
        Effects.Clear();
        AddEffect(WorldStateKeys.HasTarget, true);
    }

    public override void OnReset()
    {
        Target = null;
    }

    public override bool RequiresInRange() => false;

    public override bool CheckProceduralPrecondition(GoapAgent agent)
    {
        var potentialTargets = FindObjectsOfType<CombatStats>()
            .Where(cs => cs.agentType != CombatStats.AgentType.Enemy && cs.gameObject != agent.gameObject)
            .ToList();

        return potentialTargets.Count > 0;
    }

    public override bool SetupAction(GoapAgent agent)
    {
        var potentialTargets = FindObjectsOfType<CombatStats>()
            .Where(cs => cs.agentType != CombatStats.AgentType.Enemy && cs.gameObject != agent.gameObject)
            .OrderBy(cs => Vector3.Distance(agent.transform.position, cs.transform.position))
            .ToList();

        if (potentialTargets.Count > 0)
        {
            // Set the target on the action itself for internal use
            Target = potentialTargets[0].gameObject;
            return true;
        }
        return false;
    }

    public override bool Perform(GoapAgent agent)
    {
        // Pass the found target to the agent so other actions can use it
        agent.CurrentTarget = Target;
        SetDone(true);
        return true;
    }
}