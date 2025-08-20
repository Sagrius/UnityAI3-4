using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public interface IGoapAgent
{

    public abstract Transform GetTransform();
    public abstract string GetAgentName();
    public abstract NavMeshAgent getNavAgent();

    public abstract List<GoapAction> GetAvailableActions();

    public abstract void FindPlan();

    public abstract void ExecutePlan();

    public abstract void AbortPlan(string reason);
}