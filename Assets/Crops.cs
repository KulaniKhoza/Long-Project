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
    public int moneyAmount = 5;
    private bool isAlive = true;

    [Header("Progress Bar")]
    public Image waterProgressBar;
    public TextMeshProUGUI waterProgressText;

    [Header("Selection Settings")]
    private bool isSelected = false;
    private Color originalColor;
    public Color selectedColor = Color.blue;
    private Coroutine flashCoroutine;

    void Start()
    {
        // Initialize components
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (MoneyManager == null)
            MoneyManager = GameManager.Instance;

        // Store original color
        originalColor = spriteRenderer.color;

        // Initialize progress bar
        InitializeProgressBar();
        UpdateSprite();
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
        // Don't allow watering if at max level and fully watered
        if (isSelected && Keyboard.current.wKey.isPressed && !isWatering && !(isMaxLevel && waterLevel >= maxWater))
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

        UpdateProgressBar();

        // Level up check - only if not at max level
        if (!isMaxLevel && plantLevel < 3 && waterLevel >= maxWater)
        {
            LevelUp();
        }

        GenerateMoney();
    }

    void GenerateMoney()
    {
        // Only generate money if at max level and fully watered
        if (isMaxLevel && waterLevel >= maxWater)
        {
            moneyTimer += Time.deltaTime;

            if (moneyTimer >= moneyGenerationInterval)
            {
                int moneyToAdd = moneyAmount;

                if (MoneyManager != null)
                {
                    MoneyManager.Money += moneyToAdd;
                    MoneyManager.SpawnUIAboveField(transform, $"+R{moneyToAdd}");
                }

                moneyTimer = 0f;
            }
        }
    }

    void HandleContinuousWatering()
    {
        if (isWatering && !(isMaxLevel && waterLevel >= maxWater))
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
        if (waterProgressBar != null)
        {
            float progress = (float)waterLevel / maxWater;
            waterProgressBar.fillAmount = progress;
        }

        if (waterProgressText != null)
        {
            // If at max level and fully watered, show a special message or just the full status
            if (isMaxLevel && waterLevel >= maxWater)
            {
                waterProgressText.text = $"MAX";
            }
            else
            {
                waterProgressText.text = $"{waterLevel}/{maxWater}";
            }
        }
    }

    public void Watering(int amount)
    {
        // Don't allow watering if at max level and already fully watered
        if (isMaxLevel && waterLevel >= maxWater) return;

        int oldWaterLevel = waterLevel;
        waterLevel = Mathf.Min(waterLevel + amount, maxWater);

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

    public void StartWatering()
    {
        // Don't allow watering if at max level and already fully watered
        if (isMaxLevel && waterLevel >= maxWater) return;

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
                Debug.Log($"Crop reached MAX level {plantLevel}! Water level maintained at maximum.");
            }
            else
            {
                // Reset water level for non-final levels
                waterLevel = 0;
            }

            // Reset progress bar for new level (will show full for max level)
            UpdateProgressBar();

            // Restore selection state after level up
            if (wasSelected)
            {
                RestartFlashing();
            }

            Debug.Log($"Crop leveled up to level {plantLevel}!");
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

    // Selection methods with FLASHING EFFECT
    public void SelectCrop()
    {
        if (isSelected) return;

        isSelected = true;

        // Stop any existing flash coroutine
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        // Start flashing
        flashCoroutine = StartCoroutine(FlashCrop());
        Debug.Log("Crop selected for watering");
    }

    public void DeselectCrop()
    {
        isSelected = false;

        // Stop flashing
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

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

    void HarvestCrop()
    {
        if (GameManager.Instance != null && GameManager.Instance.Money >= 0)
        {
            if (cropData != null)
            {
                int harvestValue = cropData.harvestValue * (plantLevel + 1);
                MoneyManager.Money += harvestValue;
                Debug.Log($"Harvested level {plantLevel} crop for ${harvestValue}!");
            }
            isAlive = false;
            Destroy(this.gameObject);
        }
    }

    void OnDestroy()
    {
        StopWatering();

        // Stop flashing coroutine when destroyed
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
    }
}