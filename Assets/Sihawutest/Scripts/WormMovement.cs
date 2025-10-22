using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class WormMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public string targetTag = "Plant";

    [Header("Animation")]
    public Animator animator;

    [Header("Health Settings")]
    public int hitsToDie = 4;
    public float hitCooldown = 0.5f;
    public float knockbackForce = 2f;
    public float knockbackDuration = 0.2f;

    private Rigidbody2D rb;
    private Transform target;
    private Vector2 moveDirection;
    private bool isAttacking = false;
    private bool isKnockedBack = false;

    private int currentHits = 0;
    private float lastHitTime = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

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

        // Move in straight lines only
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
                currentHits++;
                lastHitTime = Time.time;

                // Apply knockback along current movement axis only
                Vector2 knockbackDir = moveDirection.normalized; // Use movement direction axis
                // Reverse if attacker is in front
                if (Vector2.Dot(knockbackDir, (collision.transform.position - transform.position)) > 0)
                    knockbackDir *= -1;

                rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);

                StartCoroutine(KnockbackCooldown());

                if (currentHits >= hitsToDie)
                    Destroy(gameObject);
            }
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

        yield return new WaitForSeconds(1.5f); // Adjust to animation length
        isAttacking = false;
    }
}



