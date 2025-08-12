using UnityEngine;
using System.Linq;

// #############################################################################
// # ABSTRACT GATHER ACTION
// # This is a base class to make creating new gathering actions easier.
// #############################################################################

public abstract class GatherResourceAction : GoapAction
{
    // --- CONFIGURATION ---
    protected ResourceSource.ResourceType resourceType;
    protected string blackboardKeyForResource; // e.g., "numLogs"
    protected int amountToGather;

    // --- STATE ---
    private ResourceSource targetResourceSource;

    /// <summary>
    /// Resets the action's state.
    /// </summary>
    public override void OnReset()
    {
        Target = null;
        targetResourceSource = null;
    }

    /// <summary>
    /// This action requires the agent to be in range of its target.
    /// </summary>
    public override bool RequiresInRange() => true;

    /// <summary>
    /// Checks if a valid resource source is available in the world.
    /// </summary>
    public override bool CheckProceduralPrecondition(GoapAgent agent)
    {
        // Find all sources of the correct type that have enough quantity.
        var sources = ResourceManager.Instance.ResourceSources
            .Where(s => s.Type == resourceType && s.quantity >= amountToGather)
            .ToList();

        if (sources.Count == 0) return false; // No valid sources found.

        targetResourceSource = ResourceManager.Instance.GetClosestResource(sources, agent.transform.position);

        if (targetResourceSource != null)
        {
            Target = targetResourceSource.gameObject;
            // Store the target in the agent's local memory for the Perform step.
            agent.Blackboard.SetState("currentTarget", targetResourceSource);
            return true;
        }

        return false;
    }

    /// <summary>
    /// The logic to execute when the agent is in range of the resource.
    /// </summary>
    public override bool Perform(GoapAgent agent)
    {
        // Double-check the target is still valid (another agent might have taken it).
        if (targetResourceSource == null || targetResourceSource.quantity < amountToGather)
        {
            return false; // Abort the plan.
        }

        // Simulate gathering.
        targetResourceSource.quantity -= amountToGather;
        agent.Blackboard.SetState(blackboardKeyForResource, amountToGather);
        agent.Blackboard.SetState("hasPreparedResource", true); // The agent is now holding something.

        Debug.Log($"[{agent.name}] gathered {amountToGather} of {resourceType}.");
        SetDone(true);
        return true;
    }
}