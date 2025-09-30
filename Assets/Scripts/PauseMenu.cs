using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseMenuUI;      // Assign your pause panel
    public static bool isPaused = false;

    [Header("Audio")]
    public AudioSource audioSource;     // AudioSource for sounds
    public AudioClip hoverSound;        // Hover sound for buttons
    public AudioClip clickSound;        // Click sound for buttons

    void Update()
    {
        // Detect Escape key using new Input System
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    // Pause / Resume
    public void Resume()
    {
        PlayClickSound();
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    // Scene Management
    public void LoadMainMenu()
    {
        PlayClickSound();
        Time.timeScale = 1f; // Reset time
        SceneManager.LoadScene("Tshego"); // Replace with your main menu scene name
    }

    public void QuitGame()
    {
        PlayClickSound();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Button hover handler
    public void PlayHoverSound()
    {
        if (audioSource != null && hoverSound != null)
            audioSource.PlayOneShot(hoverSound);
    }

    // Button click sound helper
    private void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
}
