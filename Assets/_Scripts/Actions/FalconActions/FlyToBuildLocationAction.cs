using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class FlyToBuildLocationAction : GoapAction
{
    private NavMeshAgent _navMesh;
    private void Awake()
    {
        _navMesh = transform.root.GetComponent<NavMeshAgent>();
    }
    public override HashSet<KeyValuePair<string, object>> Preconditions => new()
    {
        new("CarryingResource", true)
    };

    public override HashSet<KeyValuePair<string, object>> Effects => new()
    {
        new("ResourceDelivered", true)
    };

    public override bool IsValid(GameObject agent) => true;

    public override bool Perform(GameObject agent)
    {
        var falcon = agent.GetComponent<FalconGoapAgent>();
        if (!done && falcon != null && falcon.buildTarget != null)
        {
           _navMesh.SetDestination(falcon.buildTarget.position);
            done = true;
        }

        return true;
    }

    public override bool IsDone() => done;
    public override void Reset() => done = false;
}
