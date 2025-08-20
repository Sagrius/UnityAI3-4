using UnityEngine;

[CreateAssetMenu(fileName = "AttackAgentAction", menuName = "GOAP/Actions/Attack Agent")]
public class AttackAgentAction : GoapAction
{
    private float attackCooldown = 1.5f;
    private float timer;

    void OnEnable()
    {
        ActionName = "Attack Agent";
        Preconditions.Clear();
        Effects.Clear();
        AddPrecondition(WorldStateKeys.TargetInAttackRange, true);
        AddEffect(WorldStateKeys.TargetAttacked, true);
    }

    public override void OnReset()
    {
        timer = 0;
    }

    public override bool RequiresInRange() => true;
    public override bool CheckProceduralPrecondition(GoapAgent agent) => true;

    public override bool SetupAction(GoapAgent agent)
    {
        Target = agent.CurrentTarget;
        return Target != null;
    }

    public override bool Perform(GoapAgent agent)
    {
        if (Target == null)
        {
            SetDone(true);
            return false;
        }

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            Debug.Log($"<color=red>[{agent.name}] attacks {Target.name}!</color>");
            CombatStats targetStats = Target.GetComponent<CombatStats>();
            if (targetStats != null)
            {
                targetStats.TakeDamage(agent.CombatStats.attackPower);
            }
            timer = attackCooldown;
        }

        SetDone(true);
        return true;
    }
}