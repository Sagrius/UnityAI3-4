using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class ChopTreeAction : GoapAction
{
    private NavMeshAgent _agent;
    private void Awake()
    {
        _agent = transform.root.GetComponent<NavMeshAgent>();
    }
    public override bool IsValid(GameObject agent)
    {
        target = FindClosestWithTag("Tree");
        bool isValid = target != null;
        return isValid;
    }

    protected GameObject FindClosestWithTag(string tag)
    {
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tag);
        GameObject closest = null;
        float closestDistance = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (GameObject obj in taggedObjects)
        {
            float distance = Vector3.Distance(currentPosition, obj.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = obj;
            }
        }

       
        return closest;
    }

    public override bool Perform(GameObject agent)
    {
        if (target == null)
        {
            target = FindClosestWithTag("Tree");
            if (target == null) return false;
            _agent.SetDestination(target.transform.position);
        }

        if (Vector3.Distance(agent.transform.position, target.transform.position) > 2f)
        {
            // Still moving
            return true;
        }

        // At destination
        Debug.Log("Collected tree");
        ResourceManager.Instance.Add("OakLog", 1);
        done = true;
        return true;
    }

    public override bool IsDone() => done;

    public override void Reset() => done = false;

    public override HashSet<KeyValuePair<string, object>> Preconditions => new()
    {
        new("AtForest", true) // or "AtTree" or whatever name matches your worldState key
    };
    public override HashSet<KeyValuePair<string, object>> Effects => new()
    {
        new("HasOakLog", true)
    };
}

