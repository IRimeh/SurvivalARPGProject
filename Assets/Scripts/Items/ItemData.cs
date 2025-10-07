using System.Collections.Generic;
using Items.ItemBehaviours;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/Items/ItemData", order = 0)]
public class ItemData : ScriptableObject
{
    public enum ItemHoldingStyle
    {
        TwoHanded,
        OneHanded,
        None
    }
    
    [ReadOnly] 
    public int Id;
    public ItemHoldingStyle HoldingStyle;
    public string Name;
    public Mesh ItemMesh;
    public Material ItemMaterial;
    public Sprite Sprite;
    public int MaxStackSize;
    public List<ItemBehaviour> ItemBehaviours = new();
}
