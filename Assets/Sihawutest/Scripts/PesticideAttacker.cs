using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PesticideAttacker : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public string targetTag = "Enemy";
    public float knockbackForce = 5f;

    [Header("Animation")]
    public Animator animator;

    private Rigidbody2D rb;
    private Vector2 moveDirection;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        GameObject targetObj = GameObject.FindGameObjectWithTag(targetTag);
        if (targetObj != null)
        {
            Vector2 direction = (targetObj.transform.position - transform.position).normalized;

            // Move straight (no diagonal)
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                moveDirection = new Vector2(Mathf.Sign(direction.x), 0f);
            else
                moveDirection = new Vector2(0f, Mathf.Sign(direction.y));
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveDirection * moveSpeed;

        if (animator != null)
            animator.SetBool("isWalking", rb.linearVelocity.magnitude > 0.1f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(targetTag))
        {
            EnemyMovement enemy = collision.collider.GetComponent<EnemyMovement>();
            if (enemy != null)
            {
                enemy.currentHits++;

                // Knockback
                Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                knockbackDir.y = 0;
                enemy.ApplyKnockback(knockbackDir * knockbackForce);

                if (animator != null)
                    animator.SetTrigger("Spray");

                // Kill enemy after 4 hits
                if (enemy.currentHits >= enemy.hitsToDie)
                {
                    Destroy(enemy.gameObject);
                    Destroy(gameObject); // Pesticide dies after killing
                }
            }
        }
    }
}



