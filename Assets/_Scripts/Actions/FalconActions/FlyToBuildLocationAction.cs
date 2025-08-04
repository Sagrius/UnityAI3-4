using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class FlyToBuildLocationAction : GoapAction
{
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
        var falcon = agent.GetComponent<FalconAgent>();
        if (!done && falcon != null && falcon.buildTarget != null)
        {
            agent.GetComponent<NavMeshAgent>().SetDestination(falcon.buildTarget.position);
            done = true;
        }

        return true;
    }

    public override bool IsDone() => done;
    public override void Reset() => done = false;
}
