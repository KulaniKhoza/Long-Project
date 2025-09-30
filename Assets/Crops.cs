using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class Crops : MonoBehaviour
{
    // Your existing crop variables...
    public GameManager MoneyManager;
    public CropData cropData;
    public SpriteRenderer spriteRenderer;
    public Sprite[] levelSprites;

    // Crop state variables
    public int plantLevel = 0;
    public int waterLevel = 0;
    public int maxWater = 100;

    // Watering variables
    private bool isWatering = false;
    private float wateringTimer = 0f;
    public float wateringInterval = 0.1f;
    public int holdWaterAmount = 5;

    // Fixed money generation timer
    private float moneyTimer = 0f;
    public float moneyGenerationInterval = 4f; // Fixed 4 seconds
    public int moneyAmount = 5; // Fixed R5
    private bool isAlive = true;

    // Progress Bar
    private SimpleProgressBar waterProgressBar;

    // Camera reference
    private Camera mainCamera;

    void Start()
    {
        // Your existing Start code...
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (MoneyManager == null)
            MoneyManager = GameManager.Instance;

        mainCamera = Camera.main;
        UpdateSprite();
    }

    void Update()
    {
        if (!isAlive) return;

        // Your existing Update code...
        if (isWatering)
        {
            HandleContinuousWatering();
        }

        UpdateProgressBar();

        if (plantLevel < 3 && waterLevel >= maxWater)
        {
            LevelUp();
        }

        // Individual crop money generation - FIXED TIMER & AMOUNT
        GenerateMoney();
    }

    void GenerateMoney()
    {
        moneyTimer += Time.deltaTime;

        if (moneyTimer >= moneyGenerationInterval)
        {
            // Fixed money amount - always R5
            int moneyToAdd = moneyAmount;

            // Add money to manager
            if (MoneyManager != null)
            {
                MoneyManager.Money += moneyToAdd;

                // Show floating text using existing system
                MoneyManager.SpawnUIAboveField(transform, $"+R{moneyToAdd}");

                Debug.Log($"Crop generated R{moneyToAdd}! Total: {MoneyManager.Money}");
            }

            // Reset timer
            moneyTimer = 0f;
        }
    }

    void HandleContinuousWatering()
    {
        if (isWatering && plantLevel < 3)
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
            waterProgressBar.SetProgress(progress, $"{waterLevel}/{maxWater}");
        }
    }

    public void Watering(int amount)
    {
        if (plantLevel >= 3) return;

        waterLevel = Mathf.Min(waterLevel + amount, maxWater);
        Debug.Log($"Watering crop. Water level: {waterLevel}/{maxWater}");

        // Create progress bar if it doesn't exist
        if (waterProgressBar == null)
        {
            waterProgressBar = GameManager.Instance.SpawnProgressBar(transform);
        }

        // Update progress bar
        if (waterProgressBar != null)
        {
            float progress = (float)waterLevel / maxWater;
            waterProgressBar.SetProgress(progress, $"{waterLevel}/{maxWater}");
        }
    }

    public void ToggleWatering()
    {
        if (plantLevel >= 3) return;

        isWatering = !isWatering;

        if (isWatering)
        {
            Debug.Log("Started watering crop");
        }
        else
        {
            Debug.Log("Stopped watering crop");
        }
    }

    void LevelUp()
    {
        if (plantLevel >= 0 && plantLevel < 3)
        {
            plantLevel++;
            UpdateSprite();
            waterLevel = 0;

            // Remove progress bar when leveling up
            if (waterProgressBar != null)
            {
                Destroy(waterProgressBar.gameObject);
                waterProgressBar = null;
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
            Debug.Log($"Updated sprite to level {plantLevel}");
        }
    }

    void LateUpdate()
    {
        HandleWateringInput();
    }

    void HandleWateringInput()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (IsMouseOverCrop())
            {
                ToggleWatering();
            }
        }
    }

    bool IsMouseOverCrop()
    {
        if (mainCamera == null) return false;

        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, mainCamera.nearClipPlane));
        Vector2 mousePos2D = new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);

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
            isAlive = false;
            Destroy(this.gameObject);
        }
    }

    void OnDestroy()
    {
        // Clean up progress bar
        if (waterProgressBar != null)
        {
            Destroy(waterProgressBar.gameObject);
        }
    }
}