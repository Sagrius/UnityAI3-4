using UnityEngine;
using System.Collections.Generic;

public class CarryResourceToBuildAction : GoapAction
{
    public override HashSet<KeyValuePair<string, object>> Preconditions => new()
    {
        new("ResourceReady", true),
        new("FalconAvailable", false)
    };

    public override HashSet<KeyValuePair<string, object>> Effects => new()
    {
        new("ResourceDelivered", true)
    };

    public override bool IsValid(GameObject agent) => true;

    public override bool Perform(GameObject agent)
    {
        if (!done)
        {
            var buildLocation = GameObject.FindGameObjectWithTag("BuildLocation");
            agent.GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(buildLocation.transform.position);
            // You could also simulate a drop there
            ResourceManager.Instance.Add("CarriedResource", 1);
            done = true;
        }

        return true;
    }

    public override bool IsDone() => done;
    public override void Reset() => done = false;
}

