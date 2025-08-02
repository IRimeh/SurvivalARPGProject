using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemManager", menuName = "ScriptableObjects/Items/ItemManager", order = 0)]
public class ItemManager : ScriptableObject
{
    [SerializeField] private ItemView _itemViewPrefab;
    [SerializeField] private int _initialItemPoolAmount = 100;
    [SerializeField] private Vector3 _itemLaunchForce;
    [SerializeField] [Range(1, 2)] private float _maxRandLaunchForce = 2.0f;
    [SerializeField] private WorldGeneratorLocator _worldGeneratorLocator;

    private ItemManagerView _itemManagerView;
    private List<ItemView> _availableItemViewPool = new List<ItemView>();
    private List<ItemView> _usedItemViewPool = new List<ItemView>();

    public void SetItemManagerView(ItemManagerView itemManagerView)
    {
        _availableItemViewPool.Clear();
        _usedItemViewPool.Clear();
        _itemManagerView = itemManagerView;
        PopulatePool();
    }

    private void PopulatePool()
    {
        for (int i = 0; i < _initialItemPoolAmount; i++)
        {
            ItemView poolItem = Instantiate(_itemViewPrefab, Vector3.zero, Quaternion.identity, _itemManagerView.transform);
            poolItem.gameObject.SetActive(false);
            _availableItemViewPool.Add(poolItem);
        }
    }
    
    public ItemView SpawnItem(ItemData itemData, int amount, Vector3 position, float launchForce = 1, Chunk chunk = null)
    {
        ItemView newItem = null;
        if (_availableItemViewPool.Count <= 0)
        {
            newItem = _usedItemViewPool[0];
            ReturnToPool(newItem);
        }
        else
        {
            newItem = _availableItemViewPool[^1];
        }

        Quaternion rot = Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0);
        _availableItemViewPool.Remove(newItem);
        _usedItemViewPool.Add(newItem);
        newItem.gameObject.SetActive(true);
        newItem.transform.position = position;
        newItem.transform.rotation = rot;
        newItem.SetItem(new Item()
        {
            ItemData = itemData,
            Amount = amount,
            Id = itemData.Id
        });

        Vector3 force = rot * _itemLaunchForce;
        float randForce = Random.Range(1.0f, _maxRandLaunchForce);
        newItem.RigidBody.isKinematic = false;
        newItem.RigidBody.AddForce(force * (launchForce * randForce));

        if (chunk == null)
            _worldGeneratorLocator.Instance.TryGetChunkAtWorldPosition(position, out chunk);

        if (chunk != null)
        {
            chunk.AddItem(newItem);
            newItem.SetChunk(chunk);
        }

        return newItem;
    }

    public void ReturnToPool(ItemView itemView)
    {
        if (!_usedItemViewPool.Contains(itemView))
            return;

        itemView.SetPickupAble(false);
        itemView.gameObject.SetActive(false);
        _usedItemViewPool.Remove(itemView);
        _availableItemViewPool.Add(itemView);
        itemView.RigidBody.isKinematic = true;
        itemView.RigidBody.linearVelocity = Vector3.zero;

        itemView.Chunk.RemoveItem(itemView);
    }

    [Button]
    private void ReturnToPool()
    {
        ReturnToPool(_usedItemViewPool[0]);
    }
}
