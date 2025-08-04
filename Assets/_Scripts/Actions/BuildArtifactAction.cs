using UnityEngine;
using System.Collections.Generic;

public class BuildArtifactAction : GoapAction
{
    public override bool IsValid(GameObject agent)
    {
        return ResourceManager.Instance.Has("OakLog", 5) &&
               ResourceManager.Instance.Has("IronIngot", 3);
    }

    public override bool Perform(GameObject agent)
    {
        ResourceManager.Instance.Remove("OakLog", 5);
        ResourceManager.Instance.Remove("IronIngot", 3);
        ResourceManager.Instance.Add("EnchantedStaff", 1);
        done = true;
        return true;
    }

    public override void Reset()
    {
        throw new System.NotImplementedException();
    }

    public override bool IsDone()
    {
        throw new System.NotImplementedException();
    }

    public override HashSet<KeyValuePair<string, object>> Effects => new()
    {
        new("EnchantedStaffBuilt", true)
    };

    public override HashSet<KeyValuePair<string, object>> Preconditions => throw new System.NotImplementedException();
}

