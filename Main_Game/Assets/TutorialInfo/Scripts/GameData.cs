using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    private static GameData _instance;
    public static GameData Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("GameData");
                _instance = go.AddComponent<GameData>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    public int BankedMoney        { get; private set; } = 0;
    public int RoundMoney         { get; private set; } = 0;
    public int RoundMoneyEarned   { get; private set; } = 0;
    public bool PlayerEscapedLastRound { get; private set; } = false;
    public List<ItemType> Inventory { get; private set; } = new List<ItemType>();

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddRoundMoney(int amount)
    {
        RoundMoney += amount;
    }

    public void OnPlayerEscaped()
    {
        PlayerEscapedLastRound = true;
        RoundMoneyEarned = RoundMoney;
        BankedMoney += RoundMoney;
        RoundMoney = 0;
    }

    public void OnPlayerKilled()
    {
        PlayerEscapedLastRound = false;
        RoundMoneyEarned = 0;
        RoundMoney = 0;
        Inventory.Clear();
    }

    // Returns true if the purchase succeeded.
    public bool TryBuyItem(ItemType type)
    {
        ItemData data = ItemCatalog.Items[type];
        int owned = Inventory.FindAll(i => i == type).Count;

        if (BankedMoney < data.price)   return false;
        if (Inventory.Count >= 5)       return false;
        if (owned >= data.maxOwnable)   return false;

        BankedMoney -= data.price;
        Inventory.Add(type);
        return true;
    }

    // Returns true if the sell succeeded.
    public bool TrySellItem(ItemType type)
    {
        if (!Inventory.Contains(type)) return false;
        Inventory.Remove(type);
        BankedMoney += Mathf.FloorToInt(ItemCatalog.Items[type].price / 2f);
        return true;
    }

    public void RemoveItemAt(int index)
    {
        if (index >= 0 && index < Inventory.Count)
            Inventory.RemoveAt(index);
    }

    public void FullReset()
    {
        BankedMoney = 0;
        RoundMoney = 0;
        RoundMoneyEarned = 0;
        PlayerEscapedLastRound = false;
        Inventory.Clear();
    }
}
