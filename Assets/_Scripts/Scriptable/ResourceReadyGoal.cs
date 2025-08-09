using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ResourceReadyGoal", menuName = "Goap/Goals/Resource Ready Goal")]
public class ResourceReadyGoal : GoapGoal
{
    public override HashSet<KeyValuePair<string, object>> GetGoalState()
    {
        return new HashSet<KeyValuePair<string, object>>
        {
            new KeyValuePair<string, object>("resourceReadyForPickup", true)
        };
    }
}