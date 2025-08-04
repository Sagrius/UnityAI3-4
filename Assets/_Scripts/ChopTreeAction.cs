using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class ChopTreeAction : GoapAction
{
    private NavMeshAgent _agent;
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }
    public override bool IsValid(GameObject agent)
    {
        target = FindClosestWithTag("Tree");
        bool isValid = target != null;
        if(isValid) _agent.SetDestination(target.transform.position);
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
        if (Vector3.Distance(agent.transform.position, target.transform.position) > 2f) return false;


        ResourceManager.Instance.Add("OakLog", 1);
        done = true;
        return true;
    }

    public override bool IsDone() => done;

    public override void Reset() => done = false;

    public override HashSet<KeyValuePair<string, object>> Preconditions => new() { };
    public override HashSet<KeyValuePair<string, object>> Effects => new()
    {
        new("HasOakLog", true)
    };
}

