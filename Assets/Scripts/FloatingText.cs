using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float floatSpeed = 50f;
    public float fadeDuration = 1.5f;

    private TextMeshProUGUI tmp;
    private Color startColor;
    private float timer;

    void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        startColor = tmp.color;
    }

    void Update()
    {
        // Move upwards in UI space
        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);

        // Fade out
        timer += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
        tmp.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

        if (timer >= fadeDuration)
            Destroy(gameObject);
    }
}
