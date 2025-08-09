using UnityEngine;
using System.Collections.Generic;

public class GoapGoal : ScriptableObject
{
    // ### EDITED: The method is now virtual, allowing subclasses to override it. ###
    public virtual HashSet<KeyValuePair<string, object>> GetGoalState()
    {
        return null; // Base class has no goal.
    }
}