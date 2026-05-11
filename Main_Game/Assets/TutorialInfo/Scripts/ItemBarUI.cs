using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// Attach to a persistent HUD GameObject in the PacMan scene.
// Wire up the 5 ItemSlotUI references and scene references in the Inspector.
public class ItemBarUI : MonoBehaviour
{
    [Header("Slot UI References (0 = slot 1 / hotkey 1)")]
    public ItemSlotUI[] slots; // exactly 5

    [Header("Scene References")]
    public MinimapController minimapController;

    private PlayerMovement playerMovement;
    private PlayerStatus   playerStatus;
    private PacAI          pacAI;

    private bool[] slotActive = new bool[5];

    void Start()
    {
        playerMovement = Object.FindAnyObjectByType<PlayerMovement>();
        playerStatus   = Object.FindAnyObjectByType<PlayerStatus>();
        pacAI          = Object.FindAnyObjectByType<PacAI>();
        RefreshSlots();
    }

    void Update()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current.digit1Key.wasPressedThisFrame) TryActivateSlot(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) TryActivateSlot(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) TryActivateSlot(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) TryActivateSlot(3);
        if (Keyboard.current.digit5Key.wasPressedThisFrame) TryActivateSlot(4);
    }

    public void RefreshSlots()
    {
        var inv = GameData.Instance.Inventory;
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inv.Count) slots[i].SetItem(inv[i]);
            else               slots[i].ClearSlot();
        }
    }

    void TryActivateSlot(int index)
    {
        if (slotActive[index]) return;
        if (index >= GameData.Instance.Inventory.Count) return;

        ItemType type = GameData.Instance.Inventory[index];
        slotActive[index] = true;

        if (type == ItemType.Shield)
        {
            playerStatus.ActivateShield();
            slots[index].ShowArmed();
            StartCoroutine(ShieldSlotRoutine(index));
        }
        else
        {
            StartCoroutine(SlotTimer(index, type, ItemCatalog.Items[type].duration));
        }
    }

    void ActivateEffect(ItemType type)
    {
        switch (type)
        {
            case ItemType.EnergyDrink:
                if (playerMovement != null) playerMovement.speedMultiplier = 2.5f;
                break;
            case ItemType.Map:
                if (minimapController != null) minimapController.ShowMinimap();
                break;
            case ItemType.Burger:
                if (playerStatus != null) playerStatus.ActivateBurger();
                break;
            case ItemType.FlashBang:
                if (pacAI != null) pacAI.Stun(ItemCatalog.Items[ItemType.FlashBang].duration);
                break;
        }
    }

    void DeactivateEffect(ItemType type)
    {
        switch (type)
        {
            case ItemType.EnergyDrink:
                if (playerMovement != null) playerMovement.speedMultiplier = 1f;
                break;
            case ItemType.Burger:
                if (playerStatus != null) playerStatus.DeactivateBurger();
                break;
            // Map and FlashBang manage their own cleanup internally.
        }
    }

    IEnumerator SlotTimer(int index, ItemType type, float duration)
    {
        ActivateEffect(type);
        slots[index].ShowActive(duration);

        float remaining = duration;
        while (remaining > 0f)
        {
            remaining -= Time.deltaTime;
            slots[index].UpdateTimer(remaining);
            yield return null;
        }

        DeactivateEffect(type);
        ConsumeSlot(index);
    }

    // Shield: wait until the shield is consumed by a hit, then show the invincibility countdown.
    IEnumerator ShieldSlotRoutine(int index)
    {
        // Poll until the shield is consumed (PlayerStatus.TryAbsorbHit sets IsShielded=false).
        while (playerStatus != null && playerStatus.IsShielded)
            yield return null;

        // Shield absorbed a hit - now show the 5s invincibility timer in the slot.
        float invDuration = ItemCatalog.Items[ItemType.Shield].duration;
        slots[index].ShowActive(invDuration);

        float remaining = invDuration;
        while (remaining > 0f)
        {
            remaining -= Time.deltaTime;
            slots[index].UpdateTimer(remaining);
            yield return null;
        }

        ConsumeSlot(index);
    }

    void ConsumeSlot(int index)
    {
        GameData.Instance.RemoveItemAt(index);
        slots[index].ClearSlot();
        slotActive[index] = false;
        RefreshSlots();

        // Any slot index above the removed one shifts down by 1, so clear stale active flags.
        for (int i = GameData.Instance.Inventory.Count; i < 5; i++)
            slotActive[i] = false;
    }
}
