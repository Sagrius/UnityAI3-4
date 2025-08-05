using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance;
    void Awake() => Instance = this;

    private Queue<(Transform pickup, Transform build)> falconJobs = new();

    public void AddFalconJob(Transform pickup, Transform build)
    {
        falconJobs.Enqueue((pickup, build));
    }

    public bool TryGetFalconJob(out Transform pickup, out Transform build)
    {
        if (falconJobs.Count > 0)
        {
            var job = falconJobs.Dequeue();
            pickup = job.pickup;
            build = job.build;
            return true;
        }

        pickup = null;
        build = null;
        return false;
    }
}
