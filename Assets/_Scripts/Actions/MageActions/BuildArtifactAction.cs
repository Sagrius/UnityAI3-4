using UnityEngine;
using System.Collections.Generic;

public class BuildArtifactAction : GoapAction
{
    public override HashSet<KeyValuePair<string, object>> Preconditions => new()
    {
        new("AtBuildSite", true)
    };

    public override HashSet<KeyValuePair<string, object>> Effects => new()
    {
        new("ArtifactBuilt", true) // abstract — real type is determined below
    };

    public override bool IsValid(GameObject agent)
    {
        return ResourceManager.Instance.Has("OakLog", 5) &&
               ResourceManager.Instance.Has("IronIngot", 3) ||
               ResourceManager.Instance.Has("CrystalShard", 4) &&
               ResourceManager.Instance.Has("IronIngot", 2);
    }

    public override bool Perform(GameObject agent)
    {
        if (!done)
        {
            if (ResourceManager.Instance.Has("OakLog", 5) &&
                ResourceManager.Instance.Has("IronIngot", 3))
            {
                ResourceManager.Instance.Remove("OakLog", 5);
                ResourceManager.Instance.Remove("IronIngot", 3);
                ResourceManager.Instance.SetFact("EnchantedStaffBuilt", true);
                Debug.Log("Mage built Enchanted Staff.");
            }
            else if (ResourceManager.Instance.Has("CrystalShard", 4) &&
                     ResourceManager.Instance.Has("IronIngot", 2))
            {
                ResourceManager.Instance.Remove("CrystalShard", 4);
                ResourceManager.Instance.Remove("IronIngot", 2);
                ResourceManager.Instance.SetFact("RunedShieldBuilt", true);
                Debug.Log("Mage built Runed Shield.");
            }

            done = true;
        }

        return true;
    }

    public override bool IsDone() => done;
    public override void Reset() => done = false;
}


