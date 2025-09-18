using UnityEngine;

public class GrassObject : MonoBehaviour
{
    public Color defaultColor = Color.white;    // normal color
    public Color highlightColor = Color.yellow; // color when highlighted

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = defaultColor; // make sure it starts at default
    }

    void OnMouseEnter()
    {
        sr.color = highlightColor;
    }

    void OnMouseExit()
    {
        sr.color = defaultColor;
    }
}
