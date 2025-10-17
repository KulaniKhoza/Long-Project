using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class FarmGrid : MonoBehaviour
{
    // Your existing variables...
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

    public enum Mode { Farming, Defending }
    public Mode currentMode = Mode.Farming;

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
    private int plowprice = 10;
    public TextMeshProUGUI textPrefab;

    // Colors for highlight and selection
    public Color highlightColor = Color.red;
    public Color selectionColor = Color.magenta;
    private Color defaultColor = Color.white;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        Time.timeScale = 1.0f;
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
        // Handle keyboard shortcuts
        HandleKeyboardInput();

        // Read mouse position and convert to world
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));

        // Remove previous highlight if not selected
        if (highlight != null && highlight != selection)
        {
            SpriteRenderer sr = highlight.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = defaultColor;
            highlight = null;
        }

        // Raycast for highlight
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (hit.collider != null && hit.collider.CompareTag("grid") && hit.collider.transform != selection)
            {
                highlight = hit.collider.transform;
                SpriteRenderer sr = highlight.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = highlightColor;
            }
        }

        // Handle left click
        if (Mouse.current.leftButton.wasPressedThisFrame && !EventSystem.current.IsPointerOverGameObject())
        {
            if (highlight != null)
            {
                // Reset old selection
                if (selection != null)
                {
                    SpriteRenderer prevSR = selection.GetComponent<SpriteRenderer>();
                    if (prevSR != null) prevSR.color = defaultColor;
                }

                // New selection
                selection = highlight;
                SpriteRenderer selSR = selection.GetComponent<SpriteRenderer>();
                if (selSR != null) selSR.color = selectionColor;

                // =======================
                // Create field
                // =======================
                if (CreateField)
                {
                    GameObject newField = Instantiate(field, selection.position, Quaternion.identity);
                    GameManager.Instance.Money -= plowprice;
                    GameManager.Instance.SpawnUIAboveField(newField.transform, "-10");

                    Cursor.SetCursor(basicCursor, hotspot, cursorMode);
                    Sow = false;
                    CreateField = false;
                }

                // =======================
                // Place defenders
                // =======================
                if (PlaceDefenders)
                {
                    PlaceDefender(selection.position);
                }
            }
            else
            {
                // Clicked empty space - deselect
                if (selection != null)
                {
                    SpriteRenderer selSR = selection.GetComponent<SpriteRenderer>();
                    if (selSR != null) selSR.color = defaultColor;
                    selection = null;
                }
            }

            // =======================
            // Sow seeds (SEPARATE from grid selection)
            // =======================
            if (Sow && !CreateField && !PlaceDefenders)
            {
                // Raycast specifically for fields
                int fieldLayerMask = LayerMask.GetMask("FieldLayer");
                RaycastHit2D fieldHit = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity, fieldLayerMask);

                if (fieldHit.collider != null && fieldHit.collider.CompareTag("field"))
                {
                    GameObject seedToPlant = null;
                    bool hasSeed = false;

                    switch (currentSeed)
                    {
                        case SeedType.Normal:
                            if (GameManager.Instance.seeds > 0 )
                            {
                                seedToPlant = normalSeedPrefab;
                                GameManager.Instance.seeds--;
                                hasSeed = true;
                            }
                            break;

                        case SeedType.Tomato:
                            if (GameManager.Instance.tomatoSeeds > 0)
                            {
                                seedToPlant = tomatoSeedPrefab;
                                GameManager.Instance.tomatoSeeds--;
                                hasSeed = true;
                            }
                            break;

                        case SeedType.Corn:
                            if (GameManager.Instance.cornSeeds > 0)
                            {
                                seedToPlant = cornSeedPrefab;
                                GameManager.Instance.cornSeeds--;
                                hasSeed = true;
                            }
                            break;
                    }

                    if (hasSeed && seedToPlant != null)
                    {
                        // Offset slightly upward
                        Vector3 spawnPos = fieldHit.collider.transform.position + new Vector3(0, 0.1f, 0);
                        Instantiate(seedToPlant, spawnPos, Quaternion.identity);
                        Debug.Log("Planted seed at: " + spawnPos);
                        Normal();
                    }
                    else if (!hasSeed)
                    {
                        Debug.Log("Not enough seeds!");
                    }
                }
                else
                {
                    Debug.Log("No field found at mouse position");
                }
            }
        }
    }

    void HandleKeyboardInput()
    {
        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            Normal();
        }

        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            plowing();
        }

        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            Sowing();
        }

        // Toggle between farming and defending modes
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ToggleMode();
        }
    }

    void PlaceDefender(Vector3 position)
    {
        GameObject defenderToPlace = null;

        switch (currentDefender)
        {
            case DefenderType.Archer:
                defenderToPlace = archerDefenderPrefab;
                break;

            case DefenderType.Mage:
                defenderToPlace = mageDefenderPrefab;
                break;

            case DefenderType.Fence:
                defenderToPlace = fenceDefenderPrefab;
                break;
        }

        if (defenderToPlace != null)
        {
            // Check if position is not occupied
            if (!IsPositionOccupied(position))
            {
                // NO MONEY DEDUCTION - just place the defender
                Instantiate(defenderToPlace, position, Quaternion.identity);
                Debug.Log($"Placed {currentDefender} at {position}");
            }
            else
            {
                Debug.Log("Position already occupied!");
            }
        }
    }

    bool IsPositionOccupied(Vector3 position)
    {
        // Check for existing defenders or crops at this position
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 0.3f);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Defender") || collider.CompareTag("Crops") || collider.CompareTag("field"))
            {
                return true;
            }
        }
        return false;
    }

    public void ToggleMode()
    {
        if (currentMode == Mode.Farming)
        {
            currentMode = Mode.Defending;
            Normal(); // Reset to basic mode when switching
            Debug.Log("Switched to DEFENDING mode");
        }
        else
        {
            currentMode = Mode.Farming;
            Normal(); // Reset to basic mode when switching
            Debug.Log("Switched to FARMING mode");
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
        Debug.Log("Switched to Normal mode");
    }

    public void plowing()
    {
        if (currentMode != Mode.Farming) return;

        Cursor.SetCursor(FieldCursor, hotspot, cursorMode);
        CreateField = true;
        Sow = false;
        PlaceDefenders = false;
        currentSeed = SeedType.None;
        currentDefender = DefenderType.None;
        Debug.Log("Switched to Plowing mode");
    }

    public void Sowing()
    {
        if (currentMode != Mode.Farming) return;

        Cursor.SetCursor(SeedCursor, hotspot, cursorMode);
        CreateField = false;
        Sow = true;
        PlaceDefenders = false;
        currentDefender = DefenderType.None;
        Debug.Log("Switched to Sowing mode");
    }

    // Methods for CropButton system
    public void PrepareSowing(SeedType seedType)
    {
        if (currentMode != Mode.Farming) return;

        currentSeed = seedType;
        Cursor.SetCursor(SeedCursor, hotspot, cursorMode);
        Sow = true;
        CreateField = false;
        PlaceDefenders = false;
        currentDefender = DefenderType.None;
        Debug.Log($"Selected {seedType} seed for planting");
    }

    public void PrepareDefender(DefenderType defenderType)
    {
        if (currentMode != Mode.Defending) return;

        currentDefender = defenderType;
        Cursor.SetCursor(DefenderCursor, hotspot, cursorMode);
        PlaceDefenders = true;
        CreateField = false;
        Sow = false;
        currentSeed = SeedType.None;
        Debug.Log($"Selected {defenderType} for placement");
    }

    // Check methods for button interactivity - NO MONEY CHECKS
    public bool HasSeeds() { return GameManager.Instance.seeds > 0; }
    public bool HasCornSeeds() { return GameManager.Instance.cornSeeds > 0; }
    public bool HasTomatoSeeds() { return GameManager.Instance.tomatoSeeds > 0; }
}