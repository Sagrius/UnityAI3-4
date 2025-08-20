using UnityEngine;

public abstract class GenericCraftingAction : GoapAction
{
    protected CraftingRecipe recipe;

    public override void OnReset() { Target = null; }
    public override bool RequiresInRange() => true;

    public override bool CheckProceduralPrecondition(IGoapAgent agent)
    {
        Target = ResourceManager.Instance.BuildLocation.gameObject;
        if (Target == null || recipe == null) return false;

        foreach (var cost in recipe.requiredResources)
        {
            object val = WorldState.Instance.GetState(cost.resourceKey);
            if (val == null || (int)val < cost.amount)
            {
                return false; 
            }
        }
        return true;
    }

    public override bool Perform(IGoapAgent agent)
    {
        Debug.Log($"[{agent.GetAgentName()}] is crafting the {recipe.name}!");

        foreach (var cost in recipe.requiredResources)
        {
            WorldState.Instance.ModifyState(cost.resourceKey, -cost.amount);
        }

        WorldState.Instance.SetState(recipe.craftedItemKey, true);

        SetDone(true);
        return true;
    }
}