using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class Crops : MonoBehaviour
{
    [Header("Crop References")]
    public GameManager MoneyManager;
    public CropData cropData;
    public SpriteRenderer spriteRenderer;
    public Sprite[] levelSprites;

    [Header("Crop State")]
    public int plantLevel = 0;
    public int waterLevel = 0;
    public int maxWater = 100;
    private bool isMaxLevel = false;

    [Header("Watering Settings")]
    private bool isWatering = false;
    private float wateringTimer = 0f;
    public float wateringInterval = 0.2f;
    public int holdWaterAmount = 8;

    [Header("Money Generation")]
    private float moneyTimer = 0f;
    public float moneyGenerationInterval = 4f;
    private bool isAlive = true;

    [Header("Health System")]
    public int maxHealth = 4;    // Hits needed before death
    public int currentHealth;
    private Coroutine damageEffectCoroutine;
    private bool wasDamagedThisFrame = false;

    [Header("Progress Bar")]
    public Image waterProgressBar;
    public TextMeshProUGUI waterProgressText;
    public float smoothFillDuration = 2f; // Duration for smooth fill animation
    private Coroutine smoothFillCoroutine;
    private bool isAnimating = false;
    public GameObject WaterPanel;
    [Header("Selection Settings")]
    private bool isSelected = false;
    private Color originalColor;
    public Color selectedColor = Color.blue;
    private Coroutine flashCoroutine;
    
    // Static reference for button access
    public static Crops CurrentlySelectedCrop { get; private set; }

    void Start()
    {
        // Initialize components
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (MoneyManager == null)
            MoneyManager = GameManager.Instance;

        // Store original color
        originalColor = spriteRenderer.color;

        // Initialize health
        currentHealth = maxHealth;

        // Initialize progress bar
        InitializeProgressBar();
        UpdateSprite();

        Debug.Log("Crop initialized at level: " + plantLevel);
    }

    void InitializeProgressBar()
    {
        if (waterProgressBar != null)
        {
            waterProgressBar.type = Image.Type.Filled;
            waterProgressBar.fillMethod = Image.FillMethod.Horizontal;
            waterProgressBar.fillAmount = 0f;
        }

        if (waterProgressText != null)
        {
            waterProgressText.text = $"0/{maxWater}";
        }
    }

    void Update()
    {
        if (!isAlive) return;

        // Handle continuous watering if this crop is selected and W key is held
        // Don't allow watering if at max level and fully watered OR if animation is playing
        if (isSelected && Keyboard.current.wKey.isPressed && !isWatering && !(isMaxLevel && waterLevel >= maxWater) && !isAnimating)
        {
            StartWatering();
        }
        else if ((!isSelected || !Keyboard.current.wKey.isPressed) && isWatering)
        {
            StopWatering();
        }

        // Handle the actual watering process
        if (isWatering)
        {
            HandleContinuousWatering();
        }

        // Level up check - only if not at max level
        if (!isMaxLevel && plantLevel < 3 && waterLevel >= maxWater)
        {
            LevelUp();
        }

        GenerateMoney();

        // CONTINUOUS HEALTH MONITORING
        // Reset damage flag for next frame
        wasDamagedThisFrame = false;
    }

    void GenerateMoney()
    {
        // Generate money at EVERY level when fully watered
        if (plantLevel >= 1) // Level 1 and above
        {
            moneyTimer += Time.deltaTime;

            if (moneyTimer >= moneyGenerationInterval)
            {
                // Calculate money based on plant level: R5 at level 1, R7 at level 2, R9 at level 3
                int moneyToAdd = CalculateMoneyAmount();

                if (MoneyManager != null)
                {
                    // ADD MONEY TO MANAGER
                    MoneyManager.AddMoney(moneyToAdd);

                    // SPAWN UI - FIXED POSITION
                    MoneyManager.SpawnUIAboveField(transform, $"+R{moneyToAdd}");

                    Debug.Log($"Money generated: +R{moneyToAdd} at level {plantLevel}. Total money: {MoneyManager.Money}");
                }
                else
                {
                    Debug.LogError("MoneyManager is null!");
                }

                moneyTimer = 0f;
            }
        }
    }

    // Calculate money amount based on plant level
    private int CalculateMoneyAmount()
    {
        switch (plantLevel)
        {
            case 1: return 5; // R5 at level 1
            case 2: return 7; // R7 at level 2
            case 3: return 9; // R9 at level 3
            default: return 0; // No money at level 0
        }
    }

    void HandleContinuousWatering()
    {
        if (isWatering && !(isMaxLevel && waterLevel >= maxWater) && !isAnimating)
        {
            wateringTimer += Time.deltaTime;

            if (wateringTimer >= wateringInterval)
            {
                Watering(holdWaterAmount);
                wateringTimer = 0f;
            }
        }
    }

    void UpdateProgressBar()
    {
        // Don't update progress bar if smooth animation is running
        if (isAnimating) return;

        if (waterProgressBar != null)
        {
            float progress = (float)waterLevel / maxWater;
            waterProgressBar.fillAmount = progress;
        }

        if (waterProgressText != null)
        {
            waterProgressText.text = $"{waterLevel}/{maxWater}";
        }
        if (waterProgressText != null && isMaxLevel && waterLevel >= maxWater)
        {
            waterProgressText.text = $"MAX";
        }
    }

    public void Watering(int amount)
    {
        // Don't allow watering if at max level and already fully watered or if animating
        if (isMaxLevel && waterLevel >= maxWater || isAnimating) return;

        int oldWaterLevel = waterLevel;
        waterLevel = Mathf.Min(waterLevel + amount, maxWater);

        // Update progress bar instantly (no smooth animation for manual watering)
        UpdateProgressBar();

        // Visual feedback when water level changes significantly
        if (waterLevel / 20 != oldWaterLevel / 20)
        {
            Debug.Log($"Watering crop. Water level: {waterLevel}/{maxWater}");
        }

        // If we reached max water at max level, ensure progress bar stays full
        if (isMaxLevel && waterLevel >= maxWater)
        {
            waterLevel = maxWater; // Ensure it's exactly max
            UpdateProgressBar();
        }
    }

    // =======================
    // BUTTON-TRIGGERED SMOOTH FILL ANIMATION
    // =======================

    // Call this method when your button is pressed
    public void StartSmoothWaterFill()
    {
        if (isAnimating)
        {
            Debug.Log("Water fill animation already in progress!");
            return;
        }

        if (isMaxLevel && waterLevel >= maxWater)
        {
            Debug.Log("Crop is already at maximum water level!");
            return;
        }

        // Start the smooth fill animation (no water tank check needed)
        StartCoroutine(SmoothFillToMax());
        Debug.Log($"Started smooth water fill animation!");
    }

    // IEnumerator for smooth fill animation when button is pressed
    private IEnumerator SmoothFillToMax()
    {
        isAnimating = true;

        float startFill = (float)waterLevel / maxWater;
        float targetFill = 1f;
        float elapsedTime = 0f;
        int startWaterLevel = waterLevel;

        while (elapsedTime < smoothFillDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / smoothFillDuration;

            // Smooth interpolation
            float currentFill = Mathf.Lerp(startFill, targetFill, progress);

            // Update progress bar
            if (waterProgressBar != null)
            {
                waterProgressBar.fillAmount = currentFill;
            }

            // Calculate current water level for text display
            int currentWaterValue = Mathf.RoundToInt(Mathf.Lerp(startWaterLevel, maxWater, progress));

            // Update text
            if (waterProgressText != null)
            {
                waterProgressText.text = $"{currentWaterValue}/{maxWater}";
            }

            yield return null;
        }

        // Ensure we end exactly at max
        waterLevel = maxWater;

        if (waterProgressBar != null)
        {
            waterProgressBar.fillAmount = 1f;
        }

        if (waterProgressText != null)
        {
            if (isMaxLevel)
            {
                waterProgressText.text = "MAX";
            }
            else
            {
                waterProgressText.text = $"{maxWater}/{maxWater}";
            }
        }

        isAnimating = false;

        // Check for level up after animation completes
        if (!isMaxLevel && plantLevel < 3)
        {
            LevelUp();
        }

        Debug.Log("Smooth water fill animation completed!");
    }

    // Public method for button to check if this crop can be watered
    public bool CanBeWatered()
    {
        return !isMaxLevel &&
               waterLevel < maxWater &&
               !isAnimating;
    }

    // Public method to check if crop is at max level
    public bool IsMaxLevel()
    {
        return isMaxLevel && waterLevel >= maxWater;
    }

    // Static method for button to water the currently selected crop
    public static void WaterSelectedCrop()
    {
        if (CurrentlySelectedCrop != null)
        {
            CurrentlySelectedCrop.StartSmoothWaterFill();
        }
        else
        {
            Debug.Log("No crop selected to water!");
        }
    }

    // Static method for button to check if any crop can be watered
    public static bool CanWaterAnyCrop()
    {
        return CurrentlySelectedCrop != null && CurrentlySelectedCrop.CanBeWatered();
    }

    public void StartWatering()
    {
        // Don't allow watering if at max level and already fully watered or if animating
        if (isMaxLevel && waterLevel >= maxWater || isAnimating) return;

        isWatering = true;
        Debug.Log("Started watering crop");
    }

    public void StopWatering()
    {
        if (isWatering)
        {
            isWatering = false;
            wateringTimer = 0f;
            Debug.Log("Stopped watering crop");
        }
    }

    void LevelUp()
    {
        if (plantLevel >= 0 && plantLevel < 3)
        {
            // Store current selection state before leveling up
            bool wasSelected = isSelected;

            plantLevel++;
            UpdateSprite();

            // Check if this is the final level
            if (plantLevel >= 3)
            {
                isMaxLevel = true;
                // Don't reset water level for final level - keep it full
                waterLevel = maxWater;
                UpdateProgressBar();
                Debug.Log($"Crop reached MAX level {plantLevel}! Water level maintained at maximum. Will generate R{CalculateMoneyAmount()} every {moneyGenerationInterval} seconds.");
            }
            else
            {
                // Reset water level for non-final levels
                waterLevel = 0;
                UpdateProgressBar();
            }

            // Restore selection state after level up
            if (wasSelected)
            {
                RestartFlashing();
            }

            Debug.Log($"Crop leveled up to level {plantLevel}! Money generation: R{CalculateMoneyAmount()}");
        }
    }

    void UpdateSprite()
    {
        if (levelSprites != null && levelSprites.Length > 0 &&
            plantLevel >= 0 && plantLevel < levelSprites.Length)
        {
            spriteRenderer.sprite = levelSprites[plantLevel];

            // Only update originalColor if we're NOT currently selected
            if (!isSelected)
            {
                originalColor = spriteRenderer.color;
            }
        }
    }

    // =======================
    // SELECTION METHODS (UPDATED FOR STATIC REFERENCE)
    // =======================

    public void SelectCrop()
    {
        if (isSelected) return;

        // Deselect previous crop
        if (CurrentlySelectedCrop != null && CurrentlySelectedCrop != this)
        {
            CurrentlySelectedCrop.DeselectCrop();
        }

        isSelected = true;
        CurrentlySelectedCrop = this;

        // Stop any existing flash coroutine
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        // Start flashing
        flashCoroutine = StartCoroutine(FlashCrop()); 
        WaterPanel.SetActive(true);
        Debug.Log("Crop selected for watering");
    }

    public void DeselectCrop()
    {
        isSelected = false;

        if (CurrentlySelectedCrop == this)
        {
            CurrentlySelectedCrop = null;
        }

        // Stop flashing
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        WaterPanel.SetActive(false);
        // Return to original color
        spriteRenderer.color = originalColor;
        StopWatering();
        Debug.Log("Crop deselected");
    }

    // Method to restart flashing after level up
    private void RestartFlashing()
    {
        if (isSelected)
        {
            // Stop any existing flash coroutine
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            // Ensure we have the correct original color for the new sprite
            spriteRenderer.color = GetBaseSpriteColor();
            originalColor = spriteRenderer.color;

            // Restart flashing
            flashCoroutine = StartCoroutine(FlashCrop());
            Debug.Log("Restarted flashing after level up");
        }
    }

    // Get the base color of the current sprite
    private Color GetBaseSpriteColor()
    {
        return Color.white;
    }

    // Flashing coroutine
    private IEnumerator FlashCrop()
    {
        while (isSelected)
        {
            // Flash to blue
            spriteRenderer.color = selectedColor;
            yield return new WaitForSeconds(0.3f);

            // Return to normal color
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.3f);
        }

        // Ensure we return to normal color when done
        spriteRenderer.color = originalColor;
    }

    // =======================
    // HEALTH SYSTEM FOR ENEMIES
    // =======================

    // Called by Enemy script when attacking this crop
    public void TakeDamage(int damage = 1)
    {
        if (!isAlive) return;

        // Set damage flag for this frame
        wasDamagedThisFrame = true;

        currentHealth -= damage;
        Debug.Log($"Crop took {damage} damage! Health: {currentHealth}/{maxHealth}");

        // Play damage effect immediately
        PlayDamageEffect();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Play damage effect (can be called continuously)
    void PlayDamageEffect()
    {
        // Stop any existing damage effect
        if (damageEffectCoroutine != null)
            StopCoroutine(damageEffectCoroutine);

        // Start new damage effect
        damageEffectCoroutine = StartCoroutine(DamageEffect());
    }

    // Visual effect when crop takes damage
    private IEnumerator DamageEffect()
    {
        // Flash red a few times
        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);

            // Return to appropriate color based on selection state
            if (isSelected)
                spriteRenderer.color = selectedColor;
            else
                spriteRenderer.color = originalColor;

            yield return new WaitForSeconds(0.1f);
        }
    }

    // Public method to check if crop was damaged (for external systems)
    public bool WasDamagedThisFrame()
    {
        return wasDamagedThisFrame;
    }

    // Public method to get current health status
    public HealthStatus GetHealthStatus()
    {
        float healthPercent = (float)currentHealth / maxHealth;

        if (healthPercent > 0.6f) return HealthStatus.Healthy;
        if (healthPercent > 0.3f) return HealthStatus.Damaged;
        return HealthStatus.Critical;
    }

    public enum HealthStatus
    {
        Healthy,
        Damaged,
        Critical
    }

    void Die()
    {
        isAlive = false;
        Debug.Log("Crop has been destroyed!");

        // Stop all coroutines
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        if (damageEffectCoroutine != null)
            StopCoroutine(damageEffectCoroutine);
        if (smoothFillCoroutine != null)
            StopCoroutine(smoothFillCoroutine);

        // If this was the selected crop, clear the selection
        if (CurrentlySelectedCrop == this)
        {
            CurrentlySelectedCrop = null;
        }

        Destroy(gameObject);
    }

    void HarvestCrop()
    {
        if (GameManager.Instance != null && GameManager.Instance.Money >= 0)
        {
            if (cropData != null)
            {
                int harvestValue = cropData.harvestValue * (plantLevel + 1);
                MoneyManager.AddMoney(harvestValue);
                MoneyManager.SpawnUIAboveField(transform, $"+R{harvestValue}");
                Debug.Log($"Harvested level {plantLevel} crop for R{harvestValue}!");
            }
            isAlive = false;
            Destroy(this.gameObject);
        }
    }

    void OnDestroy()
    {
        StopWatering();

        // Stop all coroutines when destroyed
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        if (damageEffectCoroutine != null)
        {
            StopCoroutine(damageEffectCoroutine);
        }
        if (smoothFillCoroutine != null)
        {
            StopCoroutine(smoothFillCoroutine);
        }
    }
}