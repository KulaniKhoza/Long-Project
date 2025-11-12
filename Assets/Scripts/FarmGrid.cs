using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class FarmGrid : MonoBehaviour
{
    public GameObject grass;
    public int RowLength, ColumnLength;
    public float X_space, Y_space;
    public GameObject[] GameGrid;
    public bool gotgrid;

    public GameObject field;
    public bool Sow;
    public bool CreateField;
    public bool PlaceDefenders;
    public Texture2D basicCursor, SeedCursor, FieldCursor, DefenderCursor;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotspot = Vector2.zero;

    public enum SeedType { None, Normal, Tomato, Corn }
    public SeedType currentSeed = SeedType.None;
    public enum DefenderType { None, Archer, Mage, Fence }
    public DefenderType currentDefender = DefenderType.None;

    public GameObject normalSeedPrefab;
    public GameObject tomatoSeedPrefab;
    public GameObject cornSeedPrefab;
    public GameObject archerDefenderPrefab;
    public GameObject mageDefenderPrefab;
    public GameObject fenceDefenderPrefab;

    public GameObject gridOriginObject;
    public static FarmGrid instance;
    private Transform highlight;
    private Transform selection;
    private Transform fieldHighlight;
    private Crops selectedCrop;
    private int plowprice = 10;
    public TextMeshProUGUI textPrefab;

    public Color highlightColor = Color.red;
    public Color selectionColor = Color.magenta;
    public Color fieldHighlightColor = Color.blue;
    private Color defaultColor = Color.white;

    private int fieldLayerMask;
    private int cropsLayerMask;
    private int defenderLayerMask;
    private int gridLayerMask;

    public GameObject contextMenu;
    public GameObject plantMenu;
    private Vector3 lastGridPosition;
    private Vector3 lastFieldPosition;
    private bool contextMenuVisible = false;
    private bool plantMenuVisible = false;

    public Color activeFieldColor = Color.red;
    public Color inactiveGridColor = Color.blue;

    [System.Serializable]
    public class SeedButtonData
    {
        public Button button;
        public SeedType seedType;
        public GameObject seedPrefab;
    }

    public List<SeedButtonData> seedButtons = new List<SeedButtonData>();
    private bool plantMenuWasOpened = false;
    public int normalseedprice = 50;
    public int CornPrice = 100;
    public int TomatoPrice = 150;
    [System.Serializable]
    public class DefenderButtonData
    {
        public Button button;
        public DefenderType defenderType;
        public GameObject defenderPrefab;
    }

    public List<DefenderButtonData> defenderButtons = new List<DefenderButtonData>();
    public int FencePrice = 50;
    public int ChickenPrice = 80;
    public int PesticidePrice = 65;
    [Header("Buttons")]
    public List<UniversalButton> farmingButtons = new List<UniversalButton>();
    public List<UniversalButton> defendingButtons = new List<UniversalButton>();

    // SIMPLE TUTORIAL INTEGRATION
    private TutorialManager tutorialManager;
    private bool tutorialActive = true;
    TextScript Communicator;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        Time.timeScale = 1.0f;
        fieldLayerMask = LayerMask.GetMask("FieldLayer");
        cropsLayerMask = LayerMask.GetMask("CropsLayer");
        defenderLayerMask = LayerMask.GetMask("DefenderLayer");
        gridLayerMask = LayerMask.GetMask("Default");

        InitializeSeedButtons();
        InitializeDefenderButtons();
    }

    void Start()
    {
        Cursor.SetCursor(basicCursor, hotspot, cursorMode);
        Vector3 gridOrigin = gridOriginObject != null ? gridOriginObject.transform.position : Vector3.zero;

        for (int row = 0; row < RowLength; row++)
        {
            for (int col = 0; col < ColumnLength; col++)
            {
                Vector3 position = gridOrigin + new Vector3(col * X_space, row * Y_space, 0);
                Instantiate(grass, position, Quaternion.identity);
            }
        }

        if (contextMenu != null) contextMenu.SetActive(false);
        if (plantMenu != null) plantMenu.SetActive(false);

        DisableAllButtons();

        // Find simple tutorial manager
        tutorialManager = FindFirstObjectByType<TutorialManager>();
        if (tutorialManager == null)
        {
            Debug.LogWarning("SimpleTutorialManager not found in scene!");
            tutorialActive = false;
        }
        else
        {
            Debug.Log("Simple tutorial manager found successfully!");
        }
        Communicator = FindObjectOfType<TextScript>();
    }

    void LateUpdate()
    {
        if (!gotgrid)
        {
            GameGrid = GameObject.FindGameObjectsWithTag("grid");
            gotgrid = true;
        }
    }

    void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));

        RemoveHighlights();

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            HandleGridHighlight(worldPos);
            HandleFieldHighlight(worldPos);
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && !EventSystem.current.IsPointerOverGameObject())
        {
            bool clickedCrop = false;

            RaycastHit2D cropHit = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity, cropsLayerMask);
            if (cropHit.collider != null && cropHit.collider.CompareTag("Crops"))
            {
                Crops crop = cropHit.collider.GetComponent<Crops>();
                if (crop != null)
                {
                    clickedCrop = true;
                    if (selectedCrop != null && selectedCrop != crop)
                        selectedCrop.DeselectCrop();

                    selectedCrop = crop;
                    selectedCrop.SelectCrop();
                    CloseContextMenu();
                    ClosePlantMenu();
                    return;
                }
            }

            if (!clickedCrop)
            {
                if ((contextMenuVisible || plantMenuVisible) && highlight == null && fieldHighlight == null)
                {
                    CloseContextMenu();
                    ClosePlantMenu();
                }

                if (fieldHighlight != null && !plantMenuVisible)
                {
                    lastFieldPosition = fieldHighlight.position;
                    ShowPlantMenu();
                    CloseContextMenu();
                }
                else if (highlight != null)
                {
                    lastGridPosition = highlight.position;
                    HighlightClickedGridTile(lastGridPosition);
                    ShowContextMenu();
                    ClosePlantMenu();
                }
                else
                {
                    DeselectGrid();
                    CloseContextMenu();
                    ClosePlantMenu();
                }
            }

            if (Sow && !CreateField && !PlaceDefenders && !clickedCrop)
            {
                RaycastHit2D plantingFieldHit = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity, fieldLayerMask);
                if (plantingFieldHit.collider != null && plantingFieldHit.collider.CompareTag("field"))
                {
                    GameObject seedToPlant = null;
                    bool hasSeed = false;

                    switch (currentSeed)
                    {
                        case SeedType.Normal:
                            if (GameManager.Instance.seeds > 0)
                            {
                                seedToPlant = normalSeedPrefab;
                                GameManager.Instance.seeds--;
                                GameManager.Instance.SpawnUIAboveField(plantingFieldHit.collider.transform, "-1");
                                hasSeed = true;
                            }
                            break;
                        case SeedType.Tomato:
                            if (GameManager.Instance.tomatoSeeds > 0)
                            {
                                seedToPlant = tomatoSeedPrefab;
                                GameManager.Instance.tomatoSeeds--;
                                GameManager.Instance.SpawnUIAboveField(plantingFieldHit.collider.transform, "-1");
                                hasSeed = true;
                            }
                            break;
                        case SeedType.Corn:
                            if (GameManager.Instance.cornSeeds > 0)
                            {
                                seedToPlant = cornSeedPrefab;
                                GameManager.Instance.cornSeeds--;
                                GameManager.Instance.SpawnUIAboveField(plantingFieldHit.collider.transform, "-1");
                                hasSeed = true;
                            }
                            break;
                    }

                    if (hasSeed && seedToPlant != null)
                    {
                        Vector3 spawnPos = plantingFieldHit.collider.transform.position + new Vector3(0, 0.1f, 0);
                        GameObject newCrop = Instantiate(seedToPlant, spawnPos, Quaternion.identity);
                        newCrop.layer = LayerMask.NameToLayer("CropsLayer");
                        Debug.Log("Planted seed at: " + spawnPos);
                        Normal();
                    }
                    else
                    {
                        Debug.Log("Not enough seeds!");
                        if (Communicator != null && !Communicator.writingText)
                        {
                            Communicator.fullSentence = "Not enough seeds!";
                            Communicator.StartCoroutine(Communicator.ShowTextLetterByLetter());
                        }

                    }
                }
            }
        }

        if (Mouse.current.rightButton.wasPressedThisFrame && !EventSystem.current.IsPointerOverGameObject())
            DeselectAll();

        if (Keyboard.current.escapeKey.wasPressedThisFrame && plantMenuWasOpened)
        {
            ClosePlantMenu();
        }
    }

    void RemoveHighlights()
    {
        if (highlight != null && highlight != selection)
        {
            SpriteRenderer sr = highlight.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = defaultColor;
            highlight = null;
        }

        if (fieldHighlight != null)
        {
            SpriteRenderer sr = fieldHighlight.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = defaultColor;
            fieldHighlight = null;
        }
    }

    void HandleGridHighlight(Vector3 worldPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null && hit.collider.CompareTag("grid") && hit.collider.transform != selection)
        {
            highlight = hit.collider.transform;
            SpriteRenderer sr = highlight.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = highlightColor;
        }
    }

    void HandleFieldHighlight(Vector3 worldPos)
    {
        RaycastHit2D fieldHit = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity, fieldLayerMask);
        if (fieldHit.collider != null && fieldHit.collider.CompareTag("field"))
        {
            fieldHighlight = fieldHit.collider.transform;
            SpriteRenderer sr = fieldHighlight.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = fieldHighlightColor;
        }
    }

    public void HighlightClickedGridTile(Vector3 clickedPosition)
    {
        foreach (GameObject gridTile in GameGrid)
        {
            if (gridTile == null) continue;

            SpriteRenderer sr = gridTile.GetComponent<SpriteRenderer>();
            if (sr == null) continue;

            if (Vector3.Distance(gridTile.transform.position, clickedPosition) < 0.1f)
                sr.color = Color.white;
            else
                sr.color = Color.blue;
        }
    }

    void DeselectAll()
    {
        DeselectGrid();

        if (selectedCrop != null)
        {
            selectedCrop.DeselectCrop();
            selectedCrop = null;
        }

        CloseContextMenu();
        ClosePlantMenu();
        Normal();
    }

    void DeselectGrid()
    {
        if (selection != null)
        {
            SpriteRenderer selSR = selection.GetComponent<SpriteRenderer>();
            if (selSR != null) selSR.color = defaultColor;
            selection = null;
        }
    }

    void ShowContextMenu()
    {
        if (contextMenu != null)
        {
            contextMenu.transform.position = lastGridPosition;
            contextMenu.SetActive(true);
            contextMenuVisible = true;
            UpdateContextMenuButtons();
            ClosePlantMenu();
        }
    }

    void CloseContextMenu()
    {
        if (contextMenu != null)
        {
            contextMenu.SetActive(false);
            contextMenuVisible = false;
        }
    }

    void ShowPlantMenu()
    {
        if (plantMenu != null && !plantMenuVisible)
        {
            lastFieldPosition = fieldHighlight.position;
            plantMenu.transform.position = lastFieldPosition;
            plantMenu.SetActive(true);
            plantMenuVisible = true;
            EnableAllSeedButtons();
            CloseContextMenu();
        }
    }

    public void PlantButton()
    {
        EnableFarmingButtons();
        plantMenu.SetActive(false);
        plantMenuVisible = true;
        HighlightClickedGridTile(lastGridPosition);
        Debug.Log("Plant mode activated. Farming buttons enabled.");
    }

    public void OnDefendButtonClicked2()
    {
        if (!IsPositionOccupied(lastGridPosition))
        {
            int cheapestCost = Mathf.Min(GetDefenderCost(DefenderType.Archer),
                                       GetDefenderCost(DefenderType.Mage),
                                       GetDefenderCost(DefenderType.Fence));

            if (GameManager.Instance.Money >= cheapestCost)
            {
                PlaceDefenders = true;
                Sow = false;
                CreateField = false;
                currentSeed = SeedType.None;

                EnableDefenderButtons();
                HighlightClickedGridTile(lastGridPosition);
                CloseContextMenu();
                ClosePlantMenu();

                Debug.Log("Defender mode activated via context menu. Defender buttons enabled.");
            }
            else
            {
                Debug.Log($"Not enough money for defenders! Cheapest costs ${cheapestCost}");
                if (Communicator != null && !Communicator.writingText)
                {
                    Communicator.fullSentence = $"Not enough money for defenders! Cheapest costs ${cheapestCost}";
                    Communicator.StartCoroutine(Communicator.ShowTextLetterByLetter());
                }

            }
        }
        else
        {
            Debug.Log("Cannot place defender: position occupied!");
            if (Communicator != null && !Communicator.writingText)
            {
                Communicator.fullSentence = "Cannot place defender: position occupied!";
                Communicator.StartCoroutine(Communicator.ShowTextLetterByLetter());
            }

        }
    }

    public void HighlightFieldAndBlueGrid(Vector3 fieldPos)
    {
        Collider2D[] fieldHits = Physics2D.OverlapCircleAll(fieldPos, 0.3f);
        foreach (Collider2D col in fieldHits)
        {
            SpriteRenderer sr = col.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = activeFieldColor;
        }

        foreach (GameObject gridTile in GameGrid)
        {
            if (gridTile == null) continue;

            if (Vector3.Distance(gridTile.transform.position, fieldPos) < 0.1f)
                continue;

            SpriteRenderer sr = gridTile.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = inactiveGridColor;
        }
    }

    void ClosePlantMenu()
    {
        if (plantMenu != null)
        {
            plantMenu.SetActive(false);
            plantMenuVisible = false;
            DisableFarmButtons();
        }
    }

    void UpdateContextMenuButtons()
    {
        if (contextMenu == null) return;

        Button sowButton = contextMenu.transform.Find("SowButton")?.GetComponent<Button>();
        Button defendButton = contextMenu.transform.Find("DefendButton")?.GetComponent<Button>();

        bool isField = IsGridPositionField(lastGridPosition);

        if (sowButton != null)
        {
            sowButton.interactable = true;
            TextMeshProUGUI sowText = sowButton.GetComponentInChildren<TextMeshProUGUI>();
            if (sowText != null) sowText.text = isField ? "Sow Seeds" : "Plow Field";
        }

        if (defendButton != null)
        {
            bool positionOccupied = IsPositionOccupied(lastGridPosition);
            defendButton.interactable = !positionOccupied;
            TextMeshProUGUI defendText = defendButton.GetComponentInChildren<TextMeshProUGUI>();
            if (defendText != null) defendText.text = positionOccupied ? "Position Occupied" : "Place Defender";
        }
    }

    void InitializeSeedButtons()
    {
        foreach (SeedButtonData seedButton in seedButtons)
        {
            if (seedButton.button != null)
            {
                seedButton.button.interactable = false;
                seedButton.button.onClick.RemoveAllListeners();
                seedButton.button.onClick.AddListener(() => OnInventorySeedButtonClicked(seedButton));
            }
        }
    }

    void EnableAllSeedButtons()
    {
        UpdateSeedButtonVisuals();
        plantMenuWasOpened = true;
    }

    void UpdateSeedButtonVisuals()
    {
        foreach (SeedButtonData seedButton in seedButtons)
        {
            if (seedButton.button != null)
            {
                bool hasSeeds = HasSeedsForType(seedButton.seedType);
                seedButton.button.interactable = hasSeeds;

                TextMeshProUGUI buttonText = seedButton.button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    int seedCount = GetSeedCountForType(seedButton.seedType);
                    buttonText.text = $"{seedButton.seedType}\n({seedCount})";
                }

                Image buttonImage = seedButton.button.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = hasSeeds ? Color.white : Color.gray;
                }
            }
        }
    }

    public int GetSeedCost(SeedType seedType)
    {
        switch (seedType)
        {
            case SeedType.Normal: return normalseedprice;  // Normal seeds cost 1 seed
            case SeedType.Tomato: return TomatoPrice;  // Tomato seeds cost 1 seed
            case SeedType.Corn: return CornPrice;    // Corn seeds cost 1 seed
            default: return 0;
        }
    }

    int GetSeedCountForType(SeedType seedType)
    {
        switch (seedType)
        {
            case SeedType.Normal: return GameManager.Instance.seeds;
            case SeedType.Tomato: return GameManager.Instance.tomatoSeeds;
            case SeedType.Corn: return GameManager.Instance.cornSeeds;
            default: return 0;
        }
    }

    bool HasSeedsForType(SeedType seedType)
    {
        switch (seedType)
        {
            case SeedType.Normal: return GameManager.Instance.seeds > 0;
            case SeedType.Tomato: return GameManager.Instance.tomatoSeeds > 0;
            case SeedType.Corn: return GameManager.Instance.cornSeeds > 0;
            default: return false;
        }
    }

    public void OnInventorySeedButtonClicked(SeedButtonData seedButtonData)
    {
        if (!plantMenuVisible) return;

        if (!HasSeedsForType(seedButtonData.seedType))
        {
            Debug.Log("No seeds available!");
            EnableAllSeedButtons();
            if (Communicator != null && !Communicator.writingText)
            {
                Communicator.fullSentence = "No seeds available!";
                Communicator.StartCoroutine(Communicator.ShowTextLetterByLetter());
            }
            return;
        }
        int cost = GetSeedCost(seedButtonData.seedType);
        if (GameManager.Instance.Money >= cost)
        {
            switch (seedButtonData.seedType)
            {
                case SeedType.Normal:
                    if (GameManager.Instance.seeds > 0)
                    {
                        GameManager.Instance.Money -= cost;
                        GameManager.Instance.seeds++;

                        GameManager.Instance.SpawnUIAboveField(transform, $"-R{cost}");
                    }
                    break;
                case SeedType.Tomato:
                    if (GameManager.Instance.tomatoSeeds > 0)
                    {
                        GameManager.Instance.Money -= cost;
                        GameManager.Instance.tomatoSeeds++;
                        GameManager.Instance.SpawnUIAboveField(transform, $"-R{cost}");
                    }
                    break;
                case SeedType.Corn:
                    if (GameManager.Instance.cornSeeds > 0)
                    {
                        GameManager.Instance.Money -= cost;
                        GameManager.Instance.cornSeeds++;
                        GameManager.Instance.SpawnUIAboveField(transform, $"-R{cost}");
                    }
                    break;
            }
        }
        PlantSeedAtPosition(seedButtonData.seedType, seedButtonData.seedPrefab, lastFieldPosition);
        ClosePlantMenu();
    }

    void PlantSeedAtPosition(SeedType seedType, GameObject seedPrefab, Vector3 position)
    {
        if (seedPrefab == null) return;
        if (!IsPositionValidForPlanting(position)) return;

        Vector3 spawnPos = position + new Vector3(0, 0.1f, 0);
        GameObject newCrop = Instantiate(seedPrefab, spawnPos, Quaternion.identity);
        newCrop.layer = LayerMask.NameToLayer("CropsLayer");
        Debug.Log($"Planted {seedType} at: {spawnPos}");
        Normal();
    }

    bool IsPositionValidForPlanting(Vector3 position)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 0.3f);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("field"))
            {
                Collider2D[] cropColliders = Physics2D.OverlapCircleAll(position, 0.2f);
                foreach (Collider2D cropCollider in cropColliders)
                    if (cropCollider.CompareTag("Crops")) return false;

                return true;
            }
        }
        return false;
    }

    bool IsGridPositionField(Vector3 position)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 0.3f);
        foreach (Collider2D collider in colliders)
            if (collider.CompareTag("field")) return true;
        return false;
    }

    public void OnSowButtonClicked()
    {
        bool isField = IsGridPositionField(lastGridPosition);

        Debug.Log($"SowButton clicked - IsField: {isField}");

        if (isField)
        {
            Sowing();
            EnableFarmingButtons();
            CloseContextMenu();
        }
        else
        {
            CreateFieldAtSelection();
            CloseContextMenu();
        }
    }

    void CreateFieldAtSelection()
    {
        GameObject newField = Instantiate(field, lastGridPosition, Quaternion.identity);
        newField.layer = LayerMask.NameToLayer("FieldLayer");
        GameManager.Instance.Money -= plowprice - 5;
        GameManager.Instance.SpawnUIAboveField(newField.transform, "-5");
        Debug.Log("Created field at: " + lastGridPosition);
    }

    void PrepareDefenderWithPosition(DefenderType defenderType)
    {
        currentDefender = defenderType;
        Cursor.SetCursor(DefenderCursor, hotspot, cursorMode);
        PlaceDefenders = true;
        CreateField = false;
        Sow = false;
        currentSeed = SeedType.None;

        PlaceDefender(lastGridPosition);
        Normal();
    }

    Vector3 GetDefenderOffset(DefenderType defenderType)
    {
        switch (defenderType)
        {
            case DefenderType.Archer: return new Vector3(-0.2f, -0.1f, 0);
            case DefenderType.Mage: return new Vector3(-0.1f, -0.5f, 0);
            case DefenderType.Fence: return Vector3.zero;
            default: return Vector3.zero;
        }
    }

    void PlaceDefender(Vector3 position)
    {
        GameObject defenderToPlace = null;
        switch (currentDefender)
        {
            case DefenderType.Archer: defenderToPlace = archerDefenderPrefab; break;
            case DefenderType.Mage: defenderToPlace = mageDefenderPrefab; break;
            case DefenderType.Fence: defenderToPlace = fenceDefenderPrefab; break;
        }

        if (defenderToPlace != null)
        {
            Vector3 offset = GetDefenderOffset(currentDefender);
            Vector3 finalPosition = position + offset;

            if (!IsPositionOccupied(finalPosition))
            {
                GameObject newDefender = Instantiate(defenderToPlace, finalPosition, Quaternion.identity);
                newDefender.layer = LayerMask.NameToLayer("DefenderLayer");
                Debug.Log($"Placed {currentDefender} at {finalPosition}");
            }
            else
            {
                Debug.Log("Position already occupied!");
            }
        }
    }

    bool IsPositionOccupied(Vector3 position)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 0.4f);
        foreach (Collider2D collider in colliders)
            if (collider.CompareTag("Defender") || collider.CompareTag("Crops"))
                return true;
        return false;
    }

    public void OnDefenderTypeButtonClicked(DefenderButtonData defenderButtonData)
    {
        if (!PlaceDefenders) return;

        int cost = GetDefenderCost(defenderButtonData.defenderType);

        if (GameManager.Instance.Money >= cost)
        {
            GameManager.Instance.Money -= cost;
            GameManager.Instance.SpawnUIAboveField(transform, $"-R{cost}");

            currentDefender = defenderButtonData.defenderType;
            PlaceDefenderAtPosition(lastGridPosition, defenderButtonData.defenderPrefab);

            PlaceDefenders = false;
            currentDefender = DefenderType.None;
            Cursor.SetCursor(basicCursor, hotspot, cursorMode);
            DisableDefenderButtons();
            Normal();
        }
        else
        {
            Debug.Log($"Not enough money! Need {cost}, have {GameManager.Instance.Money}");
            if (Communicator != null && !Communicator.writingText)
            {
                Communicator.fullSentence = $"Not enough money! Need {cost}, have {GameManager.Instance.Money}";
                StartCoroutine(Communicator.ShowTextLetterByLetter());
            }
        }
    }

    private int GetDefenderCost(DefenderType defenderType)
    {
        switch (defenderType)
        {
            case DefenderType.Archer: return ChickenPrice;
            case DefenderType.Mage: return PesticidePrice;
            case DefenderType.Fence: return FencePrice;
            default: return 0;
        }
    }

    void InitializeDefenderButtons()
    {
        foreach (DefenderButtonData defenderButton in defenderButtons)
        {
            if (defenderButton.button != null)
            {
                defenderButton.button.interactable = false;
                defenderButton.button.onClick.RemoveAllListeners();
                defenderButton.button.onClick.AddListener(() => OnDefenderTypeButtonClicked(defenderButton));
            }
        }
    }

    void PlaceDefenderAtPosition(Vector3 position, GameObject defenderPrefab)
    {
        if (defenderPrefab == null) return;

        Vector3 offset = GetDefenderOffset(currentDefender);
        Vector3 finalPosition = position + offset;

        if (!IsPositionOccupied(finalPosition))
        {
            GameObject newDefender = Instantiate(defenderPrefab, finalPosition, Quaternion.identity);
            newDefender.layer = LayerMask.NameToLayer("DefenderLayer");
            Debug.Log($"Placed {currentDefender} at {finalPosition}");
        }
        else
        {
            Debug.Log("Cannot place defender: position already occupied!");
            if (Communicator != null && !Communicator.writingText)
            {
                Communicator.fullSentence = "Cannot place defender: position already occupied!";
                Communicator.StartCoroutine(Communicator.ShowTextLetterByLetter());
            }

        }
    }

    public void Normal()
    {
        Cursor.SetCursor(basicCursor, hotspot, cursorMode);
        CreateField = false;
        Sow = false;
        PlaceDefenders = false;
        currentSeed = SeedType.None;
        currentDefender = DefenderType.None;

        if (selectedCrop != null)
        {
            selectedCrop.DeselectCrop();
            selectedCrop = null;
        }
        foreach (GameObject tile in GameGrid)
        {
            if (tile == null) continue;

            SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = Color.white;
        }

        CloseContextMenu();
        ClosePlantMenu();
        DisableAllButtons();
    }

    public void plowing()
    {
        Cursor.SetCursor(FieldCursor, hotspot, cursorMode);
        CreateField = true;
        Sow = false;
        PlaceDefenders = false;
        currentSeed = SeedType.None;
        currentDefender = DefenderType.None;
    }

    public void Sowing()
    {
        Cursor.SetCursor(SeedCursor, hotspot, cursorMode);
        CreateField = false;
        Sow = true;
        PlaceDefenders = false;
        currentDefender = DefenderType.None;
    }

    // Button Management Methods
    public void EnableFarmingButtons()
    {
        foreach (var button in farmingButtons)
        {
            if (button != null)
                button.EnableIfAffordable();
        }
    }

    public void EnableDefenderButtons()
    {
        foreach (var button in defendingButtons)
        {
            if (button != null)
                button.EnableIfAffordable();
        }
    }

    public void DisableAllButtons()
    {
        foreach (var button in farmingButtons)
        {
            if (button != null)
                button.DisableButton();
        }

        foreach (var button in defendingButtons)
        {
            if (button != null)
                button.DisableButton();
        }
    }

    public void DisableFarmButtons()
    {
        foreach (var button in farmingButtons)
        {
            if (button != null)
                button.DisableButton();
        }
    }

    public void DisableDefenderButtons()
    {
        foreach (var button in defendingButtons)
        {
            if (button != null)
                button.DisableButton();
        }
    }

    // Methods for UniversalButton system
    public void PrepareSowing(SeedType seedType)
    {
        currentSeed = seedType;
        Cursor.SetCursor(SeedCursor, hotspot, cursorMode);
        Sow = true;
        CreateField = false;
        PlaceDefenders = false;
        currentDefender = DefenderType.None;
        EnableFarmingButtons();
        Debug.Log($"Selected {seedType} seed for planting");
    }

    public void PrepareDefender(DefenderType defenderType)
    {
        currentDefender = defenderType;
        Cursor.SetCursor(DefenderCursor, hotspot, cursorMode);
        PlaceDefenders = true;
        CreateField = false;
        Sow = false;
        currentSeed = SeedType.None;
        EnableDefenderButtons();
        Debug.Log($"Selected {defenderType} for placement");
    }

    // NEW METHOD: Called when tutorial is complete
    public void OnTutorialComplete()
    {
        Debug.Log("FarmGrid: Tutorial completed, full game systems enabled");
        if (Communicator != null && !Communicator.writingText)
        {
            Communicator.fullSentence = "FarmGrid: Tutorial completed, full game systems enabled";
            Communicator.StartCoroutine(Communicator.ShowTextLetterByLetter());
        }
        tutorialActive = false;
    }
}