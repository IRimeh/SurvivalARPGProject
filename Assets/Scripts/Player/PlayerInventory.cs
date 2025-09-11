using System;
using System.Collections;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private ItemManager _itemManager;
    [SerializeField] private Vector3 _pickupOffset;
    [SerializeField] private float _pickupTimePerMeter = 0.1f;
    [SerializeField] private bool _enabled = true;

    [Space]
    [SerializeField] private int _rows = 5;
    [SerializeField] private int _columns = 9;
    
    private InventorySlot[] _inventory;
    public InventorySlot[] Inventory => _inventory;
        
    private void Awake()
    {
        _inventory = new InventorySlot[_rows * _columns];
        InitEmptyInventory();
    }

    private void InitEmptyInventory()
    {
        for (int i = 0; i < _inventory.Length; i++)
        {
            _inventory[i] = new InventorySlot()
            {
                HasItem = false,
                Item = null
            };
        }
    }

    private bool TryGetFirstAvailableSlot(Item item, out int inventorySlotIdx)
    {
        inventorySlotIdx = -1;
        int emptySlotIdx = -1;
        for (int i = 0; i < _inventory.Length; i++)
        {
            InventorySlot inventorySlot = _inventory[i];
            if (!inventorySlot.HasItem && emptySlotIdx == -1)
            {
                emptySlotIdx = i;
            }
            else if (inventorySlot.HasItem && 
                     inventorySlot.Item.Id == item.Id && 
                     inventorySlot.Item.Amount < inventorySlot.Item.ItemData.MaxStackSize)
            {
                inventorySlotIdx = i;
                return true;
            }
        }

        if (emptySlotIdx >= 0)
        {
            inventorySlotIdx = emptySlotIdx;
            return true;
        }

        return false;
    }

    public bool TryAddItem(Item item)
    {
        while (item.Amount > 0)
        {
            if (TryGetFirstAvailableSlot(item, out int inventorySlotIdx))
            {
                AddItem(_inventory[inventorySlotIdx], item);
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    private void AddItem(InventorySlot inventorySlot, Item item)
    {
        if (inventorySlot.HasItem)
        {
            inventorySlot.Item.Amount += item.Amount;
            int leftOver = inventorySlot.Item.Amount - inventorySlot.Item.ItemData.MaxStackSize;
            if (leftOver > 0)
            {
                item.Amount = leftOver;
                inventorySlot.Item.Amount -= leftOver;
            }
            else
            {
                item.Amount = 0;
            }
        }
        else
        {
            inventorySlot.HasItem = true;
            inventorySlot.Item = new Item(item);
            item.Amount = 0;
        }
        inventorySlot.InventorySlotUpdated();
    }
    
    
    
    
    
    
    
    

    private void OnTriggerStay(Collider other)
    {
        if (!_enabled)
            return;
        
        if (other.TryGetComponent(out ItemView itemView))
        {
            PickupItem(itemView);
        }
    }

    private void PickupItem(ItemView itemView)
    {
        if (!itemView.PickupAble)
            return;

        itemView.SetPickupAble(false);
        StartCoroutine(IPickupItem(itemView));
    }

    private IEnumerator IPickupItem(ItemView itemView)
    {
        if (!TryAddItem(itemView.Item))
            yield break;
        
        Vector3 pickupPos = transform.position + _pickupOffset;
        float dist = Vector3.Distance(pickupPos, itemView.transform.position);
        float pickupTime = dist * _pickupTimePerMeter;
        Vector3 startPos = itemView.transform.position;
        itemView.RigidBody.isKinematic = true;

        for (float i = 0; i < pickupTime; i += Time.deltaTime)
        {
            float delta = i / pickupTime;
            Vector3 pos = transform.position + _pickupOffset;
            itemView.transform.position = Vector3.Lerp(startPos, pos, delta);
            yield return null;
        }
        
        _itemManager.ReturnToPool(itemView);
    }
}

[Serializable]
public class InventorySlot
{
    public bool HasItem;
    public Item Item;
    public event Action<InventorySlot> OnInventorySlotUpdated = delegate { };
    public void InventorySlotUpdated()
    {
        OnInventorySlotUpdated.Invoke(this);
    }
}
