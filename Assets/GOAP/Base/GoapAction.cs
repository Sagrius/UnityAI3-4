using UnityEngine;
using System.Collections.Generic;

public abstract class GoapAction : ScriptableObject
{
    public string ActionName { get; protected set; } = "GoapAction";
    public float Cost { get; protected set; } = 1.0f;

    public HashSet<KeyValuePair<string, object>> Preconditions = new HashSet<KeyValuePair<string, object>>();
    public HashSet<KeyValuePair<string, object>> Effects = new HashSet<KeyValuePair<string, object>>();

    [System.NonSerialized] public GameObject Target;
    [System.NonSerialized] private bool isDone = false;

    protected void AddPrecondition(string key, object value) => Preconditions.Add(new KeyValuePair<string, object>(key, value));
    protected void AddEffect(string key, object value) => Effects.Add(new KeyValuePair<string, object>(key, value));

    public void DoReset()
    {
        isDone = false;
        Target = null;
        OnReset();
    }

    public bool IsDone() => isDone;
    public void SetDone(bool done) => isDone = done;

    public abstract void OnReset();

    
    public abstract bool CheckProceduralPrecondition(GoapAgent agent);

   
    public abstract bool SetupAction(GoapAgent agent);

    public abstract bool Perform(GoapAgent agent);
    public abstract bool RequiresInRange();
}