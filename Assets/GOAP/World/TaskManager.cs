using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }
    [SerializeField] private List<GoapGoal> tasks = new List<GoapGoal>();
    private List<GoapGoal> availableTasks;

    private Dictionary<string, int> totalResourceRequirements = new Dictionary<string, int>();

    // (FIX) A new set to track which gathering goals are currently being worked on.
    private HashSet<GoapGoal> assignedGatheringTasks = new HashSet<GoapGoal>();

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
        availableTasks = tasks.OrderByDescending(t => t.Priority).ToList();

        totalResourceRequirements.Add("oakLogsInStockpile", 5);
        totalResourceRequirements.Add("ironIngotsInStockpile", 5);
        totalResourceRequirements.Add("crystalShardsInStockpile", 4);
    }

    public GoapGoal RequestTask(GoapAgent agent)
    {
        foreach (var task in availableTasks)
        {
            // (FIX) If this is a gathering task, check if it's already being worked on.
            if (IsGatheringGoal(task) && assignedGatheringTasks.Contains(task))
            {
                continue; // Skip, another agent is already on it.
            }

            if (!IsTaskStillNeeded(task))
            {
                continue;
            }

            if (IsAgentCapable(agent, task))
            {
                Debug.Log($"<color=yellow>[TaskManager] Assigning needed task '{task.GoalName}' to {agent.name}.</color>");

                // (FIX) If it's a gathering goal, "lock" it.
                if (IsGatheringGoal(task))
                {
                    assignedGatheringTasks.Add(task);
                }

                availableTasks.Remove(task);
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

        // (FIX) "Unlock" the task if it failed.
        if (IsGatheringGoal(task))
        {
            assignedGatheringTasks.Remove(task);
        }

        availableTasks.Add(task);
        availableTasks = availableTasks.OrderByDescending(t => t.Priority).ToList();
    }

    public void CompleteTask(GoapGoal task)
    {
        if (task == null) return;
        Debug.Log($"[TaskManager] Task '{task.GoalName}' completed and returned to the pool.");

        // (FIX) "Unlock" the task upon completion.
        if (IsGatheringGoal(task))
        {
            assignedGatheringTasks.Remove(task);
        }

        availableTasks.Add(task);
        availableTasks = availableTasks.OrderByDescending(t => t.Priority).ToList();
    }

    // A helper method to identify gathering goals.
    private bool IsGatheringGoal(GoapGoal goal)
    {
        return goal.GoalName.Contains("Logs") || goal.GoalName.Contains("Iron") || goal.GoalName.Contains("Crystals");
    }
}