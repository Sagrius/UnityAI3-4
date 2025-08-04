using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GoapAgent : MonoBehaviour
{
    public List<GoapAction> actions = new();
    public Queue<GoapAction> currentPlan;
    public Dictionary<string, bool> worldState;
    public Dictionary<string, bool> goal;

    void Start()
    {
        actions = GetComponents<GoapAction>().ToList();
    }

    void Update()
    {
        if (currentPlan == null || currentPlan.Count == 0)
        {
            Plan();
        }
        else
        {
            var action = currentPlan.Peek();
            if (action.IsDone())
            {
                currentPlan.Dequeue();
            }
            else
            {
                action.Perform(gameObject);
            }
        }
    }

    void Plan()
    {
        goal = new Dictionary<string, bool> { { "CombinedMagicalArtifactBuilt", true } };
        // Add planner logic or plug in a planner here
    }
}

