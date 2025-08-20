using UnityEngine;

[CreateAssetMenu(fileName = "NewCraftItemAction", menuName = "GOAP/Actions/Craft Item")]
public class CraftItemAction : GoapAction
{
    [Tooltip("The recipe this action will use to check for and consume resources.")]
    public CraftingRecipe recipe;

    void OnEnable()
    {
        if (recipe == null) return;
        ActionName = $"Craft {recipe.name}";
        Effects.Clear();
        AddEffect(recipe.craftedItemKey, true);
    }

    public override void OnReset() { Target = null; }
    public override bool RequiresInRange() => true;

    public override bool CheckProceduralPrecondition(IGoapAgent agent)
    {
        if (recipe == null)
        {
            Debug.LogError($"{name}: Recipe is not assigned for this action asset!");
            return false;
        }

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

    public override bool SetupAction(IGoapAgent agent)
    {
        Target = ResourceManager.Instance.BuildLocation.gameObject;
        return Target != null;
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