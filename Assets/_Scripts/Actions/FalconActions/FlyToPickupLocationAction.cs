using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class FlyToPickupLocationAction : GoapAction
{
    public override HashSet<KeyValuePair<string, object>> Preconditions => new()
    {
        new("PickupTaskAssigned", true)
    };

    public override HashSet<KeyValuePair<string, object>> Effects => new()
    {
        new("AtPickupLocation", true)
    };

    public override bool IsValid(GameObject agent) => true;

    public override bool Perform(GameObject agent)
    {
        if (!done)
        {
            var falcon = agent.GetComponent<FalconAgent>();
            if (falcon != null && falcon.pickupTarget != null)
            {
                agent.GetComponent<NavMeshAgent>().SetDestination(falcon.pickupTarget.position);
                done = true;
            }
            else
            {
                Debug.LogWarning("Falcon: No pickupTarget assigned.");
            }
        }

        return true;
    }

    public override bool IsDone() => done;
    public override void Reset() => done = false;
}

