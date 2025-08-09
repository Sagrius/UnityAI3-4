using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ResourceDeliveredGoal", menuName = "Goap/Goals/Resource Delivered Goal")]
public class ResourceDeliveredGoal : GoapGoal
{
    public override HashSet<KeyValuePair<string, object>> GetGoalState()
    {
        return new HashSet<KeyValuePair<string, object>>
        {
            new KeyValuePair<string, object>("resourceDelivered", true)
        };
    }
}