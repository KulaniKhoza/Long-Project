using UnityEngine;
using UnityEngine.UI;

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

    // Add water to tank (for refilling)
    public void AddWater(float amount)
    {
        currentWaterAmount = Mathf.Min(currentWaterAmount + amount, maxWaterCapacity);
        UpdateTankUI();
    }

    // Refill tank button method
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
}