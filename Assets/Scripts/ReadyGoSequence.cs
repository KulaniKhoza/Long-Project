using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ReadyGoSequence : MonoBehaviour
{
    public CanvasGroup readyTextCanvasGroup;
    public CanvasGroup goTextCanvasGroup;
    public float readyFadeTime = 0.5f;
    public float readyDisplayTime = 1.0f;
    public float goFadeTime = 0.3f;
    public float goDisplayTime = 0.5f;
    public AudioSource audioSource;
    public AudioClip goSound;

    void Start()
    {
        Time.timeScale = 0f; // Pause game
        StartCoroutine(ShowReadyGo());
    }

    private IEnumerator ShowReadyGo()
    {
        // Fade in "Ready to Plant?"
        float timer = 0f;
        while (timer < readyFadeTime)
        {
            timer += Time.unscaledDeltaTime;
            readyTextCanvasGroup.alpha = Mathf.Lerp(0, 1, timer / readyFadeTime);
            yield return null;
        }
        readyTextCanvasGroup.alpha = 1;

        // Display "Ready" for a short time
        yield return new WaitForSecondsRealtime(readyDisplayTime);

        // Fade out "Ready to Plant?"
        timer = 0f;
        while (timer < readyFadeTime)
        {
            timer += Time.unscaledDeltaTime;
            readyTextCanvasGroup.alpha = Mathf.Lerp(1, 0, timer / readyFadeTime);
            yield return null;
        }
        readyTextCanvasGroup.alpha = 0;

        // Fade in "GO!"
        timer = 0f;
        while (timer < goFadeTime)
        {
            timer += Time.unscaledDeltaTime;
            goTextCanvasGroup.alpha = Mathf.Lerp(0, 1, timer / goFadeTime);
            yield return null;
        }
        goTextCanvasGroup.alpha = 1;

        // Optional: play Go sound
        if (audioSource != null && goSound != null)
            audioSource.PlayOneShot(goSound);

        // Display "GO!" briefly
        yield return new WaitForSecondsRealtime(goDisplayTime);

        // Fade out "GO!"
        timer = 0f;
        while (timer < goFadeTime)
        {
            timer += Time.unscaledDeltaTime;
            goTextCanvasGroup.alpha = Mathf.Lerp(1, 0, timer / goFadeTime);
            yield return null;
        }
        goTextCanvasGroup.alpha = 0;

        // Resume gameplay
        Time.timeScale = 1f;
    }
}
