using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CropButton : MonoBehaviour
{
    public FarmGrid.SeedType seedType;
    private Button button;

    [Header("Cooldown Settings")]
    public float cooldownTime = 2f;
    public Image cooldownOverlay;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnCropButtonClick);

        // Hide cooldown overlay initially
        if (cooldownOverlay != null)
        {
            cooldownOverlay.gameObject.SetActive(false);
        }
    }

    void OnCropButtonClick()
    {
        if (FarmGrid.instance != null && button.interactable)
        {
            FarmGrid.instance.PrepareSowing(seedType);
            StartCoroutine(StartCooldown());
        }
    }

    IEnumerator StartCooldown()
    {
        // Disable button
        button.interactable = false;

        // Show and animate cooldown overlay
        if (cooldownOverlay != null)
        {
            cooldownOverlay.gameObject.SetActive(true);
            float timer = 0f;

            while (timer < cooldownTime)
            {
                timer += Time.deltaTime;
                float fillAmount = 1f - (timer / cooldownTime);
                cooldownOverlay.fillAmount = fillAmount;
                yield return null;
            }

            cooldownOverlay.gameObject.SetActive(false);
        }
        else
        {
            // Fallback: just wait without visual
            yield return new WaitForSeconds(cooldownTime);
        }

        // Re-enable button
        button.interactable = true;
    }
}