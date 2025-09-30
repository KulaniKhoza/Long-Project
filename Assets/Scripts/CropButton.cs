using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class UniversalButton : MonoBehaviour
{
    [Header("Button Type")]
    public bool isFarmingButton = true;

    [Header("Farming Settings")]
    public FarmGrid.SeedType seedType;
    public int farmingPrice = 20;

    [Header("Defending Settings")]
    public FarmGrid.DefenderType defenderType;
    public int defendingPrice = 50;

    [Header("UI References")]
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI nameText;

    [Header("Button Settings")]
    public Color affordableColor = Color.green;
    public Color cannotAffordColor = Color.gray;

    [Header("Cooldown")]
    public float cooldownTime = 2f;
    public Image cooldownOverlay;

    private Button button;
    private Image buttonImage;
    private FarmGrid farmGrid;

    void Start()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        farmGrid = FarmGrid.instance;

        button.onClick.AddListener(OnButtonClick);

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
        if (button == null || farmGrid == null || GameManager.Instance == null) return;

        int currentPrice = isFarmingButton ? farmingPrice : defendingPrice;
        bool canAfford = GameManager.Instance.Money >= currentPrice;
        bool isCorrectMode = (isFarmingButton && farmGrid.currentMode == FarmGrid.Mode.Farming) ||
                           (!isFarmingButton && farmGrid.currentMode == FarmGrid.Mode.Defending);

        button.interactable = canAfford && isCorrectMode;

        // Update button color based on affordability
        if (buttonImage != null)
        {
            buttonImage.color = canAfford ? affordableColor : cannotAffordColor;
        }

        // Update price text
        if (priceText != null)
        {
            priceText.text = $"R{currentPrice}";
            priceText.color = canAfford ? Color.white : Color.red;
        }

        // Update name text
        if (nameText != null)
        {
            if (isFarmingButton)
            {
                nameText.text = seedType.ToString();
            }
            else
            {
                nameText.text = defenderType.ToString();
            }
        }
    }

    void UpdateButtonDisplay()
    {
        // Initial display setup
        int currentPrice = isFarmingButton ? farmingPrice : defendingPrice;

        if (priceText != null)
        {
            priceText.text = $"R{currentPrice}";
        }

        if (nameText != null)
        {
            if (isFarmingButton)
            {
                nameText.text = seedType.ToString();
            }
            else
            {
                nameText.text = defenderType.ToString();
            }
        }
    }

    void OnButtonClick()
    {
        if (GameManager.Instance == null || farmGrid == null) return;

        int currentPrice = isFarmingButton ? farmingPrice : defendingPrice;

        if (GameManager.Instance.Money >= currentPrice && button.interactable)
        {
            // Deduct money
            GameManager.Instance.Money -= currentPrice;

            // Enter placement mode
            if (isFarmingButton)
            {
                farmGrid.PrepareSowing(seedType);
                Debug.Log($"Bought {seedType} for R{currentPrice}. Ready to plant!");
            }
            else
            {
                farmGrid.PrepareDefender(defenderType);
                Debug.Log($"Bought {defenderType} for R{currentPrice}. Ready to place!");
            }

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