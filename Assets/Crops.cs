using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Crops : MonoBehaviour
{
    public GameManager MoneyManager;
    public CropData cropData;
    public SpriteRenderer spriteRenderer;
    public Sprite[] levelSprites;

    // Crop state variables
    public int plantLevel = 0;
    public int waterLevel = 0;
    public int maxWater = 100;

    // Progress Bar References
    public Image waterProgressBar;
    public CanvasGroup progressBarCanvas;

    // Watering variables
    private bool isWatering = false;
    private float wateringTimer = 0f;
    public float wateringInterval = 0.1f; // Time between each watering increment
    public int baseWaterAmount = 10; // Base water per click
    public int holdWaterAmount = 5; // Water amount per interval when holding

    void Start()
    {
        // Get references if not assigned
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (MoneyManager == null)
            MoneyManager = GameManager.Instance;

        // Set initial sprite based on current level
        UpdateSprite();

        // Hide progress bar initially
        if (progressBarCanvas != null)
        {
            progressBarCanvas.alpha = 0f;
        }
    }

    void Update()
    {
        // Handle continuous watering when holding right mouse button
        HandleContinuousWatering();

        // Update progress bar
        UpdateProgressBar();

        // Constantly check if we need to level up
        if (plantLevel < 3 && waterLevel >= maxWater)
        {
            LevelUp();
        }
    }

    void HandleContinuousWatering()
    {
        if (isWatering && plantLevel < 3)
        {
            wateringTimer += Time.deltaTime;

            // Add water at regular intervals while holding
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
            // Calculate fill amount
            float fillAmount = (float)waterLevel / maxWater;
            waterProgressBar.fillAmount = fillAmount;

            // Show/hide progress bar based on water needs
            if (progressBarCanvas != null)
            {
                bool showBar = plantLevel < 3 && waterLevel < maxWater;
                progressBarCanvas.alpha = showBar ? 1f : 0f;
            }
        }
    }

    public void Watering(int amount)
    {
        if (plantLevel >= 3) return;

        int previousWaterLevel = waterLevel;
        waterLevel = Mathf.Min(waterLevel + amount, maxWater);

        Debug.Log($"Watered crop. Water level: {waterLevel}/{maxWater}");

        // Show progress bar when watered
        if (progressBarCanvas != null)
        {
            progressBarCanvas.alpha = 1f;
        }
    }

    void LevelUp()
    {
        if (plantLevel >= 0 && plantLevel < 3)
        {
            plantLevel++;
            UpdateSprite();
            waterLevel = 0;

            Debug.Log($"Crop leveled up to level {plantLevel}!");
        }
    }

    void UpdateSprite()
    {
        if (levelSprites != null && levelSprites.Length > 0 &&
            plantLevel >= 0 && plantLevel < levelSprites.Length)
        {
            spriteRenderer.sprite = levelSprites[plantLevel];
            Debug.Log($"Updated sprite to level {plantLevel}");
        }
    }

    void LateUpdate()
    {
        HandleRightClickInput();
    }

    void HandleRightClickInput()
    {
        if (Mouse.current != null)
        {
            // Check for right mouse button press
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                if (IsMouseOverCrop() && FarmGrid.instance != null && FarmGrid.instance.Sow == false)
                {
                    // Start continuous watering
                    isWatering = true;
                    wateringTimer = 0f;

                    // Add base water amount on initial click
                    Watering(baseWaterAmount);
                }
            }

            // Check for right mouse button release
            if (Mouse.current.rightButton.wasReleasedThisFrame)
            {
                // Stop continuous watering
                isWatering = false;
            }

            // Handle harvesting with a different key (e.g., 'H' key or middle mouse)
            if (Keyboard.current.hKey.wasPressedThisFrame) // Or use middle mouse: Mouse.current.middleButton.wasPressedThisFrame
            {
                if (IsMouseOverCrop() && FarmGrid.instance != null && FarmGrid.instance.Sow == false)
                {
                    HarvestCrop();
                }
            }
        }
    }

    bool IsMouseOverCrop()
    {
        // Simple alternative using Unity's built-in mouse functions (if available)
        Vector2 mousePos2D = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        int layerMask = LayerMask.GetMask("CropsLayer");
        RaycastHit2D hit2D = Physics2D.Raycast(mousePos2D, Vector2.zero, Mathf.Infinity, layerMask);

        return hit2D.collider != null && hit2D.collider.CompareTag("Crops") && hit2D.collider.gameObject == this.gameObject;
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
            Destroy(this.gameObject);
        }
    }

    // Optional: Show progress bar on mouse hover
    void OnMouseEnter()
    {
        if (progressBarCanvas != null && plantLevel < 3)
        {
            progressBarCanvas.alpha = 1f;
        }
    }

    void OnMouseExit()
    {
        if (progressBarCanvas != null && waterLevel < maxWater)
        {
            progressBarCanvas.alpha = (waterLevel < maxWater) ? 1f : 0f;
        }

        // Stop watering if mouse leaves the crop
        isWatering = false;
    }
}