using UnityEngine;
using System.Collections.Generic;

public class ReturnToIdleAction : GoapAction
{
    public override HashSet<KeyValuePair<string, object>> Preconditions => new()
    {
        new("TaskComplete", true)
    };

    public override HashSet<KeyValuePair<string, object>> Effects => new()
    {
        new("FalconAvailable", true),
        new("CarryingResource", false)
    };

    public override bool IsValid(GameObject agent) => true;

    public override bool Perform(GameObject agent)
    {
        var falcon = agent.GetComponent<FalconGoapAgent>();
        if (falcon != null)
        {
            falcon.isCarryingResource = false;
            falcon.pickupTarget = null;
            falcon.buildTarget = null;
            Debug.Log("Falcon returned to idle.");
            done = true;
        }

        return true;
    }

    public override bool IsDone() => done;
    public override void Reset() => done = false;
}

