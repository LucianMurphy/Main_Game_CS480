using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to each of the 5 item bar slot panels in the PacMan scene HUD.
public class ItemSlotUI : MonoBehaviour
{
    public Image itemIcon;
    public Image grayOverlay;
    public TMP_Text timerText;
    public GameObject emptyIndicator;

    public void SetItem(ItemType type)
    {
        ItemData data = ItemCatalog.Items[type];

        emptyIndicator.SetActive(false);
        itemIcon.gameObject.SetActive(true);
        itemIcon.sprite = data.icon;
        itemIcon.enabled = data.icon != null;
        grayOverlay.gameObject.SetActive(false);
        timerText.gameObject.SetActive(false);
    }

    public void ClearSlot()
    {
        emptyIndicator.SetActive(true);
        itemIcon.gameObject.SetActive(false);
        grayOverlay.gameObject.SetActive(false);
        timerText.gameObject.SetActive(false);
    }

    // Shield only: armed but waiting for a hit — grayed, no timer.
    public void ShowArmed()
    {
        grayOverlay.gameObject.SetActive(true);
        timerText.gameObject.SetActive(false);
    }

    // All other items: gray overlay + countdown timer.
    public void ShowActive(float duration)
    {
        grayOverlay.gameObject.SetActive(true);
        timerText.gameObject.SetActive(true);
        timerText.text = Mathf.CeilToInt(duration) + "s";
    }

    public void UpdateTimer(float remaining)
    {
        timerText.text = Mathf.CeilToInt(remaining) + "s";
    }
}
