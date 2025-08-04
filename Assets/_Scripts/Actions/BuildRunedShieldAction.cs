using UnityEngine;
using System.Collections.Generic;

public class BuildRunedShieldAction : GoapAction
{
    public override HashSet<KeyValuePair<string, object>> Preconditions => new()
    {
        new("AtBuildSite", true),
        new("CrystalShard", 4),
        new("IronIngot", 2)
    };

    public override HashSet<KeyValuePair<string, object>> Effects => new()
    {
        new("RunedShieldBuilt", true)
    };

    public override bool IsValid(GameObject agent)
    {
        return ResourceManager.Instance.Has("CrystalShard", 4) &&
               ResourceManager.Instance.Has("IronIngot", 2);
    }

    public override bool Perform(GameObject agent)
    {
        if (!done)
        {
            Debug.Log("Mage is building the Runed Shield...");
            ResourceManager.Instance.Remove("CrystalShard", 4);
            ResourceManager.Instance.Remove("IronIngot", 2);
            done = true;
        }

        return true;
    }

    public override bool IsDone() => done;
    public override void Reset() => done = false;
}
