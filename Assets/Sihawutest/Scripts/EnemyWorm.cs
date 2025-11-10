using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyWorm : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public string targetTag = "Plant";

    [Header("Health Settings")]
    public int maxHits = 4;
    public int currentHits = 0;
    public float hitCooldown = 0.5f;
    public float knockbackForce = 2f;
    public float knockbackDuration = 0.2f;

    [Header("Visual Effects")]
    public GameObject hitEffect;
    public GameObject deathEffect;
    public AudioClip hitSound;
    public AudioClip deathSound;

    [Header("Animation")]
    public Animator animator;

    private Rigidbody2D rb;
    private Transform target;
    private Vector2 moveDirection;
    private bool isAttacking = false;
    private bool isKnockedBack = false;
    private float lastHitTime = 0f;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject targetObj = GameObject.FindGameObjectWithTag(targetTag);
        if (targetObj != null)
            target = targetObj.transform;
        else
            Debug.LogWarning($"{gameObject.name} couldn't find any GameObject tagged '{targetTag}'.");
    }

    private void Update()
    {
        if (isAttacking || target == null || isKnockedBack)
        {
            moveDirection = Vector2.zero;
            if (animator != null)
                animator.SetBool("isWalking", false);
            return;
        }

        Vector2 direction = target.position - transform.position;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            moveDirection = new Vector2(Mathf.Sign(direction.x), 0f);
        else
            moveDirection = new Vector2(0f, Mathf.Sign(direction.y));

        if (animator != null)
            animator.SetBool("isWalking", moveDirection.magnitude > 0.1f);
    }

    private void FixedUpdate()
    {
        if (!isAttacking && !isKnockedBack && target != null)
            rb.linearVelocity = moveDirection * moveSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Plant"))
        {
            StartCoroutine(AttackPlant());
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Attacker"))
        {
            if (Time.time - lastHitTime >= hitCooldown)
            {
                TakeDamage(1); // Use the new damage system

                // Apply knockback
                Vector2 knockbackDir = moveDirection.normalized;
                if (Vector2.Dot(knockbackDir, (collision.transform.position - transform.position)) > 0)
                    knockbackDir *= -1;
                rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
                StartCoroutine(KnockbackCooldown());
            }
        }
    }

    // This is called by ChickenMovement
    public void TakeDamage(int damage = 1)
    {
        currentHits += damage;
        lastHitTime = Time.time;

        // Visual feedback
        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, Quaternion.identity);

        if (hitSound != null)
            AudioSource.PlayClipAtPoint(hitSound, transform.position);

        StartCoroutine(FlashRed());

        if (currentHits >= maxHits)
        {
            Die();
        }
        else if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
    }

    private IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }

    private void Die()
    {
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position);

        if (animator != null)
        {
            animator.SetTrigger("Die");
            Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator KnockbackCooldown()
    {
        isKnockedBack = true;
        yield return new WaitForSeconds(knockbackDuration);
        isKnockedBack = false;
    }

    private IEnumerator AttackPlant()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetTrigger("Attack");
        }
        yield return new WaitForSeconds(1.5f);
        isAttacking = false;
    }

    // Getters for external scripts
    public bool IsAlive() => currentHits < maxHits;
    public float GetHealthPercentage() => 1f - ((float)currentHits / maxHits);
    public int GetRemainingHits() => maxHits - currentHits;
}
