using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }
    [SerializeField] private List<GoapGoal> tasks = new List<GoapGoal>();
    private List<GoapGoal> availableTasks;

    // (FIX) The TaskManager is now the "meta-agent" that knows the total project requirements.
    private Dictionary<string, int> totalResourceRequirements = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
        availableTasks = tasks.OrderByDescending(t => t.Priority).ToList();

        // Hardcode the total requirements based on the brief.
        totalResourceRequirements.Add("oakLogsInStockpile", 5);
        totalResourceRequirements.Add("ironIngotsInStockpile", 5); // 3 for staff + 2 for shield
        totalResourceRequirements.Add("crystalShardsInStockpile", 4);
    }

    public GoapGoal RequestTask(GoapAgent agent)
    {
        // We no longer need to get the world state here, the helper method will do it.
        // var worldState = WorldState.Instance.GetWorldState();
        foreach (var task in availableTasks)
        {
            // (FIX) New check to see if we still need the resources this goal provides.
            if (!IsTaskStillNeeded(task))
            {
                continue;
            }

            // We can simplify this line as IsTaskStillNeeded already checks preconditions implicitly.
            if (IsAgentCapable(agent, task))
            {
                Debug.Log($"<color=yellow>[TaskManager] Assigning needed task '{task.GoalName}' to {agent.name}.</color>");
                availableTasks.Remove(task);
                return task;
            }
        }
        return null;
    }

    // This new method acts as the high-level brain.
    private bool IsTaskStillNeeded(GoapGoal goal)
    {
        // This logic is specific to our "Prepare" goals.
        // It's a bit brittle based on names, but effective for this project scope.
        if (goal.GoalName.Contains("Logs"))
        {
            int currentStock = (int)WorldState.Instance.GetState("oakLogsInStockpile");
            int onTheGround = WorldState.Instance.CountPickupsOfType(PickupLocation.ResourceType.Logs);
            return (currentStock + onTheGround) < totalResourceRequirements["oakLogsInStockpile"];
        }
        if (goal.GoalName.Contains("Iron"))
        {
            int currentStock = (int)WorldState.Instance.GetState("ironIngotsInStockpile");
            int onTheGround = WorldState.Instance.CountPickupsOfType(PickupLocation.ResourceType.Iron);
            return (currentStock + onTheGround) < totalResourceRequirements["ironIngotsInStockpile"];
        }
        if (goal.GoalName.Contains("Crystals"))
        {
            int currentStock = (int)WorldState.Instance.GetState("crystalShardsInStockpile");
            int onTheGround = WorldState.Instance.CountPickupsOfType(PickupLocation.ResourceType.Crystals);
            return (currentStock + onTheGround) < totalResourceRequirements["crystalShardsInStockpile"];
        }

        // For all other goals (like crafting), we check if their preconditions are met.
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
        Debug.LogWarning($"[TaskManager] Task '{task.GoalName}' failed and was returned. Adding back to queue.");
        availableTasks.Add(task);
        availableTasks = availableTasks.OrderByDescending(t => t.Priority).ToList();
    }

    public void CompleteTask(GoapGoal task)
    {
        if (task == null) return;
        Debug.Log($"[TaskManager] Task '{task.GoalName}' completed and returned to the pool.");
        availableTasks.Add(task);
        availableTasks = availableTasks.OrderByDescending(t => t.Priority).ToList();
    }
}