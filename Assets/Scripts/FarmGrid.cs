using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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
    public Texture2D basicCursor, SeedCursor;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotspot = Vector2.zero;
    public enum SeedType { None, Normal, Tomato, Corn }
    public SeedType currentSeed = SeedType.None;
    public GameObject normalSeedPrefab;
    public GameObject tomatoSeedPrefab;
    public GameObject cornSeedPrefab;
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
                    GameManager.Instance.SpawnUIAboveField(newField.transform, "-15");

                    Cursor.SetCursor(basicCursor, hotspot, cursorMode);
                    Sow = false;
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
            if (Sow && !CreateField)
            {
                // Raycast specifically for fields
                int fieldLayerMask = LayerMask.GetMask("FieldLayer");
                RaycastHit2D fieldHit = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity, fieldLayerMask);

                if (fieldHit.collider != null && fieldHit.collider.CompareTag("field"))
                {
                    GameObject seedToPlant = null;
                    bool hasSeed = false;
                    Debug.Log("Hit field: " + fieldHit.collider.name);

                    switch (currentSeed)
                    {
                        case SeedType.Normal:
                            if (GameManager.Instance.seeds > 0)
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

    public void PlantCorn()
    {
        PrepareSowing(SeedType.Corn);
    }

    public void PrepareSowing(SeedType seedType)
    {
        currentSeed = seedType;
        Cursor.SetCursor(SeedCursor, hotspot, cursorMode);
        Sow = true;
        CreateField = false;
    }

    public void Sowing()
    {
        Cursor.SetCursor(SeedCursor, hotspot, cursorMode);
        CreateField = false;
        Sow = true;
    }
}
