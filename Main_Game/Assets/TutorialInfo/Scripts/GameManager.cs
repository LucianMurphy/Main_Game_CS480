using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Shown only when the player wins the whole game (catches scared PacMan).
    public GameObject WinPanel;

    private bool gameOver = false;

    void Start()
    {
        if (WinPanel != null) WinPanel.SetActive(false);
    }

    // PacMan (not scared) catches the player -> player is killed, go to shop.
    //swapped the names of win and lose
    public void Lose()
    {
        if (gameOver) return;
        gameOver = true;
        UnlockCursor();
        GameData.Instance.OnPlayerKilled();
        SceneManager.LoadScene("ItemShop");
    }

    // PacMan (scared) is caught by the player -> whole-game win, show panel.
    public void Win()
    {
        if (gameOver) return;
        gameOver = true;
        UnlockCursor();
        if (WinPanel != null) WinPanel.SetActive(true);
    }

    // Player walks through the exit -> escaped, go to shop with money banked.
    public void PlayerEscaped()
    {
        if (gameOver) return;
        gameOver = true;
        UnlockCursor();
        GameData.Instance.OnPlayerEscaped();
        SceneManager.LoadScene("ItemShop");
    }

    // Wired to WinPanel "Try Again" button - resets everything.
    public void FullResetAndRestart()
    {
        GameData.Instance.FullReset();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Wired to WinPanel "Main Menu" button.
    public void GoToMainMenu()
    {
        GameData.Instance.FullReset();
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
