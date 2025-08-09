using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CombineArtifactsGoal", menuName = "Goap/Goals/Combine Artifacts Goal")]
public class CombineArtifactsGoal : GoapGoal
{
    public override HashSet<KeyValuePair<string, object>> GetGoalState()
    {
        // This is the ultimate goal of the entire system.
        return new HashSet<KeyValuePair<string, object>>
        {
            new KeyValuePair<string, object>("combinedArtifactBuilt", true)
        };
    }
}