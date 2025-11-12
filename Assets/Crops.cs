using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

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
    public int level1cash = 3; 
    public int level2cash = 5;
    public int level3cash = 7;
    [Header("Watering Settings")]
    private bool isWatering = false;
    private float wateringTimer = 0f;
    public float wateringInterval = 0.2f;
    public int holdWaterAmount = 8;
    
    // ? WATER DECAY SYSTEM
    [Header("Water Decay Settings")]
    public float decayDelay = 7f;
    public float decayRate = 10f;
    private bool isDecaying = false;
    private float decayTimer = 0f;

    // ? NEW — detect level-up interruption
    private bool levelUpJustHappened = false;

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
    public float smoothFillDuration = 5f;
    public float smoothDecreaseDuration = 14f; // ? NEW - Duration for smooth decrease
    public float decreaseStartDelay = 2f; // ? NEW - Delay before decrease starts
    private Coroutine smoothFillCoroutine;
    private Coroutine smoothDecreaseCoroutine; // ? NEW - Coroutine for smooth decrease
    private bool isAnimating = false;
    public GameObject WaterPanel;

    [Header("Selection Settings")]
    private bool isSelected = false;
    private Color originalColor;
    public Color selectedColor = Color.blue;
    private Coroutine flashCoroutine;

    // ? NEW - Clickable state (collider remains enabled)
    private bool isClickable = true;

    public static Crops CurrentlySelectedCrop { get; private set; }

    void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (MoneyManager == null)
            MoneyManager = GameManager.Instance;

        originalColor = spriteRenderer.color;
        currentHealth = maxHealth;

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

        // Watering logic
        if (isSelected && Keyboard.current.wKey.isPressed && !isWatering && !(isMaxLevel && waterLevel >= maxWater) && !isAnimating)
        {
            StartWatering();
        }
        else if ((!isSelected || !Keyboard.current.wKey.isPressed) && isWatering)
        {
            StopWatering();
        }

        if (isWatering)
        {
            HandleContinuousWatering();
        }

        GenerateMoney();

        // ? DECAY LOGIC ALWAYS CHECKS LAST
        HandleWaterDecay();

        wasDamagedThisFrame = false;
    }

    // ? IMPROVED DECAY SYSTEM
    void HandleWaterDecay()
    {
        if (isWatering || isAnimating) return;

        // ? Reset decay if leveling up happened
        if (levelUpJustHappened)
        {
            levelUpJustHappened = false;
            isDecaying = false;
            decayTimer = 0f;
            return;
        }

        // ? Don't decay if we're at max level and full water
        if (isMaxLevel && waterLevel >= maxWater)
        {
            isDecaying = false;
            decayTimer = 0f;
            return;
        }

        // ? Start decay countdown when full (only for non-max levels)
        if (!isDecaying && waterLevel >= maxWater && !isMaxLevel && smoothDecreaseCoroutine == null)
        {
            decayTimer += Time.deltaTime;

            if (decayTimer >= decayDelay)
            {
                // ? Start smooth decrease instead of instant decay
                StartSmoothDecreaseToMinimum();
                decayTimer = 0f;
            }
        }

        // ? Active decay (only when not doing smooth decrease)
        if (isDecaying && !isMaxLevel && smoothDecreaseCoroutine == null)
        {
            waterLevel -= Mathf.RoundToInt(decayRate * Time.deltaTime);

            if (waterLevel <= 0)
            {
                waterLevel = 0;
                isDecaying = false;
                Debug.Log("Water fully decayed");
            }

            UpdateProgressBar();
        }
    }

    void GenerateMoney()
    {
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

                    Debug.Log($"Money generated: +R{moneyToAdd} at level {plantLevel}. Total: {MoneyManager.Money}");
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
            case 1: return level1cash;
            case 2: return level2cash;
            case 3: return level3cash;
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
            if (isMaxLevel && waterLevel >= maxWater)
                waterProgressText.text = $"MAX";
            else
                waterProgressText.text = $"{waterLevel}/{maxWater}";
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

        // ? Start smooth decrease when reaching max water (if not already decreasing)
        if (waterLevel >= maxWater && !isMaxLevel && smoothDecreaseCoroutine == null)
        {
            decayTimer = 0f; // Reset decay timer
            isDecaying = false;
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

        // ? NEW - Stop flashing and deselect when watering starts
        StopFlashingAndDeselect();

        StartCoroutine(SmoothFillToMax());
        Debug.Log($"Started smooth water fill animation!");
    }

    // ? NEW - Stop flashing and deselect crop
    private void StopFlashingAndDeselect()
    {
        // Stop flashing
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        // Reset color
        spriteRenderer.color = originalColor;

        // Deselect crop
        if (isSelected)
        {
            isSelected = false;
            if (CurrentlySelectedCrop == this)
            {
                CurrentlySelectedCrop = null;
            }
            WaterPanel.SetActive(false);
        }

        // Make unclickable (but keep collider enabled)
        SetClickable(false);

        Debug.Log("Crop deselected and made unclickable for watering");
    }

    // ? NEW - Make crop clickable or unclickable (without removing collider)
    private void SetClickable(bool clickable)
    {
        isClickable = clickable;
        // Collider remains enabled - we handle clickability through state checking
    }

    // ? NEW - Public method to check if crop can be interacted with
    public bool CanBeClicked()
    {
        return isClickable && isAlive && waterLevel == 0 && !isAnimating;
    }

    // ? NEW - Smooth decrease to minimum
    public void StartSmoothDecreaseToMinimum()
    {
        if (isAnimating || smoothDecreaseCoroutine != null)
        {
            Debug.Log("Water decrease animation already in progress!");
            return;
        }

        if (isMaxLevel)
        {
            Debug.Log("Max level crop doesn't need to decrease water!");
            return;
        }

        smoothDecreaseCoroutine = StartCoroutine(SmoothDecreaseToZero());
        Debug.Log($"Started smooth water decrease animation!");
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
            waterProgressText.text = $"{maxWater}/{maxWater}";
        }

        isAnimating = false;

        // ? After filling to max, wait a bit then start the smooth decrease
        if (!isMaxLevel && plantLevel < 3)
        {
            StartCoroutine(DelayedSmoothDecrease());
        }
        else
        {
            // ? NEW - Make clickable again if at max level
            SetClickable(true);
        }

        Debug.Log("Smooth water fill animation completed!");
    }

    // ? NEW - Wait before starting decrease
    private IEnumerator DelayedSmoothDecrease()
    {
        Debug.Log($"Waiting {decreaseStartDelay} seconds before starting smooth decrease...");

        yield return new WaitForSeconds(decreaseStartDelay);

        StartSmoothDecreaseToMinimum();
    }

    // ? NEW - Smooth decrease coroutine
    private IEnumerator SmoothDecreaseToZero()
    {
        isAnimating = true;

        float startFill = (float)waterLevel / maxWater;
        float targetFill = 0f;
        float elapsedTime = 0f;
        int startWaterLevel = waterLevel;

        while (elapsedTime < smoothDecreaseDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / smoothDecreaseDuration;

            float currentFill = Mathf.Lerp(startFill, targetFill, progress);

            if (waterProgressBar != null)
            {
                waterProgressBar.fillAmount = currentFill;
            }

            int currentWaterValue = Mathf.RoundToInt(Mathf.Lerp(startWaterLevel, 0, progress));

            if (waterProgressText != null)
            {
                waterProgressText.text = $"{currentWaterValue}/{maxWater}";
            }

            yield return null;
        }

        waterLevel = 0;

        if (waterProgressBar != null)
        {
            waterProgressBar.fillAmount = 0f;
        }

        if (waterProgressText != null)
        {
            waterProgressText.text = $"0/{maxWater}";
        }

        isAnimating = false;
        smoothDecreaseCoroutine = null;

        // ? Level up when water reaches zero
        if (!isMaxLevel && plantLevel < 3)
        {
            LevelUp();
        }
        else
        {
            // ? NEW - If not leveling up (like during normal decay), flash to indicate ready
            FlashReadyForWatering();
        }

        Debug.Log("Smooth water decrease animation completed! Plant leveled up.");
    }

    // ? NEW - Flash once to indicate crop is ready for watering
    private void FlashReadyForWatering()
    {
        // Make crop clickable again
        SetClickable(true);

        // Start one-time flash
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashOnce());
        Debug.Log("Crop ready for watering - flashing indicator");
    }

    // ? NEW - Single flash coroutine (not continuous)
    private IEnumerator FlashOnce()
    {
        // Flash on
        spriteRenderer.color = selectedColor;
        yield return new WaitForSeconds(0.3f);

        // Flash off
        spriteRenderer.color = originalColor;
        yield return new WaitForSeconds(0.3f);

        // Flash on again
        spriteRenderer.color = selectedColor;
        yield return new WaitForSeconds(0.3f);

        // Return to normal
        spriteRenderer.color = originalColor;

        flashCoroutine = null;
        Debug.Log("Ready flash completed");
    }

    public bool CanBeWatered()
    {
        return waterLevel == 0 && !isAnimating && isClickable;
    }

    public bool IsMaxLevel()
    {
        return isMaxLevel && waterLevel >= maxWater;
    }

    public static void WaterSelectedCrop()
    {
        if (CurrentlySelectedCrop != null && CurrentlySelectedCrop.CanBeWatered())
        {
            CurrentlySelectedCrop.StartSmoothWaterFill();
        }
        else
        {
            Debug.Log("No crop selected to water or crop cannot be watered!");
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
            // ? NEW — tell decay system to restart
            levelUpJustHappened = true;

            bool wasSelected = isSelected;

            plantLevel++;
            UpdateSprite();

            if (plantLevel >= 3)
            {
                isMaxLevel = true;
                waterLevel = maxWater;
                UpdateProgressBar();
                Debug.Log($"Crop reached MAX level {plantLevel}! Water stays full.");
            }
            else
            {
                // ? Reset water level to 0 since we just decreased it
                waterLevel = 0;
                UpdateProgressBar();

                // ? Reset decay timer so decay doesn't start immediately
                isDecaying = false;
                decayTimer = 0f;
            }

            // ? NEW - Make crop clickable again after leveling up and flash to indicate ready
            SetClickable(true);
            FlashReadyForWatering();

            if (wasSelected)
            {
                // Don't restart flashing automatically - wait for user to click again
                // RestartFlashing();
            }

            Debug.Log($"Crop leveled up to level {plantLevel}! Water level reset to {waterLevel}/{maxWater}");
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
        // ? NEW - Check if crop is clickable before selecting
        if (!CanBeClicked() || isSelected) return;

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
        if (isSelected && isClickable)
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
        while (isSelected && isClickable)
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

            spriteRenderer.color = isSelected ? selectedColor : originalColor;
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
        if (smoothDecreaseCoroutine != null) // ? NEW - Stop decrease coroutine
            StopCoroutine(smoothDecreaseCoroutine);

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
        if (smoothDecreaseCoroutine != null) // ? NEW - Stop decrease coroutine
        {
            StopCoroutine(smoothDecreaseCoroutine);
        }
    }

    // ? NEW - Optional: Visual indicator when crop is unclickable
    void OnMouseEnter()
    {
        if (!CanBeClicked() && !isSelected)
        {
            // Optional: Change cursor or show tooltip to indicate crop is busy
        }
    }

    void OnMouseExit()
    {
        // Optional: Reset cursor
    }
}