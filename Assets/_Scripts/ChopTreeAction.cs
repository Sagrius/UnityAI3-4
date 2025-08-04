using UnityEngine;
using System.Collections.Generic;
using System;

public class ChopTreeAction : GoapAction
{
    public override bool CheckPrecondition(GameObject agent)
    {
        // Find tree
        target = FindClosestWithTag("Tree");
        return target != null;
    }

    private GameObject FindClosestWithTag(string v)
    {
        throw new NotImplementedException();
    }

    public override bool Perform(GameObject agent)
    {
        // Simulate chopping
        ResourceManager.Instance.Add("OakLog", 1);
        done = true;
        return true;
    }

    public override bool IsDone() => done;

    public override void Reset() => done = false;

    public override HashSet<KeyValuePair<string, bool>> Preconditions => new() { };
    public override HashSet<KeyValuePair<string, bool>> Effects => new()
    {
        new("HasOakLog", true)
    };
}

