using UnityEngine;

public abstract class GenericCraftingAction : GoapAction
{
    // Assign the specific recipe for this item in the derived class or inspector
    protected CraftingRecipe recipe;

    public override void OnReset() { Target = null; }
    public override bool RequiresInRange() => true;

    public override bool CheckProceduralPrecondition(IGoapAgent agent)
    {
        Target = ResourceManager.Instance.BuildLocation.gameObject;
        if (Target == null || recipe == null) return false;

        // Check if all required resources are available in the stockpile
        foreach (var cost in recipe.requiredResources)
        {
            object val = WorldState.Instance.GetState(cost.resourceKey);
            if (val == null || (int)val < cost.amount)
            {
                return false; // Not enough of this resource
            }
        }
        return true;
    }

    public override bool Perform(IGoapAgent agent)
    {
        Debug.Log($"[{agent.GetAgentName()}] is crafting the {recipe.name}!");

        // Consume resources
        foreach (var cost in recipe.requiredResources)
        {
            WorldState.Instance.ModifyState(cost.resourceKey, -cost.amount);
        }

        // Add effect to world state
        WorldState.Instance.SetState(recipe.craftedItemKey, true);

        SetDone(true);
        return true;
    }
}