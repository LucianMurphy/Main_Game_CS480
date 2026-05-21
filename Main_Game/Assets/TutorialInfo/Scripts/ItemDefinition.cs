using System.Collections.Generic;
using UnityEngine;

public enum ItemType { EnergyDrink, Shield, Map, Burger, FlashBang }

[System.Serializable]
public class ItemData
{
    public string displayName;
    public string description;
    public int price;
    public int maxOwnable;
    public float duration;
    public Sprite icon;
}

public static class ItemCatalog
{
    public static readonly Dictionary<ItemType, ItemData> Items = new Dictionary<ItemType, ItemData>
    {
        { ItemType.EnergyDrink, new ItemData { displayName = "Energy Drink",  description = "2.5x move speed for 30s",           price = 40, maxOwnable = 5, duration = 30f } },
        { ItemType.Shield,      new ItemData { displayName = "Shield",         description = "Absorbs 1 hit, then 5s invincible",  price = 80, maxOwnable = 1, duration =  5f } },
        { ItemType.Map,         new ItemData { displayName = "Map",            description = "Shows minimap for 15s",              price = 25, maxOwnable = 5, duration = 15f } },
        { ItemType.Burger,      new ItemData { displayName = "Burger",         description = "1.5x width for 20s",                 price = 35, maxOwnable = 5, duration = 20f } },
        { ItemType.FlashBang,   new ItemData { displayName = "Flash Bang",     description = "Stuns enemy for 3s",                 price = 60, maxOwnable = 5, duration =  3f } },
    };

    static ItemCatalog()
    {
        var sprites = Resources.Load<ItemSpriteDatabase>("ItemSpriteDatabase");
        if (sprites == null) return;

        foreach (ItemType type in Items.Keys)
            Items[type].icon = sprites.Get(type);
    }
}
