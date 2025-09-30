using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Money and UI
    public int Money = 100;
    public TextMeshProUGUI MoneyText;

    // Seed inventory
    public int tomatoSeeds = 1;
    public int cornSeeds = 1;
    public int seeds = 1;

    // Singleton instance
    public static GameManager Instance;
    public CropData cropData;

    // UI references
    public GameObject uiPrefab;
    public GameObject progressBarPrefab;
    public Transform parentCanvas;

    // Crop money generation system
    [Header("Crop Money Generation")]
    public float moneyGenerationInterval = 4f;
    public int baseMoneyAmount = 5;
    private List<Crops> activeCrops = new List<Crops>();
    private float moneyTimer = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        Time.timeScale = 1.0f;
    }

    void Update()
    {
        MoneyText.text = "R" + Money;

        // Generate money from active crops
        GenerateCropMoney();
    }

    void GenerateCropMoney()
    {
        moneyTimer += Time.deltaTime;

        if (moneyTimer >= moneyGenerationInterval && activeCrops.Count > 0)
        {
            moneyTimer = 0f;

            // Generate money for each active crop
            foreach (Crops crop in activeCrops)
            {
                if (crop != null && crop.gameObject.activeInHierarchy)
                {
                    GenerateMoneyForCrop(crop);
                }
            }

            // Clean up null references
            activeCrops.RemoveAll(crop => crop == null);
        }
    }

    void GenerateMoneyForCrop(Crops crop)
    {
        // Generate random amount (3 to 7 money)
        int randomMoney = baseMoneyAmount + Random.Range(-2, 3);

        // Add to total money
        Money += randomMoney;

        // Use your existing SpawnUIAboveField method to show the money text
        SpawnUIAboveField(crop.transform, $"+R{randomMoney}");

        Debug.Log($"Crop generated R{randomMoney}! Total: R{Money}");
    }

    // Register crops for money generation
    public void RegisterCrop(Crops crop)
    {
        if (!activeCrops.Contains(crop))
        {
            activeCrops.Add(crop);
            Debug.Log($"Registered crop for money generation. Total crops: {activeCrops.Count}");
        }
    }

    // Unregister crops when they're destroyed
    public void UnregisterCrop(Crops crop)
    {
        if (activeCrops.Contains(crop))
        {
            activeCrops.Remove(crop);
            Debug.Log($"Unregistered crop. Total crops: {activeCrops.Count}");
        }
    }

    public void SpawnUIAboveField(Transform field, string textToShow)
    {
        if (uiPrefab == null || parentCanvas == null) return;

        // Convert world to screen space
        Vector3 screenPos = Camera.main.WorldToScreenPoint(field.position + new Vector3(1.4f, 0.7f, 0));

        // Instantiate under Canvas
        GameObject newUIElement = Instantiate(uiPrefab, parentCanvas, false);

        // Force position in screen space
        newUIElement.transform.position = screenPos;

        // If prefab has TMP text, set it
        var tmp = newUIElement.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = textToShow;
            tmp.color = Color.green; // Changed to green for money
        }

        // Force Unity to refresh UI immediately
        Canvas.ForceUpdateCanvases();
    }
    public SimpleProgressBar SpawnProgressBar(Transform target)
    {
        if (progressBarPrefab == null || parentCanvas == null) return null;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position + Vector3.up * 1.5f);
        GameObject progressBar = Instantiate(progressBarPrefab, parentCanvas, false);
        progressBar.transform.position = screenPos;

        return progressBar.GetComponent<SimpleProgressBar>();
    }

}