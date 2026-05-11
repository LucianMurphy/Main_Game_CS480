using UnityEngine;
using TMPro;

// Attach to each of the 5 inventory slot panels in the ItemShop scene.
public class InventorySlotUI : MonoBehaviour
{
    public TMP_Text itemNameText;
    public GameObject emptyIndicator;

    public void Show(ItemType type)
    {
        emptyIndicator.SetActive(false);
        itemNameText.gameObject.SetActive(true);
        itemNameText.text = ItemCatalog.Items[type].displayName;
    }

    public void Clear()
    {
        emptyIndicator.SetActive(true);
        itemNameText.gameObject.SetActive(false);
    }
}
