using UnityEngine;

public class ResourceSource : MonoBehaviour
{
    public enum ResourceType { Tree, Mine, CrystalCavern }
    public ResourceType Type;

    // Registration is now handled centrally by the ResourceManager.
}