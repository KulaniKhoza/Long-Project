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
    public int maxHealth = 4;
    public int currentHealth;
    private Coroutine damageEffectCoroutine;
    private bool wasDamagedThisFrame = false;

    [Header("Progress Bar")]
    public Image waterProgressBar;
    public TextMeshProUGUI waterProgressText;
    public float smoothFillDuration = 2f;
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
        wasDamagedThisFrame = false;
    }

    void GenerateMoney()
    {
        // Generate money at EVERY level when fully watered
        if (plantLevel >= 1)
        {
            moneyTimer += Time.deltaTime;

            if (moneyTimer >= moneyGenerationInterval)
            {
                int moneyToAdd = CalculateMoneyAmount();

                if (MoneyManager != null)
                {
                    MoneyManager.AddMoney(moneyToAdd);
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

    private int CalculateMoneyAmount()
    {
        switch (plantLevel)
        {
            case 1: return 5;
            case 2: return 7;
            case 3: return 9;
            default: return 0;
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
        if (isMaxLevel && waterLevel >= maxWater || isAnimating) return;

        int oldWaterLevel = waterLevel;
        waterLevel = Mathf.Min(waterLevel + amount, maxWater);

        UpdateProgressBar();

        if (waterLevel / 20 != oldWaterLevel / 20)
        {
            Debug.Log($"Watering crop. Water level: {waterLevel}/{maxWater}");
        }

        if (isMaxLevel && waterLevel >= maxWater)
        {
            waterLevel = maxWater;
            UpdateProgressBar();
        }
    }

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

        StartCoroutine(SmoothFillToMax());
        Debug.Log($"Started smooth water fill animation!");
    }

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

            float currentFill = Mathf.Lerp(startFill, targetFill, progress);

            if (waterProgressBar != null)
            {
                waterProgressBar.fillAmount = currentFill;
            }

            int currentWaterValue = Mathf.RoundToInt(Mathf.Lerp(startWaterLevel, maxWater, progress));

            if (waterProgressText != null)
            {
                waterProgressText.text = $"{currentWaterValue}/{maxWater}";
            }

            yield return null;
        }

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

        if (!isMaxLevel && plantLevel < 3)
        {
            LevelUp();
        }

        Debug.Log("Smooth water fill animation completed!");
    }

    public bool CanBeWatered()
    {
        return !isMaxLevel &&
               waterLevel < maxWater &&
               !isAnimating;
    }

    public bool IsMaxLevel()
    {
        return isMaxLevel && waterLevel >= maxWater;
    }

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

    public static bool CanWaterAnyCrop()
    {
        return CurrentlySelectedCrop != null && CurrentlySelectedCrop.CanBeWatered();
    }

    public void StartWatering()
    {
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
            bool wasSelected = isSelected;

            plantLevel++;
            UpdateSprite();

            if (plantLevel >= 3)
            {
                isMaxLevel = true;
                waterLevel = maxWater;
                UpdateProgressBar();
                Debug.Log($"Crop reached MAX level {plantLevel}! Water level maintained at maximum. Will generate R{CalculateMoneyAmount()} every {moneyGenerationInterval} seconds.");
            }
            else
            {
                waterLevel = 0;
                UpdateProgressBar();
            }

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

            if (!isSelected)
            {
                originalColor = spriteRenderer.color;
            }
        }
    }

    public void SelectCrop()
    {
        if (isSelected) return;

        if (CurrentlySelectedCrop != null && CurrentlySelectedCrop != this)
        {
            CurrentlySelectedCrop.DeselectCrop();
        }

        isSelected = true;
        CurrentlySelectedCrop = this;

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

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

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        WaterPanel.SetActive(false);
        spriteRenderer.color = originalColor;
        StopWatering();
        Debug.Log("Crop deselected");
    }

    private void RestartFlashing()
    {
        if (isSelected)
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            spriteRenderer.color = GetBaseSpriteColor();
            originalColor = spriteRenderer.color;

            flashCoroutine = StartCoroutine(FlashCrop());
            Debug.Log("Restarted flashing after level up");
        }
    }

    private Color GetBaseSpriteColor()
    {
        return Color.white;
    }

    private IEnumerator FlashCrop()
    {
        while (isSelected)
        {
            spriteRenderer.color = selectedColor;
            yield return new WaitForSeconds(0.3f);

            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.3f);
        }

        spriteRenderer.color = originalColor;
    }

    public void TakeDamage(int damage = 1)
    {
        if (!isAlive) return;

        wasDamagedThisFrame = true;

        currentHealth -= damage;
        Debug.Log($"Crop took {damage} damage! Health: {currentHealth}/{maxHealth}");

        PlayDamageEffect();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void PlayDamageEffect()
    {
        if (damageEffectCoroutine != null)
            StopCoroutine(damageEffectCoroutine);

        damageEffectCoroutine = StartCoroutine(DamageEffect());
    }

    private IEnumerator DamageEffect()
    {
        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);

            if (isSelected)
                spriteRenderer.color = selectedColor;
            else
                spriteRenderer.color = originalColor;

            yield return new WaitForSeconds(0.1f);
        }
    }

    public bool WasDamagedThisFrame()
    {
        return wasDamagedThisFrame;
    }

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

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        if (damageEffectCoroutine != null)
            StopCoroutine(damageEffectCoroutine);
        if (smoothFillCoroutine != null)
            StopCoroutine(smoothFillCoroutine);

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