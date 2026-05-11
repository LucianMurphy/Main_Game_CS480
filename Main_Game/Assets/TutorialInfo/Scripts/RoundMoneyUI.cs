using UnityEngine;
using TMPro;

// Attach to the round money text element in the PacMan scene HUD.
public class RoundMoneyUI : MonoBehaviour
{
    public TMP_Text roundMoneyText;

    void Update()
    {
        roundMoneyText.text = $"Round: ${GameData.Instance.RoundMoney}";
    }
}
