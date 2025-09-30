using UnityEngine;
using UnityEngine.UI; // or TMPro if using TextMeshPro
using System.Collections;

public class ReadyGoFade : MonoBehaviour
{
    public CanvasGroup readyTextCanvasGroup;  // Assign CanvasGroup
    public float fadeInTime = 0.5f;
    public float displayTime = 1.0f;
    public float fadeOutTime = 0.5f;
    public AudioSource audioSource;           // Optional Go! sound
    public AudioClip goSound;

    void Start()
    {
        // Pause game at start if needed
        Time.timeScale = 0f;
        StartCoroutine(ShowReadyGo());
    }

    private IEnumerator ShowReadyGo()
    {
        // Fade in
        float timer = 0f;
        while (timer < fadeInTime)
        {
            timer += Time.unscaledDeltaTime;
            readyTextCanvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeInTime);
            yield return null;
        }
        readyTextCanvasGroup.alpha = 1;

        // Optional: play sound at the start of "Go!"
        if (audioSource != null && goSound != null)
        {
            audioSource.PlayOneShot(goSound);
        }

        // Wait while fully visible
        yield return new WaitForSecondsRealtime(displayTime);

        // Fade out
        timer = 0f;
        while (timer < fadeOutTime)
        {
            timer += Time.unscaledDeltaTime;
            readyTextCanvasGroup.alpha = Mathf.Lerp(1, 0, timer / fadeOutTime);
            yield return null;
        }
        readyTextCanvasGroup.alpha = 0;

        // Enable gameplay
        Time.timeScale = 1f;
    }
}
