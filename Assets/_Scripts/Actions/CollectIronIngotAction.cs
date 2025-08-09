using UnityEngine;

public class CollectIronIngotAction : GoapAction
{
    private bool collected = false;
    private float startTime = 0;
    public float workDuration = 1.5f;

    public CollectIronIngotAction()
    {
        AddPrecondition("atMine", true);
        AddEffect("hasRawResource", true);
    }

    public override void Reset() { collected = false; startTime = 0; }
    public override bool IsDone() => collected;
    public override bool RequiresInRange() => true;
    public override bool CheckProceduralPrecondition(GameObject agent) => true;
    public override bool Perform(GameObject agent)
    {
        if (startTime == 0) startTime = Time.time;
        if (Time.time - startTime > workDuration)
        {
            Debug.Log("Collected Iron Ingot!");
            collected = true;
        }
        return true;
    }
}