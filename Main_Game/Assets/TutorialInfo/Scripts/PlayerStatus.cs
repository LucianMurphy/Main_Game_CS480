using System.Collections;
using UnityEngine;

// Attached to the Player GameObject alongside PlayerMovement.
// Tracks shield, invincibility, and burger width state.
public class PlayerStatus : MonoBehaviour
{
    public static PlayerStatus Instance { get; private set; }

    public bool IsShielded   { get; private set; } = false;
    public bool IsInvincible { get; private set; } = false;
    public bool IsBurgerActive { get; private set; } = false;

    // Raised when the shield absorbs a hit so ItemBarUI can start the invincibility timer display.
    public System.Action OnShieldConsumed;

    private Vector3 originalScale;
    private float originalControllerRadius;
    private CharacterController cc;

    void Awake()
    {
        Instance = this;
        cc = GetComponent<CharacterController>();
        originalScale = transform.localScale;
        if (cc != null) originalControllerRadius = cc.radius;
    }

    public void ActivateShield()
    {
        IsShielded = true;
    }

    // Called from PacAI when the player would be killed.
    // Returns true if the hit was absorbed (player survives).
    public bool TryAbsorbHit()
    {
        if (IsInvincible) return true;

        if (IsShielded)
        {
            IsShielded = false;
            OnShieldConsumed?.Invoke();
            StartCoroutine(InvincibilityTimer(ItemCatalog.Items[ItemType.Shield].duration));
            return true;
        }

        return false;
    }

    private IEnumerator InvincibilityTimer(float duration)
    {
        IsInvincible = true;
        yield return new WaitForSeconds(duration);
        IsInvincible = false;
    }

    public void ActivateBurger()
    {
        if (IsBurgerActive) return;
        IsBurgerActive = true;
        transform.localScale = new Vector3(originalScale.x * 1.5f, originalScale.y, originalScale.z);
        if (cc != null) cc.radius = originalControllerRadius * 1.5f;
    }

    public void DeactivateBurger()
    {
        IsBurgerActive = false;
        transform.localScale = originalScale;
        if (cc != null) cc.radius = originalControllerRadius;
    }

    public void ResetAll()
    {
        IsShielded = false;
        IsInvincible = false;
        StopAllCoroutines();
        DeactivateBurger();
    }
}
