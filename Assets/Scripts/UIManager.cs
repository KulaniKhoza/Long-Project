using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public void PlayAgain()
    {
        // Reload the same level (e.g. "Kulani")
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1f; // Resume the game
    }

    public void MainMenu()
    {
        // Go back to the start screen
        SceneManager.LoadScene("Tshego");
        Time.timeScale = 1f; // Resume the game
    }

    public void Quit()
    {
        // Quit the game
        Application.Quit();
        Debug.Log("Game exiting..."); // Works only in build
    }
}
