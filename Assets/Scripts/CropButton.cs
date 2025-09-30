using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class CropButton : MonoBehaviour
{
    public FarmGrid.SeedType seedType;
    private Button button;
    private Image buttonImage;

    [Header("UI References")]
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI plantNameText;

    [Header("Button Settings")]
    public Color affordableColor = Color.green;
    public Color cannotAffordColor = Color.gray;

    [Header("Price & Cooldown")]
    public int price = 20; // Set this individually for each button in Inspector
    public float cooldownTime = 2f;
    public Image cooldownOverlay;

    void Start()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        button.onClick.AddListener(OnCropButtonClick);

        if (cooldownOverlay != null)
        {
            cooldownOverlay.gameObject.SetActive(false);
        }

        UpdateButtonDisplay();
    }

    void Update()
    {
        UpdateButtonState();
    }

    void UpdateButtonState()
    {
        if (button == null || GameManager.Instance == null) return;

        bool canAfford = GameManager.Instance.Money >= price;
        button.interactable = canAfford;

        // Update button color based on affordability
        if (buttonImage != null)
        {
            buttonImage.color = canAfford ? affordableColor : cannotAffordColor;
        }

        // Update price text
        if (priceText != null)
        {
            priceText.text = $"R{price}";
            priceText.color = canAfford ? Color.white : Color.red;
        }

        // Update plant name
        if (plantNameText != null)
        {
            plantNameText.text = seedType.ToString();
        }
    }

    void UpdateButtonDisplay()
    {
        // Initial display setup
        if (priceText != null)
        {
            priceText.text = $"R{price}";
        }
        if (plantNameText != null)
        {
            plantNameText.text = seedType.ToString();
        }
    }

    void OnCropButtonClick()
    {
        if (GameManager.Instance != null && GameManager.Instance.Money >= price && button.interactable)
        {
            // Deduct money
            GameManager.Instance.Money -= price;

            // Enter planting mode
            if (FarmGrid.instance != null)
            {
                FarmGrid.instance.PrepareSowing(seedType);
            }

            Debug.Log($"Bought {seedType} for R{price}. Ready to plant!");

            // Cooldown
            StartCoroutine(StartCooldown());
        }
    }

    IEnumerator StartCooldown()
    {
        button.interactable = false;

        if (cooldownOverlay != null)
        {
            cooldownOverlay.gameObject.SetActive(true);
            float timer = 0f;

            while (timer < cooldownTime)
            {
                timer += Time.deltaTime;
                cooldownOverlay.fillAmount = 1f - (timer / cooldownTime);
                yield return null;
            }

            cooldownOverlay.gameObject.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(cooldownTime);
        }

        button.interactable = true;
        UpdateButtonState();
    }
}