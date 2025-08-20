using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "RetreatAction", menuName = "GOAP/Actions/Retreat")]
public class RetreatAction : GoapAction
{
    private Vector3 retreatPoint;

    void OnEnable()
    {
        ActionName = "Retreat";
        Effects.Clear();
        AddEffect(WorldStateKeys.IsSafe, true);
    }

    public override void OnReset() { }
    public override bool RequiresInRange() => true;

    public override bool CheckProceduralPrecondition(GoapAgent agent)
    {
        return agent.CombatStats.currentHealth < 30;
    }

    public override bool SetupAction(GoapAgent agent)
    {
        var enemies = FindObjectsOfType<CombatStats>().Where(cs => cs.agentType != agent.CombatStats.agentType).ToList();
        if (enemies.Count > 0)
        {
            var closestEnemy = enemies.OrderBy(e => Vector3.Distance(agent.transform.position, e.transform.position)).First();
            Vector3 fleeDirection = (agent.transform.position - closestEnemy.transform.position).normalized;
            retreatPoint = agent.transform.position + fleeDirection * 10f;
            Target = new GameObject("RetreatPoint");
            Target.transform.position = retreatPoint;
            return true;
        }
        return false;
    }

    public override bool Perform(GoapAgent agent)
    {
        if (Vector3.Distance(agent.transform.position, retreatPoint) < 1.5f)
        {
            Destroy(Target);
            SetDone(true);
        }
        return true;
    }
}