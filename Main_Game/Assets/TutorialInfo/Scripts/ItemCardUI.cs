using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to each of the 5 item card panels in the ItemShop scene.
public class ItemCardUI : MonoBehaviour
{
    public TMP_Text itemNameText;
    public TMP_Text descriptionText;
    public TMP_Text priceText;
    public TMP_Text sellPriceText;
    public Button buyButton;
    public Button sellButton;

    private ItemType itemType;
    private ItemShopManager shopManager;

    public void Setup(ItemType type, ItemShopManager manager)
    {
        itemType   = type;
        shopManager = manager;

        ItemData data = ItemCatalog.Items[type];
        itemNameText.text    = data.displayName;
        descriptionText.text = data.description;
        priceText.text       = $"${data.price}";
        sellPriceText.text   = $"Sell: ${Mathf.FloorToInt(data.price / 2f)}";

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => shopManager.OnBuyClicked(itemType));

        sellButton.onClick.RemoveAllListeners();
        sellButton.onClick.AddListener(() => shopManager.OnSellClicked(itemType));

        Refresh();
    }

    public void Refresh()
    {
        ItemData data = ItemCatalog.Items[itemType];
        int owned         = GameData.Instance.Inventory.FindAll(i => i == itemType).Count;
        bool canAfford    = GameData.Instance.BankedMoney >= data.price;
        bool invFull      = GameData.Instance.Inventory.Count >= 5;
        bool atMax        = owned >= data.maxOwnable;

        buyButton.interactable = canAfford && !invFull && !atMax;
        sellButton.gameObject.SetActive(owned > 0);
    }
}
