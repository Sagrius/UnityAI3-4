using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "GetHealingPotionAction", menuName = "GOAP/Actions/Get Healing Potion")]
public class GetHealingPotionAction : GoapAction
{
    [System.NonSerialized] private HealthPotion potion;

    void OnEnable()
    {
        ActionName = "Get Healing Potion";
        Effects.Clear();
        AddEffect(WorldStateKeys.IsHealed, true);
    }

    public override void OnReset()
    {
        potion = null;
    }

    public override bool RequiresInRange() => true;

    public override bool CheckProceduralPrecondition(GoapAgent agent)
    {
        // Agent will only consider this action if its health is below the threshold
        return agent.CombatStats.currentHealth < agent.CombatStats.healingThreshold && FindObjectOfType<HealthPotion>() != null;
    }

    public override bool SetupAction(GoapAgent agent)
    {
        var potions = FindObjectsOfType<HealthPotion>();
        if (potions.Length > 0)
        {
            potion = potions.OrderBy(p => Vector3.Distance(agent.transform.position, p.transform.position)).First();
            Target = potion.gameObject;
            return true;
        }
        return false;
    }

    public override bool Perform(GoapAgent agent)
    {
        if (potion == null)
        {
            SetDone(true);
            return false;
        }

        agent.CombatStats.Heal(potion.healAmount);
        Destroy(potion.gameObject); // Consume the potion
        SetDone(true);
        return true;
    }
}