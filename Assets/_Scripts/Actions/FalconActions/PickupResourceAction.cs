using UnityEngine;
using System.Collections.Generic;

public class PickupResourceAction : GoapAction
{
    public override HashSet<KeyValuePair<string, object>> Preconditions => new()
    {
        new("AtPickupLocation", true),
        new("ResourceReady", true)
    };

    public override HashSet<KeyValuePair<string, object>> Effects => new()
    {
        new("CarryingResource", true)
    };

    public override bool IsValid(GameObject agent) => true;

    public override bool Perform(GameObject agent)
    {
        var falcon = agent.GetComponent<FalconAgent>();
        if (falcon != null)
        {
            Debug.Log("Falcon picked up the resource.");
            falcon.isCarryingResource = true;
            done = true;
        }

        return true;
    }

    public override bool IsDone() => done;
    public override void Reset() => done = false;
}

