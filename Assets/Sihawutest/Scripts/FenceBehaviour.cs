using UnityEngine;
using System.Collections;

public class FenceBehaviour : MonoBehaviour
{
    [Header("Fence Settings")]
    public int hitsToBreak = 6;
    private int currentHits = 0;
    public float shakeDuration = 0.2f;
    public float shakeStrength = 0.1f;

    private Vector3 originalPos;
    private bool isShaking = false;

    private void Start()
    {
        originalPos = transform.localPosition;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Ignore collision if it's a worm (EnemyW)
        if (collision.collider.CompareTag("EnemyW"))
        {
            // Let worm pass through by disabling collision for a moment
            Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>(), true);
            StartCoroutine(ReenableCollision(collision.collider));
            return;
        }

        // Only react to pests (Enemy)
        if (collision.collider.CompareTag("Enemy"))
        {
            currentHits++;
            if (!isShaking)
                StartCoroutine(ShakeFence());

            if (currentHits >= hitsToBreak)
            {
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator ReenableCollision(Collider2D col)
    {
        yield return new WaitForSeconds(1f); // short delay so worm passes through fully
        if (col != null)
            Physics2D.IgnoreCollision(col, GetComponent<Collider2D>(), false);
    }

    private IEnumerator ShakeFence()
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-shakeStrength, shakeStrength);
            float y = Random.Range(-shakeStrength, shakeStrength);
            transform.localPosition = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
        isShaking = false;
    }
}
