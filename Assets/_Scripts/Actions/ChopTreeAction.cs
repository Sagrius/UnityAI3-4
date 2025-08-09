using UnityEngine;

public class ChopTreeAction : GoapAction
{
    private bool chopped = false;
    private float startTime = 0;
    public float workDuration = 2f;

    public ChopTreeAction()
    {
        AddPrecondition("atTree", true);
        AddEffect("hasRawResource", true);
    }

    public override void Reset() { chopped = false; startTime = 0; }
    public override bool IsDone() => chopped;
    public override bool RequiresInRange() => true;
    public override bool CheckProceduralPrecondition(GameObject agent) => true;
    public override bool Perform(GameObject agent)
    {
        if (startTime == 0) startTime = Time.time;
        if (Time.time - startTime > workDuration)
        {
            Debug.Log("Finished chopping tree!");
            chopped = true;
        }
        return true;
    }
}