using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Crops : MonoBehaviour
{
    public GameManager MoneyManager;
    public CropData cropData;
    public SpriteRenderer spriteRenderer;
    public Sprite[] levelSprites;

    void Start()
    {
        // Get references if not assigned
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (MoneyManager == null)
            MoneyManager = GameManager.Instance;

        // Set initial sprite based on current level
        UpdateSprite();
    }

    void Update()
    {
        // Constantly check if we need to level up (only if not fully grown)
        if (cropData != null && cropData.plantlevel < 3 && cropData.waterlevel >= cropData.maxwater)
        {
            LevelUp();
        }
    }

    public void Watering(int amount)
    {
        if (cropData == null || cropData.plantlevel >= 3) return;

        cropData.waterlevel += amount;
        // LevelUp will be handled automatically in Update()
    }

    void LevelUp()
    {
        if (cropData.plantlevel >= 0 && cropData.plantlevel < 3)
        {
            cropData.plantlevel++;
            UpdateSprite();
            cropData.waterlevel = 0; // Reset water level after leveling up

            Debug.Log($"Crop leveled up to level {cropData.plantlevel}!");
        }
    }

    void UpdateSprite()
    {
        if (levelSprites != null && levelSprites.Length > 0 &&
            cropData.plantlevel >= 0 && cropData.plantlevel < levelSprites.Length)
        {
            spriteRenderer.sprite = levelSprites[cropData.plantlevel];
        }
    }

    void LateUpdate()
    {
        // Right-click harvesting
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (FarmGrid.instance != null && FarmGrid.instance.Sow == false)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

                int layerMask = LayerMask.GetMask("CropsLayer");
                RaycastHit2D hit2D = Physics2D.Raycast(mousePos2D, Vector2.zero, Mathf.Infinity, layerMask);

                if (hit2D.collider != null && hit2D.collider.CompareTag("Crops") &&
                    GameManager.Instance != null && GameManager.Instance.Money >= 0)
                {
                    if (cropData != null)
                    {
                        // Higher level crops give more money
                        int harvestValue = cropData.harvestValue * (cropData.plantlevel + 1);
                        MoneyManager.Money += harvestValue;
                        Debug.Log($"Harvested level {cropData.plantlevel} crop for ${harvestValue}!");
                    }
                    Destroy(this.gameObject);
                }
            }
        }
    }
}