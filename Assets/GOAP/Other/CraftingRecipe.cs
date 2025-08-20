using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ResourceCost
{
    [Tooltip("The WorldStateKey for the resource.")]
    public string resourceKey;
    [Tooltip("The amount of the resource required.")]
    public int amount;
}

[CreateAssetMenu(fileName = "NewCraftingRecipe", menuName = "GOAP/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [Tooltip("The WorldStateKey for the item that gets created.")]
    public string craftedItemKey;
    [Tooltip("A list of all resources and their amounts needed for this recipe.")]
    public List<ResourceCost> requiredResources;
}