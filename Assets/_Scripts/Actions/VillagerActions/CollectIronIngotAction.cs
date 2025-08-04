using UnityEngine;
using System.Collections.Generic;

public class CollectIronIngotAction : GoapAction
{
    public override HashSet<KeyValuePair<string, object>> Preconditions => new()
    {
        new("AtMine", true)
    };

    public override HashSet<KeyValuePair<string, object>> Effects => new()
    {
        new("IronIngot", 1)
    };

    public override bool IsValid(GameObject agent) => true;

    public override bool Perform(GameObject agent)
    {
        if (!done)
        {
            Debug.Log("Collecting Iron Ingot...");
            ResourceManager.Instance.Add("IronIngot", 1);
            done = true;
        }

        return true;
    }

    public override bool IsDone() => done;
    public override void Reset() => done = false;
}

