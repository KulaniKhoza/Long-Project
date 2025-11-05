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
    public Color disabledColor = Color.gray;

    [Header("Cooldown")]
    public float cooldownTime = 2f;
    public Image cooldownOverlay;

    private Button button;
    private Image buttonImage;
    private FarmGrid farmGrid;
    private bool isEnabled = false;

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

        // Start disabled
        DisableButton();
        UpdateButtonDisplay();
    }

    void Update()
    {
        if (isEnabled)
        {
            UpdateButtonState();
        }
    }

    void UpdateButtonState()
    {
        if (button == null || farmGrid == null || GameManager.Instance == null) return;

        int currentPrice = isFarmingButton ? farmingPrice : defendingPrice;
        bool canAfford = GameManager.Instance.Money >= currentPrice;

        button.interactable = canAfford;

        if (buttonImage != null)
        {
            buttonImage.color = canAfford ? affordableColor : cannotAffordColor;
        }

        if (priceText != null)
        {
            priceText.text = $"R{currentPrice}";
            priceText.color = canAfford ? Color.darkGreen : Color.red;
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

    void UpdateButtonDisplay()
    {
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
            GameManager.Instance.Money -= currentPrice;

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

        if (isEnabled)
        {
            button.interactable = GameManager.Instance.Money >= (isFarmingButton ? farmingPrice : defendingPrice);
            UpdateButtonState();
        }
    }

    // Public methods to control button state
    public void EnableIfAffordable()
    {
        isEnabled = true;
        int currentPrice = isFarmingButton ? farmingPrice : defendingPrice;
        bool canAfford = GameManager.Instance.Money >= currentPrice;

        button.interactable = canAfford;

        if (buttonImage != null)
        {
            buttonImage.color = canAfford ? affordableColor : cannotAffordColor;
        }
    }

    public void DisableButton()
    {
        isEnabled = false;
        button.interactable = false;

        if (buttonImage != null)
        {
            buttonImage.color = disabledColor;
        }
    }
}