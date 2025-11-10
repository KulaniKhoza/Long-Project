using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WaterTank : MonoBehaviour
{
    public static WaterTank Instance { get; private set; }

    [Header("Water Tank Settings")]
    public float maxWaterCapacity = 1000f;
    public float currentWaterAmount = 1000f;

    [Header("UI References")]
    public Image waterFillImage; // UI Image with Image Type set to Filled
    public Text waterText; // Optional text display

    [Header("Refill Settings")]
    public int refillCost = 50; // Cost to refill the tank
    public float refillAmount = 1000f; // Amount to refill
    public float refillDuration = 8f; // How long the refill animation takes

    private Coroutine fillCoroutine;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            // Optional: Don't destroy on load if you want it persistent
            // DontDestroyOnLoad(gameObject);
        }
        Time.timeScale = 1.0f;
    }

    void Start()
    {
        UpdateTankUI();
    }

    void UpdateTankUI()
    {
        // Update the fill amount of the image (0-1)
        if (waterFillImage != null)
        {
            waterFillImage.fillAmount = currentWaterAmount / maxWaterCapacity;
        }

        // Update text if available
        if (waterText != null)
        {
            waterText.text = $"Water: {Mathf.RoundToInt(currentWaterAmount)}/{Mathf.RoundToInt(maxWaterCapacity)}";
        }
    }

    // Check if we can withdraw water
    public bool CanWithdrawWater(float amount)
    {
        return currentWaterAmount >= amount;
    }

    // Withdraw water from tank (returns true if successful)
    public bool WithdrawWater(float amount)
    {
        if (CanWithdrawWater(amount))
        {
            currentWaterAmount -= amount;
            UpdateTankUI();
            return true;
        }
        return false;
    }

    // Add water to tank (for refilling) - Instant version
    public void AddWater(float amount)
    {
        currentWaterAmount = Mathf.Min(currentWaterAmount + amount, maxWaterCapacity);
        UpdateTankUI();
    }

    // Add water to tank with smooth animation
    public void AddWaterSmooth(float amount)
    {
        if (fillCoroutine != null)
            StopCoroutine(fillCoroutine);

        fillCoroutine = StartCoroutine(FillWaterSmooth(amount));
    }

    // IEnumerator for smooth water filling
    private IEnumerator FillWaterSmooth(float amount)
    {
        float targetAmount = Mathf.Min(currentWaterAmount + amount, maxWaterCapacity);
        float startAmount = currentWaterAmount;
        float elapsedTime = 0f;

        while (elapsedTime < refillDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / refillDuration;

            // Smooth interpolation
            currentWaterAmount = Mathf.Lerp(startAmount, targetAmount, progress);
            UpdateTankUI();

            yield return null;
        }

        // Ensure we end exactly at the target amount
        currentWaterAmount = targetAmount;
        UpdateTankUI();
    }

    // Refill tank button method - Instant version
    public void RefillTank()
    {
        if (GameManager.Instance.Money >= refillCost)
        {
            GameManager.Instance.Money -= refillCost;
            currentWaterAmount = maxWaterCapacity;
            UpdateTankUI();

            // Show UI feedback
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SpawnUIAboveField(transform, $"-R{refillCost}");
            }
            Debug.Log($"Water tank refilled! Cost: R{refillCost}");
        }
        else
        {
            Debug.Log("Not enough money to refill water tank!");
        }
    }

    // Refill tank with smooth animation
    public void RefillTankSmooth()
    {
        if (GameManager.Instance.Money >= refillCost)
        {
            GameManager.Instance.Money -= refillCost;

            if (fillCoroutine != null)
                StopCoroutine(fillCoroutine);

            fillCoroutine = StartCoroutine(FillToFull());

            // Show UI feedback
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SpawnUIAboveField(transform, $"-R{refillCost}");
            }
            Debug.Log($"Water tank refilled smoothly! Cost: R{refillCost}");
        }
        else
        {
            Debug.Log("Not enough money to refill water tank!");
        }
    }

    // IEnumerator for filling tank to full capacity
    private IEnumerator FillToFull()
    {
        float startAmount = currentWaterAmount;
        float elapsedTime = 0f;

        while (elapsedTime < refillDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / refillDuration;

            // Smooth interpolation from current to max
            currentWaterAmount = Mathf.Lerp(startAmount, maxWaterCapacity, progress);
            UpdateTankUI();

            yield return null;
        }

        // Ensure we end exactly at max capacity
        currentWaterAmount = maxWaterCapacity;
        UpdateTankUI();
    }

    // Get current water percentage (0-1)
    public float GetWaterPercentage()
    {
        return currentWaterAmount / maxWaterCapacity;
    }

    // Get current water amount
    public float GetCurrentWater()
    {
        return currentWaterAmount;
    }

    // Check if tank is empty
    public bool IsEmpty()
    {
        return currentWaterAmount <= 0;
    }

    // Stop any ongoing fill coroutine
    public void StopFillAnimation()
    {
        if (fillCoroutine != null)
        {
            StopCoroutine(fillCoroutine);
            fillCoroutine = null;
        }
    }
}