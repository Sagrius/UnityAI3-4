using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }
    [SerializeField] private List<GoapGoal> tasks = new List<GoapGoal>();
    [SerializeField] private List<CraftingRecipe> allCraftingRecipes = new List<CraftingRecipe>();

    [Header("Spawning")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject healthPotionPrefab;
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private int numberOfEnemies = 4;
    [SerializeField] private int maxPotions = 3;
    [SerializeField] private float potionSpawnRate = 15f;

    private List<GoapGoal> allTasks;
    private Dictionary<PickupLocation.ResourceType, int> inProgressResources = new Dictionary<PickupLocation.ResourceType, int>();
    private Dictionary<string, int> totalResourceDemand = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
        allTasks = new List<GoapGoal>(tasks);

        inProgressResources.Add(PickupLocation.ResourceType.Logs, 0);
        inProgressResources.Add(PickupLocation.ResourceType.Iron, 0);
        inProgressResources.Add(PickupLocation.ResourceType.Crystals, 0);
    }

    void Start()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            if (enemyPrefab != null && spawnPoints.Count > 0)
            {
                Instantiate(enemyPrefab, spawnPoints[Random.Range(0, spawnPoints.Count)].position, Quaternion.identity);
            }
        }
        StartCoroutine(SpawnPotionsRoutine());
    }

    private System.Collections.IEnumerator SpawnPotionsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(potionSpawnRate);

            int currentPotionCount = FindObjectsOfType<HealthPotion>().Length;
            if (currentPotionCount < maxPotions)
            {
                if (healthPotionPrefab != null && spawnPoints.Count > 0)
                {
                    Debug.Log("Spawning a new health potion.");
                    Instantiate(healthPotionPrefab, spawnPoints[Random.Range(0, spawnPoints.Count)].position, Quaternion.identity);
                }
            }
        }
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
            if (itemState is bool && (bool)itemState) continue;

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

    public GoapGoal RequestTask(GoapAgent agent)
    {
        object finalArtifactState = WorldState.Instance.GetState(WorldStateKeys.CombinedArtifactBuilt);
        if (finalArtifactState is bool && (bool)finalArtifactState) return null;

        // *** THE FIX: Self-Preservation Override ***
        if (agent.CombatStats.IsUnderAttack || agent.CombatStats.currentHealth < 30)
        {
            var retreatGoal = allTasks.FirstOrDefault(g => g.GoalName == "Retreat");
            if (retreatGoal != null && IsAgentCapable(agent, retreatGoal)) return retreatGoal;

            var attackGoal = allTasks.FirstOrDefault(g => g.GoalName == "Attack Target");
            if (attackGoal != null && IsAgentCapable(agent, attackGoal)) return attackGoal;
        }

        if (agent.CombatStats.currentHealth < agent.CombatStats.healingThreshold)
        {
            var healGoal = allTasks.FirstOrDefault(g => g.GoalName == "Heal");
            if (healGoal != null && IsAgentCapable(agent, healGoal)) return healGoal;
        }

        if (IsVillager(agent))
        {
            return GetFocusedVillagerTask(agent);
        }

        foreach (var task in allTasks.OrderByDescending(t => t.Priority))
        {
            if (IsVillagerResourceTask(task)) continue;

            if (IsTaskStillNeeded(task) && IsAgentCapable(agent, task))
            {
                return task;
            }
        }
        return null;
    }

    private GoapGoal GetFocusedVillagerTask(GoapAgent agent)
    {
        PickupLocation.ResourceType? priorityType = GetPriorityResourceType();
        if (!priorityType.HasValue) return null;

        ResourceSource.ResourceType sourceType = GetSourceTypeForResourceType(priorityType.Value);
        int unclaimedSources = ResourceManager.Instance.GetUnclaimedResourceCount(sourceType);

        if (unclaimedSources > 0)
        {
            GoapGoal task = GetGoalForResourceType(priorityType.Value);
            if (task != null && IsAgentCapable(agent, task))
            {
                inProgressResources[priorityType.Value]++;
                return task;
            }
        }
        return null;
    }

    private PickupLocation.ResourceType? GetPriorityResourceType()
    {
        int logDeficit = GetStableDeficit(WorldStateKeys.LogsInStockpile, PickupLocation.ResourceType.Logs);
        int ironDeficit = GetStableDeficit(WorldStateKeys.IronInStockpile, PickupLocation.ResourceType.Iron);
        int crystalDeficit = GetStableDeficit(WorldStateKeys.CrystalsInStockpile, PickupLocation.ResourceType.Crystals);

        if (logDeficit <= 0 && ironDeficit <= 0 && crystalDeficit <= 0) return null;

        if (logDeficit > 0) return PickupLocation.ResourceType.Logs;
        if (ironDeficit > 0) return PickupLocation.ResourceType.Iron;
        if (crystalDeficit > 0) return PickupLocation.ResourceType.Crystals;

        return null;
    }

    private int GetStableDeficit(string resourceKey, PickupLocation.ResourceType type)
    {
        totalResourceDemand.TryGetValue(resourceKey, out int demand);
        int currentStock = (int)WorldState.Instance.GetState(resourceKey);
        int onTheGround = WorldState.Instance.CountPickupsOfType(type);
        return demand - (currentStock + onTheGround);
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
        if (allConditionsMet) return false;

        return true;
    }

    private bool IsVillager(GoapAgent agent) => agent.GetAvailableActions().Any(a => a is PrepareAndDropResourceAction);
    private bool IsVillagerResourceTask(GoapGoal goal) => goal.GoalName.Contains("Logs") || goal.GoalName.Contains("Iron") || goal.GoalName.Contains("Crystals");
    private GoapGoal GetGoalForResourceType(PickupLocation.ResourceType type)
    {
        if (type == PickupLocation.ResourceType.Logs) return allTasks.FirstOrDefault(g => g.GoalName.Contains("Logs"));
        if (type == PickupLocation.ResourceType.Iron) return allTasks.FirstOrDefault(g => g.GoalName.Contains("Iron"));
        if (type == PickupLocation.ResourceType.Crystals) return allTasks.FirstOrDefault(g => g.GoalName.Contains("Crystals"));
        return null;
    }
    private PickupLocation.ResourceType? GetResourceTypeForGoal(GoapGoal goal)
    {
        if (goal.GoalName.Contains("Logs")) return PickupLocation.ResourceType.Logs;
        if (goal.GoalName.Contains("Iron")) return PickupLocation.ResourceType.Iron;
        if (goal.GoalName.Contains("Crystals")) return PickupLocation.ResourceType.Crystals;
        return null;
    }
    private ResourceSource.ResourceType GetSourceTypeForResourceType(PickupLocation.ResourceType resType)
    {
        switch (resType)
        {
            case PickupLocation.ResourceType.Logs: return ResourceSource.ResourceType.Tree;
            case PickupLocation.ResourceType.Iron: return ResourceSource.ResourceType.Mine;
            case PickupLocation.ResourceType.Crystals: return ResourceSource.ResourceType.CrystalCavern;
            default: throw new System.ArgumentOutOfRangeException();
        }
    }
    private string GetKeyForResourceType(PickupLocation.ResourceType type)
    {
        if (type == PickupLocation.ResourceType.Logs) return WorldStateKeys.LogsInStockpile;
        if (type == PickupLocation.ResourceType.Iron) return WorldStateKeys.IronInStockpile;
        if (type == PickupLocation.ResourceType.Crystals) return WorldStateKeys.CrystalsInStockpile;
        return "";
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

    private bool IsAgentCapable(GoapAgent agent, GoapGoal goal)
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

        var resourceType = GetResourceTypeForGoal(task);
        if (resourceType.HasValue)
        {
            if (inProgressResources[resourceType.Value] > 0)
                inProgressResources[resourceType.Value]--;
        }
    }

    public void CompleteTask(GoapGoal task)
    {
        if (task == null) return;
        Debug.Log($"[TaskManager] Task '{task.GoalName}' completed.");

        var resourceType = GetResourceTypeForGoal(task);
        if (resourceType.HasValue)
        {
            if (inProgressResources[resourceType.Value] > 0)
            {
                inProgressResources[resourceType.Value]--;
            }
        }
    }
}