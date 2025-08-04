using UnityEngine;
using System.Collections.Generic;

public abstract class GoapAction : MonoBehaviour
{
    public float cost = 1f;
    protected bool done = false;
    protected GameObject target;

    public abstract bool CheckPrecondition(GameObject agent);
    public abstract bool Perform(GameObject agent);
    public abstract void Reset();
    public abstract bool IsDone();

    public abstract HashSet<KeyValuePair<string, bool>> Preconditions { get; }
    public abstract HashSet<KeyValuePair<string, bool>> Effects { get; }
}

