using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;
    public GameObject aboutContentUI;
    public Button pauseButton;

    public void Pause()
    {
        pauseMenuUI.SetActive(true); // Show the pause menu
        Time.timeScale = 0f; // Freeze the game
        GameIsPaused = true; // Game is paused
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false); // Hide the pause menu
        Time.timeScale = 1f; // Unfreeze the game
        GameIsPaused = false; // Game is no longer paused
    }

    public void LoadSettings()
    {
        pauseMenuUI.SetActive(false); // Hide the pause menu
        Debug.Log("Loading settings...");
    }

    public void CloseSettings()
    {
        Debug.Log("Closing settings...");
    }

    public void LoadAbout()
    {
        pauseMenuUI.SetActive(false); // Hide the pause menu
        aboutContentUI.SetActive(true); // Show the about content
    }

    public void CloseAbout()
    {
        pauseMenuUI.SetActive(true); // Show the pause menu
        aboutContentUI.SetActive(false); // Hide the about content
    }

    public void CloseCurrentMenu()
    {
        if (aboutContentUI.activeSelf)
        {
            CloseAbout();
        }
    }
}