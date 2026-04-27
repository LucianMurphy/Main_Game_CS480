using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject WinPanel;
    public GameObject LosePanel;

    private bool gameOver = false;

    void Start()
    {
        if (WinPanel != null) WinPanel.SetActive(false);
        if (LosePanel != null) LosePanel.SetActive(false);
    }

    public void Win()
    {
        if (gameOver) return;
        gameOver = true;
        if (WinPanel != null) WinPanel.SetActive(true);
    }

    public void Lose()
    {
        if (gameOver) return;
        gameOver = true;
        if (LosePanel != null) LosePanel.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
