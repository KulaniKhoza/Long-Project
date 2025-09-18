using System.Collections;
using System.Collections.Generic;
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

    public Texture2D basicCursor;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotspot = Vector2.zero;

    public GameObject gridOriginObject;

    private Transform highlight;
    private Transform selection;

    // Colors for highlight and selection
    public Color highlightColor = Color.red;
    public Color selectionColor = Color.magenta;
    private Color defaultColor = Color.white;

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
        Vector2 mousePos = Mouse.current.position.ReadValue(); // new Input System
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        // Remove previous highlight if it’s not selected
        if (highlight != null && highlight != selection)
        {
            SpriteRenderer sr = highlight.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = defaultColor;
            highlight = null;
        }

        // Raycast 2D
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (hit.collider != null && hit.collider.CompareTag("grid") && hit.collider.transform != selection)
            {
                highlight = hit.collider.transform;

                SpriteRenderer sr = highlight.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.color = highlightColor;
            }
            else
            {
                highlight = null;
            }
        }

        // Selection
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (highlight != null)
            {
                // Reset previous selection
                if (selection != null)
                {
                    SpriteRenderer prevSR = selection.GetComponent<SpriteRenderer>();
                    if (prevSR != null)
                        prevSR.color = defaultColor;
                }

                selection = highlight;
                SpriteRenderer selSR = selection.GetComponent<SpriteRenderer>();
                if (selSR != null)
                    selSR.color = selectionColor;

                highlight = null;
            }
            else
            {
                // Deselect if clicked empty
                if (selection != null)
                {
                    SpriteRenderer selSR = selection.GetComponent<SpriteRenderer>();
                    if (selSR != null)
                        selSR.color = defaultColor;

                    selection = null;
                }
            }
        }
    }
}
