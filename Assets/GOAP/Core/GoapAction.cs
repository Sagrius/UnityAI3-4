using UnityEngine;
using System.Collections.Generic;

public abstract class GoapAction
{
    public string ActionName { get; protected set; } = "GoapAction";
    public float Cost { get; protected set; } = 1.0f;
    public HashSet<KeyValuePair<string, object>> Preconditions { get; protected set; }
    public HashSet<KeyValuePair<string, object>> Effects { get; protected set; }
    public GameObject Target { get; set; }
    private bool isDone = false;

    public GoapAction()
    {
        Preconditions = new HashSet<KeyValuePair<string, object>>();
        Effects = new HashSet<KeyValuePair<string, object>>();
    }

    public void DoReset()
    {
        isDone = false;
        Target = null;
        OnReset();
    }

    public bool IsDone() => isDone;
    public void SetDone(bool done) => isDone = done;
    protected void AddPrecondition(string key, object value) => Preconditions.Add(new KeyValuePair<string, object>(key, value));
    protected void AddEffect(string key, object value) => Effects.Add(new KeyValuePair<string, object>(key, value));

    public abstract void OnReset();
    public abstract bool CheckProceduralPrecondition(GoapAgent agent);
    public abstract bool Perform(GoapAgent agent);
    public abstract bool RequiresInRange();
}