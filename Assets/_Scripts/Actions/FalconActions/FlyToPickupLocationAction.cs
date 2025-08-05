using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class FlyToPickupLocationAction : GoapAction
{
    private NavMeshAgent _navMesh;

    private void Awake()
    {
        _navMesh = transform.root.GetComponent<NavMeshAgent>();
    }
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
            var falcon = agent.GetComponent<FalconGoapAgent>();
            if (falcon != null && falcon.pickupTarget != null)
            {
                _navMesh.SetDestination(falcon.pickupTarget.position);
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

