using System.Collections;
using UnityEngine;
using TMPro;

public class TextScript : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI textMeshPro;

    [Header("Typewriter Settings")]
    [TextArea]
    public string fullSentence = "Hello there!.";
    public float typingSpeed = 0.05f; // seconds per character
    public bool writingText = false;

    private void Start()
    {
        // Start the coroutine automatically
        StartCoroutine(ShowTextLetterByLetter());
    }

    public IEnumerator ShowTextLetterByLetter()
    {
        writingText = true;
        // Clear text first
        textMeshPro.text = "";

        // Go through each character one by one
        foreach (char c in fullSentence)
        {
            textMeshPro.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(5f);

        writingText = false;

        textMeshPro.text = "";

    }
}
