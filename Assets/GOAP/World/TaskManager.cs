using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }
    [SerializeField] private List<GoapGoal> tasks = new List<GoapGoal>();
    private List<GoapGoal> allTasks;

    private Dictionary<string, int> totalResourceRequirements = new Dictionary<string, int>();
    // (FIX) A new dictionary to track resources that are "in-progress" of being gathered.
    private Dictionary<string, int> inProgressResources = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
        allTasks = new List<GoapGoal>(tasks);

        totalResourceRequirements.Add("oakLogsInStockpile", 5);
        totalResourceRequirements.Add("ironIngotsInStockpile", 5);
        totalResourceRequirements.Add("crystalShardsInStockpile", 4);

        // Initialize the in-progress counts to zero.
        inProgressResources.Add("Logs", 0);
        inProgressResources.Add("Iron", 0);
        inProgressResources.Add("Crystals", 0);
    }

    // In TaskManager.cs

    public GoapGoal RequestTask(GoapAgent agent)
    {
        // === NEW CODE START ===
        // First, check if the final artifact has been built.
        // If it has, no more tasks should be assigned to any agent.
        object finalArtifactState = WorldState.Instance.GetState("combinedArtifactBuilt");
        if (finalArtifactState is bool && (bool)finalArtifactState)
        {
            return null; // Return null to stop assigning tasks.
        }
        // === NEW CODE END ===

        foreach (var task in allTasks.OrderByDescending(t => t.Priority))
        {
            if (!IsTaskStillNeeded(task))
            {
                continue;
            }

            if (IsAgentCapable(agent, task))
            {
                Debug.Log($"<color=yellow>[TaskManager] Assigning needed task '{task.GoalName}' to {agent.name}.</color>");

                if (task.GoalName.Contains("Logs")) inProgressResources["Logs"]++;
                if (task.GoalName.Contains("Iron")) inProgressResources["Iron"]++;
                if (task.GoalName.Contains("Crystals")) inProgressResources["Crystals"]++;

                return task;
            }
        }
        return null;
    }
    // In TaskManager.cs

    private bool IsTaskStillNeeded(GoapGoal goal)
    {
        var worldState = WorldState.Instance.GetWorldState();

        // 1. Check if the goal's effects are already present in the world state.
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
            return false; // Goal has already been achieved.
        }

        // 2. Check resource-specific gathering goals.
        if (goal.GoalName.Contains("Logs"))
        {
            int currentStock = (int)WorldState.Instance.GetState("oakLogsInStockpile");
            int onTheGround = WorldState.Instance.CountPickupsOfType(PickupLocation.ResourceType.Logs);
            return (currentStock + onTheGround + inProgressResources["Logs"]) < totalResourceRequirements["oakLogsInStockpile"];
        }
        if (goal.GoalName.Contains("Iron"))
        {
            int currentStock = (int)WorldState.Instance.GetState("ironIngotsInStockpile");
            int onTheGround = WorldState.Instance.CountPickupsOfType(PickupLocation.ResourceType.Iron);
            return (currentStock + onTheGround + inProgressResources["Iron"]) < totalResourceRequirements["ironIngotsInStockpile"];
        }
        if (goal.GoalName.Contains("Crystals"))
        {
            int currentStock = (int)WorldState.Instance.GetState("crystalShardsInStockpile");
            int onTheGround = WorldState.Instance.CountPickupsOfType(PickupLocation.ResourceType.Crystals);
            return (currentStock + onTheGround + inProgressResources["Crystals"]) < totalResourceRequirements["crystalShardsInStockpile"];
        }

        // 3. For all other goals (like CombineArtifacts), check their preconditions.
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

        // (FIX) If a gathering task fails, decrement the in-progress count.
        if (task.GoalName.Contains("Logs")) inProgressResources["Logs"]--;
        if (task.GoalName.Contains("Iron")) inProgressResources["Iron"]--;
        if (task.GoalName.Contains("Crystals")) inProgressResources["Crystals"]--;
    }

    // In TaskManager.cs

    public void CompleteTask(GoapGoal task)
    {
        if (task == null) return;
        Debug.Log($"[TaskManager] Task '{task.GoalName}' completed.");

        // DELETE OR COMMENT OUT THE CODE BLOCK BELOW
        /*
        if(task.GoalName.Contains("Logs")) inProgressResources["Logs"]--;
        if(task.GoalName.Contains("Iron")) inProgressResources["Iron"]--;
        if(task.GoalName.Contains("Crystals")) inProgressResources["Crystals"]--;
        */
    }
    // In TaskManager.cs

    public void NotifyResourceDelivered(PickupLocation.ResourceType type)
    {
        switch (type)
        {
            case PickupLocation.ResourceType.Logs:
                if (inProgressResources["Logs"] > 0) inProgressResources["Logs"]--;
                break;
            case PickupLocation.ResourceType.Iron:
                if (inProgressResources["Iron"] > 0) inProgressResources["Iron"]--;
                break;
            case PickupLocation.ResourceType.Crystals:
                if (inProgressResources["Crystals"] > 0) inProgressResources["Crystals"]--;
                break;
        }
    }
}
