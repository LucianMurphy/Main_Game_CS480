using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    void Start()
    {
        // Wipe all persistent progress whenever the main menu loads.
        GameData.Instance.FullReset();
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LoadPacMan()
    {
        SceneManager.LoadScene("PacMan");
    }

    public void LoadVampireHunter()
    {
        SceneManager.LoadScene("Mansion");
    }

    public void LoadSlenderMan()
    {
        SceneManager.LoadScene("SlenderLevel");
    }
}
