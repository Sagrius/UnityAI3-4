using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }
    [SerializeField] private List<GoapGoal> tasks = new List<GoapGoal>();
    [Tooltip("A list of all possible items that can be crafted in the game.")]
    [SerializeField] private List<CraftingRecipe> allCraftingRecipes = new List<CraftingRecipe>();

    private List<GoapGoal> allTasks;
    private Dictionary<PickupLocation.ResourceType, int> inProgressResources = new Dictionary<PickupLocation.ResourceType, int>();
    private Dictionary<string, int> totalResourceDemand = new Dictionary<string, int>();


    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
        allTasks = new List<GoapGoal>(tasks);

    }

    void Update()
    {
        CalculateTotalResourceDemand();
    }

    private void CalculateTotalResourceDemand()
    {
        totalResourceDemand.Clear();
        foreach (var recipe in allCraftingRecipes)
        {
            object itemState = WorldState.Instance.GetState(recipe.craftedItemKey);
            if (itemState is bool && (bool)itemState)
            {
                continue;
            }

            foreach (var cost in recipe.requiredResources)
            {
                if (totalResourceDemand.ContainsKey(cost.resourceKey))
                {
                    totalResourceDemand[cost.resourceKey] += cost.amount;
                }
                else
                {
                    totalResourceDemand.Add(cost.resourceKey, cost.amount);
                }
            }
        }
    }

    public GoapGoal RequestTask(IGoapAgent agent)
    {
        object finalArtifactState = WorldState.Instance.GetState(WorldStateKeys.CombinedArtifactBuilt);
        if (finalArtifactState is bool && (bool)finalArtifactState)
        {
            return null;
        }

        foreach (var task in allTasks.OrderByDescending(t => t.Priority))
        {
            if (!IsTaskStillNeeded(task))
            {
                continue;
            }

            if (IsAgentCapable(agent, task))
            {
                Debug.Log($"<color=yellow>[TaskManager] Assigning needed task '{task.GoalName}' to {agent.GetAgentName()}.</color>");


                return task;
            }
        }
        return null;
    }

    private bool IsTaskStillNeeded(GoapGoal goal)
    {
        var worldState = WorldState.Instance.GetWorldState();

        var goalState = goal.GetGoalState();
        bool allConditionsMet = true;
        foreach (var condition in goalState)
        {
            if (!worldState.Contains(condition))
            {
                allConditionsMet = false;
                break;
            }
        }
        if (allConditionsMet)
        {
            return false;
        }

        

        return ArePreconditionsMet(goal.GetPreconditions(), worldState);
    }

    private bool ArePreconditionsMet(HashSet<KeyValuePair<string, object>> preconditions, HashSet<KeyValuePair<string, object>> worldState)
    {
        if (preconditions == null || preconditions.Count == 0) return true;
        foreach (var precon in preconditions)
        {
            if (!worldState.Contains(precon)) return false;
        }
        return true;
    }

    private bool IsAgentCapable(IGoapAgent agent, GoapGoal goal)
    {
        var agentActions = agent.GetAvailableActions();
        var goalState = goal.GetGoalState();
        foreach (var action in agentActions)
        {
            foreach (var effect in action.Effects)
            {
                if (goalState.Contains(effect)) return true;
            }
        }
        return false;
    }

    public void FailTask(GoapGoal task)
    {
        if (task == null) return;
        Debug.LogWarning($"[TaskManager] Task '{task.GoalName}' failed.");
    }

    public void CompleteTask(GoapGoal task)
    {
        if (task == null) return;
        Debug.Log($"[TaskManager] Task '{task.GoalName}' completed.");
    }

    public void NotifyResourceDelivered(PickupLocation.ResourceType type)
    {
        if (inProgressResources.ContainsKey(type) && inProgressResources[type] > 0)
        {
            inProgressResources[type]--;
        }
    }
}