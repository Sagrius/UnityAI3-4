using System.Collections.Generic;
using UnityEngine;

public class IdleAction : GoapAction
{
    public override HashSet<KeyValuePair<string, object>> Preconditions => new();
    public override HashSet<KeyValuePair<string, object>> Effects => new()
    {
        new("IsIdle", true)
    };

    public override bool IsValid(GameObject agent) => true;

    public override bool Perform(GameObject agent)
    {
        // Just wait here
        return true;
    }

    public override bool IsDone() => true;
    public override void Reset() { }
}

