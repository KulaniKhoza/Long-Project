using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

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

    [Header("Watering Settings")]
    private bool mouseOverCrop = false;
    private bool waterKeyHeld = false;

    // Camera reference
    private Camera mainCamera;

    void Start()
    {
        // Initialize components
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (MoneyManager == null)
            MoneyManager = GameManager.Instance;

        mainCamera = Camera.main;

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

        // Handle W key input using the same system as FarmGrid
        HandleWaterInput();

        // Handle continuous watering while water key is held AND mouse is over crop
        if (waterKeyHeld && mouseOverCrop && !isWatering)
        {
            StartWatering();
        }
        else if ((!waterKeyHeld || !mouseOverCrop) && isWatering)
        {
            StopWatering();
        }

        // Handle the actual watering process
        if (isWatering)
        {
            HandleContinuousWatering();
        }

        UpdateProgressBar();

        // Level up check
        if (plantLevel < 3 && waterLevel >= maxWater)
        {
            LevelUp();
        }

        GenerateMoney();
    }

    void HandleWaterInput()
    {
        // Check for W key using the same input system as FarmGrid
        if (Keyboard.current != null)
        {
            // W key pressed
            if (Keyboard.current.wKey.wasPressedThisFrame)
            {
                waterKeyHeld = true;

                // If W is pressed and mouse is over this crop, start watering
                if (mouseOverCrop && !isWatering)
                {
                    StartWatering();
                }
            }

            // W key released
            if (Keyboard.current.wKey.wasReleasedThisFrame)
            {
                waterKeyHeld = false;
            }
        }
    }

    void GenerateMoney()
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
            waterProgressBar.fillAmount = progress;
        }

        if (waterProgressText != null)
        {
            waterProgressText.text = $"{waterLevel}/{maxWater}";
        }
    }

    public void Watering(int amount)
    {
        if (plantLevel >= 3) return;

        int oldWaterLevel = waterLevel;
        waterLevel = Mathf.Min(waterLevel + amount, maxWater);

        // Visual feedback when water level changes significantly
        if (waterLevel / 20 != oldWaterLevel / 20)
        {
            Debug.Log($"Watering crop. Water level: {waterLevel}/{maxWater}");
        }

        // Visual feedback for watering
        ShowWateringEffect();
    }

    void ShowWateringEffect()
    {
        // Optional: Add particle effect or visual feedback here
        // Example: 
        // if (wateringParticles != null) wateringParticles.Play();
    }

    public void StartWatering()
    {
        if (plantLevel >= 3) return;

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
            plantLevel++;
            UpdateSprite();
            waterLevel = 0;

            // Reset progress bar for new level
            InitializeProgressBar();

            Debug.Log($"Crop leveled up to level {plantLevel}!");
        }
    }

    void UpdateSprite()
    {
        if (levelSprites != null && levelSprites.Length > 0 &&
            plantLevel >= 0 && plantLevel < levelSprites.Length)
        {
            spriteRenderer.sprite = levelSprites[plantLevel];
        }
    }

    // Mouse enter/exit events to detect when mouse is over crop
    void OnMouseEnter()
    {
        // Check if we're not over UI and in farming mode
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            mouseOverCrop = true;
            Debug.Log("Mouse entered crop");
        }
    }

    void OnMouseExit()
    {
        mouseOverCrop = false;
        Debug.Log("Mouse exited crop");
    }

   

    // Optional: Visual feedback when crop can be watered
    void OnMouseOver()
    {
        // Optional: Highlight crop when mouse is over and water key can be used
        if (plantLevel < 3)
        {
            // You could change color or show a tooltip here
        }
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
        // Clean up
        StopWatering();
    }
}