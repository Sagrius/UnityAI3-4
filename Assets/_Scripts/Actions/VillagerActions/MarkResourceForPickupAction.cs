using UnityEngine;
using System.Collections.Generic;

public class MarkResourceForPickupAction : GoapAction
{
    public override HashSet<KeyValuePair<string, object>> Preconditions => new()
    {
        new("ResourceReady", true)
    };

    public override HashSet<KeyValuePair<string, object>> Effects => new()
    {
        new("ResourceMarkedForPickup", true)
    };

    public override bool IsValid(GameObject agent) => true;

    public override bool Perform(GameObject agent)
    {
        Debug.Log("Marked resource for pickup.");
        done = true;
        return true;
    }

    public override bool IsDone() => done;
    public override void Reset() => done = false;
}

