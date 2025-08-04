using UnityEngine;
using System.Collections.Generic;

public class ConnectArtifactsAction : GoapAction
{
    public override HashSet<KeyValuePair<string, object>> Preconditions => new()
    {
        new("AtBuildSite", true),
        new("EnchantedStaffBuilt", true),
        new("RunedShieldBuilt", true)
    };

    public override HashSet<KeyValuePair<string, object>> Effects => new()
    {
        new("CombinedMagicalArtifactBuilt", true)
    };

    public override bool IsValid(GameObject agent)
    {
        return true; // Assumes conditions are validated by world state
    }

    public override bool Perform(GameObject agent)
    {
        if (!done)
        {
            Debug.Log("Mage is combining the artifacts into the Combined Magical Artifact!");
            done = true;
        }

        return true;
    }

    public override bool IsDone() => done;
    public override void Reset() => done = false;
}

