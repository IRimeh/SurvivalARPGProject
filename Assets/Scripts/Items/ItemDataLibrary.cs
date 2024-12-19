using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDataLibrary", menuName = "ScriptableObjects/Items/ItemDataLibrary", order = 0)]
public class ItemDataLibrary : ScriptableObject
{
    [SerializeField] 
    private List<ItemData> _itemDatas = new List<ItemData>();

    public ItemData GetItemFromID(int ID)
    {
        return _itemDatas[ID];
    }

    [Button]
    private void SetIDs()
    {
        for (int i = 0; i < _itemDatas.Count; i++)
        {
            _itemDatas[i].Id = i;
        }
    }
    
    private void OnValidate()
    {
        SetIDs();
    }
}