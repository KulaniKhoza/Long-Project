using UnityEngine;



[RequireComponent(typeof(Rigidbody2D))]
public class AttackerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public string targetTag = "EnemyW"; // Only targets Worms

    [Header("Animation")]
    public Animator animator;

    private Rigidbody2D rb;
    private Transform target;
    private Vector2 moveDirection;
    private bool isAttacking = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Find the first object tagged as EnemyW (worm)
        GameObject targetObj = GameObject.FindGameObjectWithTag(targetTag);
        if (targetObj != null)
            target = targetObj.transform;
        else
            Debug.LogWarning($"{gameObject.name} couldn't find any GameObject tagged '{targetTag}'.");
    }

    private void Update()
    {
        if (isAttacking || target == null)
        {
            moveDirection = Vector2.zero;
            if (animator != null)
                animator.SetBool("isWalking", false);
            return;
        }

        // Only move horizontally towards the target
        float directionX = Mathf.Sign(target.position.x - transform.position.x);
        moveDirection = new Vector2(directionX, 0f);

        if (animator != null)
            animator.SetBool("isWalking", Mathf.Abs(moveDirection.x) > 0.1f);
    }

    private void FixedUpdate()
    {
        if (!isAttacking && target != null)
            rb.linearVelocity = moveDirection * moveSpeed;
        else
            rb.linearVelocity = Vector2.zero;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("EnemyW")) // Only kills Worms
        {
            Destroy(collision.gameObject);

            if (animator != null)
            {
                animator.SetBool("isWalking", false);
                animator.SetTrigger("Attack");
            }

            Destroy(gameObject, 0.2f); // adjust delay for attack animation
        }
    }
}


