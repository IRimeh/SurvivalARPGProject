using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/Items/ItemData", order = 0)]
public class ItemData : ScriptableObject
{
    [ReadOnly] 
    public int Id;
    public string Name;
    public Mesh ItemMesh;
    public Material ItemMaterial;
    public Sprite Sprite;
    public int MaxStackSize;
}
