using UnityEngine;

public class ResourceSource : MonoBehaviour
{
    public enum ResourceType { Tree, Mine, CrystalCavern }
    public ResourceType Type;
    public int quantity = 1;
}