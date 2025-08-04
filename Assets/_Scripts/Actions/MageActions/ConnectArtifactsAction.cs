using UnityEngine;
using System.Collections.Generic;

public class ConnectArtifactsAction : GoapAction
{
    public override HashSet<KeyValuePair<string, object>> Preconditions => new()
    {
        new("EnchantedStaffBuilt", true),
        new("RunedShieldBuilt", true),
        new("AtBuildSite", true)
    };

    public override HashSet<KeyValuePair<string, object>> Effects => new()
    {
        new("CombinedMagicalArtifactBuilt", true)
    };

    public override bool IsValid(GameObject agent)
    {
        return true; // Planner checks preconditions
    }

    public override bool Perform(GameObject agent)
    {
        Debug.Log("Mage connected Enchanted Staff and Runed Shield!");
        ResourceManager.Instance.SetFact("CombinedMagicalArtifactBuilt", true);
        done = true;
        return true;
    }

    public override bool IsDone() => done;
    public override void Reset() => done = false;
}


