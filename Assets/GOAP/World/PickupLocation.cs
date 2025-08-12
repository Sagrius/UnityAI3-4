using UnityEngine;

public class PickupLocation : MonoBehaviour
{
    public enum ResourceType { Logs, Iron, Crystals }
    public ResourceType Type;
    public int Amount;
}