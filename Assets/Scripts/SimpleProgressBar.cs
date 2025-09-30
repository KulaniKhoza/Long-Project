using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleProgressBar : MonoBehaviour
{
    public Image fillImage;
    public TextMeshProUGUI progressText;

    public void SetProgress(float progress, string text = "")
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01(progress);
        }

        if (progressText != null && !string.IsNullOrEmpty(text))
        {
            progressText.text = text;
        }
    }
}