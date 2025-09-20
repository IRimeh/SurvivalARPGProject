using System;

namespace Player
{
    [Serializable]
    public class InventorySlot
    {
        public int InventorySlotID { get; private set; }
        public bool HasItem { get; private set; }
        public Item Item { get; private set; }
        public event Action<InventorySlot> OnInventorySlotUpdated = delegate { };

        public InventorySlot(int inventorySlotID)
        {
            InventorySlotID = inventorySlotID;
        }
    
        public void InventorySlotUpdated()
        {
            OnInventorySlotUpdated.Invoke(this);
        }

        public void SetItem(Item item)
        {
            Item = item;
            HasItem = item != null && item.Amount > 0;
        }

        public bool TryTakeItemFromSlot(out Item item)
        {
            item = HasItem ? new Item(Item) : null;
            bool success = HasItem;
            HasItem = false;
            InventorySlotUpdated();
            return success;
        }

        public bool TryTakeHalfOfItemsFromSlot(out Item half)
        {
            if (!HasItem || Item.Amount == 1)
                return TryTakeItemFromSlot(out half);

            half = new Item(Item);
            half.Amount = Item.Amount / 2;
            Item.Amount -= half.Amount;
            InventorySlotUpdated();
            return true;
        }

        public void AddToInventorySlot(Item item, out Item leftOver)
        {
            if (!HasItem)
            {
                leftOver = new Item(item);
                Item = item;
                HasItem = true;
                leftOver.Amount = 0;
                InventorySlotUpdated();
                return;
            }

            if (Item.Id != item.Id)
            {
                leftOver = new Item(Item);
                Item = new Item(item);
                InventorySlotUpdated();
                return;
            }

            leftOver = new Item(item);
            Item.Amount += item.Amount;
            int leftOverAmount = Math.Max(0, Item.Amount - Item.ItemData.MaxStackSize);
            Item.Amount = Math.Min(Item.Amount, Item.ItemData.MaxStackSize);
            leftOver.Amount = leftOverAmount;
            InventorySlotUpdated();
        }
    }
}