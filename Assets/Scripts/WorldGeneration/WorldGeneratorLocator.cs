using UnityEngine;

[CreateAssetMenu(fileName = "WorldGeneratorLocator", menuName = "ScriptableObjects/WorldGeneration/WorldGeneratorLocator", order = 0)]
public class WorldGeneratorLocator : ScriptableObject
{
    public WorldGenerator Instance { get; private set; }

    public void Assign(WorldGenerator worldGenerator)
    {
        Instance = worldGenerator;
    }
}
