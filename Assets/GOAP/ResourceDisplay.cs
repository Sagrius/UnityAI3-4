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
        // Get current resource counts from the WorldState
        int logs = (int)WorldState.Instance.GetState("oakLogsInStockpile");
        int iron = (int)WorldState.Instance.GetState("ironIngotsInStockpile");
        int crystals = (int)WorldState.Instance.GetState("crystalShardsInStockpile");

        // Create the display string
        string displayText = $"Logs: {logs} / 5\n" +
                             $"Iron: {iron} / 5\n" +
                             $"Crystals: {crystals} / 4";

        // Draw a background box and the text
        GUI.Box(new Rect(10, 10, 150, 80), "Stockpile");
        GUI.Label(new Rect(20, 35, 140, 70), displayText, style);
    }
}