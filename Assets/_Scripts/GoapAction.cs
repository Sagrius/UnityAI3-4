using UnityEngine;
using System.Collections.Generic;

public abstract class GoapAction : MonoBehaviour
{
    public float Cost = 1f;
    protected GameObject target;
    protected bool done;

    public abstract HashSet<KeyValuePair<string, object>> Preconditions { get; }
    public abstract HashSet<KeyValuePair<string, object>> Effects { get; }

    public abstract bool IsValid(GameObject agent);
    public abstract bool Perform(GameObject agent);
    public abstract void Reset();
    public abstract bool IsDone();
}



