using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class SearchForResourceAction : GoapAction
{
    public float searchRadius = 15f;
    private NavMeshAgent _navMesh;

    private void Awake()
    {
        _navMesh = transform.root.GetComponent<NavMeshAgent>();
    }
    public override HashSet<KeyValuePair<string, object>> Preconditions => new()
    {
        new("IsIdle", true)
    };

    public override HashSet<KeyValuePair<string, object>> Effects => new()
    {
        new("HasSearchTarget", true)
    };

    public override bool IsValid(GameObject agent) => true;

    public override bool Perform(GameObject agent)
    {
        if (!done)
        {
            Vector3 randomDirection = Random.insideUnitSphere * searchRadius;
            randomDirection.y = 0;

            Vector3 targetPos = agent.transform.position + randomDirection;

            if (IsWalkable(targetPos))
            {
                _navMesh.SetDestination(targetPos);
                done = true;
            }
        }

        return true;
    }

    private bool IsWalkable(Vector3 pos)
    {
        // You can raycast or check with NavMesh.SamplePosition
        return true;
    }

    public override bool IsDone() => done;
    public override void Reset() => done = false;
}

