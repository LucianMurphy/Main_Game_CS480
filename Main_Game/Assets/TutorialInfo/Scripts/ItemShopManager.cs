using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ItemShopManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text bankedMoneyText;
    public TMP_Text outcomeMessageText;

    [Header("Item Cards - assign in Inspector order: EnergyDrink, Shield, Map, Burger, FlashBang")]
    public ItemCardUI[] itemCards;

    [Header("Inventory Slots - 5 slots in order")]
    public InventorySlotUI[] inventorySlots;

    void Start()
    {
        ShowOutcomeMessage();
        RefreshUI();
    }

    private void ShowOutcomeMessage()
    {
        if (GameData.Instance.PlayerEscapedLastRound)
        {
            outcomeMessageText.text =
                $"You escaped!  <color=green>+${GameData.Instance.RoundMoneyEarned}</color> added to your bank.";
        }
        else
        {
            outcomeMessageText.text =
                "You were caught.  Your round money and items have been lost.";
        }
    }

    public void RefreshUI()
    {
        bankedMoneyText.text = $"Bank:  ${GameData.Instance.BankedMoney}";

        ItemType[] allTypes = (ItemType[])System.Enum.GetValues(typeof(ItemType));
        for (int i = 0; i < itemCards.Length && i < allTypes.Length; i++)
            itemCards[i].Setup(allTypes[i], this);

        RefreshInventorySlots();
    }

    public void RefreshInventorySlots()
    {
        List<ItemType> inv = GameData.Instance.Inventory;
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (i < inv.Count)
                inventorySlots[i].Show(inv[i]);
            else
                inventorySlots[i].Clear();
        }

        foreach (var card in itemCards)
            card.Refresh();

        bankedMoneyText.text = $"Bank:  ${GameData.Instance.BankedMoney}";
    }

    public void OnBuyClicked(ItemType type)
    {
        if (GameData.Instance.TryBuyItem(type))
            RefreshInventorySlots();
    }

    public void OnSellClicked(ItemType type)
    {
        if (GameData.Instance.TrySellItem(type))
            RefreshInventorySlots();
    }

    public void OnReadyClicked()
    {
        SceneManager.LoadScene("PacMan");
    }

    public void OnMainMenuClicked()
    {
        GameData.Instance.FullReset();
        SceneManager.LoadScene("MainMenu");
    }
}
