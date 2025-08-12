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

    public GoapGoal RequestTask(GoapAgent agent)
    {
        foreach (var task in allTasks.OrderByDescending(t => t.Priority))
        {
            if (!IsTaskStillNeeded(task))
            {
                continue;
            }

            if (IsAgentCapable(agent, task))
            {
                Debug.Log($"<color=yellow>[TaskManager] Assigning needed task '{task.GoalName}' to {agent.name}.</color>");

                // (FIX) When assigning a gathering task, increment the in-progress count.
                if (task.GoalName.Contains("Logs")) inProgressResources["Logs"]++;
                if (task.GoalName.Contains("Iron")) inProgressResources["Iron"]++;
                if (task.GoalName.Contains("Crystals")) inProgressResources["Crystals"]++;

                return task;
            }
        }
        return null;
    }

    private bool IsTaskStillNeeded(GoapGoal goal)
    {
        if (goal.GoalName.Contains("Logs"))
        {
            int currentStock = (int)WorldState.Instance.GetState("oakLogsInStockpile");
            int onTheGround = WorldState.Instance.CountPickupsOfType(PickupLocation.ResourceType.Logs);
            // (FIX) Now includes the in-progress count in its calculation.
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

        return ArePreconditionsMet(goal.GetPreconditions(), WorldState.Instance.GetWorldState());
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

    public void CompleteTask(GoapGoal task)
    {
        if (task == null) return;
        Debug.Log($"[TaskManager] Task '{task.GoalName}' completed.");

        // (FIX) When a resource is successfully dropped, decrement the in-progress count.
        // This is now handled by the action itself, but we'll keep this as a fallback.
        if (task.GoalName.Contains("Logs")) inProgressResources["Logs"]--;
        if (task.GoalName.Contains("Iron")) inProgressResources["Iron"]--;
        if (task.GoalName.Contains("Crystals")) inProgressResources["Crystals"]--;
    }
}
