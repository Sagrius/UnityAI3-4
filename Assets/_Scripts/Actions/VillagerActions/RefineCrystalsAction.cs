using UnityEngine;
using System.Collections.Generic;

public class RefineCrystalsAction : GoapAction
{
    public override HashSet<KeyValuePair<string, object>> Preconditions => new()
    {
        new("AtCrystalCave", true)
    };

    public override HashSet<KeyValuePair<string, object>> Effects => new()
    {
        new("CrystalShard", 1)
    };

    public override bool IsValid(GameObject agent) => true;

    public override bool Perform(GameObject agent)
    {
        if (!done)
        {
            Debug.Log("Refining Crystals...");
            ResourceManager.Instance.Add("CrystalShard", 1);
            done = true;
        }

        return true;
    }

    public override bool IsDone() => done;
    public override void Reset() => done = false;
}
