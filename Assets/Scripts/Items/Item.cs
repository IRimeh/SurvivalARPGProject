using System;

[Serializable]
public class Item
{
    public ItemData ItemData;
    public int Id;
    public int Amount;

    public Item()
    {
        
    }

    public Item(Item item)
    {
        ItemData = item.ItemData;
        Id = item.Id;
        Amount = item.Amount;
    }
}