using UnityEngine;

[CreateAssetMenu(fileName = "ItemSpriteDatabase", menuName = "Game/Item Sprite Database")]
public class ItemSpriteDatabase : ScriptableObject
{
    public Sprite energyDrink;
    public Sprite shield;
    public Sprite map;
    public Sprite burger;
    public Sprite flashBang;

    public Sprite Get(ItemType type)
    {
        switch (type)
        {
            case ItemType.EnergyDrink: return energyDrink;
            case ItemType.Shield:      return shield;
            case ItemType.Map:         return map;
            case ItemType.Burger:      return burger;
            case ItemType.FlashBang:   return flashBang;
            default:                   return null;
        }
    }
}
