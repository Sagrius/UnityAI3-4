using UnityEngine;
using System.Collections.Generic;

public abstract class GoapAction : MonoBehaviour
{
    public HashSet<KeyValuePair<string, object>> Preconditions { get; private set; }
    public HashSet<KeyValuePair<string, object>> Effects { get; private set; }
    public float Cost = 1.0f;
    public GameObject Target;

    private bool isDone = false;

    public GoapAction()
    {
        Preconditions = new HashSet<KeyValuePair<string, object>>();
        Effects = new HashSet<KeyValuePair<string, object>>();
    }

    public void DoReset()
    {
        isDone = false;
        Reset();
    }

    public virtual bool IsDone() => isDone;
    public void SetDone(bool done) => isDone = done;

    // Helper methods for cleaner action definitions
    public void AddPrecondition(string key, object value) => Preconditions.Add(new KeyValuePair<string, object>(key, value));
    public void AddEffect(string key, object value) => Effects.Add(new KeyValuePair<string, object>(key, value));

    // Abstract methods for concrete actions to implement
    public abstract void Reset();
    public abstract bool CheckProceduralPrecondition(GameObject agent);
    public abstract bool Perform(GameObject agent);
    public abstract bool RequiresInRange();
}