using UnityEngine;

public class ResourceDisplay : MonoBehaviour
{
    private GUIStyle style;

    void Awake()
    {
        style = new GUIStyle();
        style.fontSize = 16;
        style.normal.textColor = Color.white;
    }

    void OnGUI()
    {
        int logs = (int)WorldState.Instance.GetState(WorldStateKeys.LogsInStockpile);
        int iron = (int)WorldState.Instance.GetState(WorldStateKeys.IronInStockpile);
        int crystals = (int)WorldState.Instance.GetState(WorldStateKeys.CrystalsInStockpile);

        // This could also be driven by the TaskManager's totalResourceDemand dictionary
        string displayText = $"Logs: {logs} / 5\n" +
                             $"Iron: {iron} / 5\n" +
                             $"Crystals: {crystals} / 4";

        GUI.Box(new Rect(10, 10, 150, 80), "Stockpile");
        GUI.Label(new Rect(20, 35, 140, 70), displayText, style);
    }
}