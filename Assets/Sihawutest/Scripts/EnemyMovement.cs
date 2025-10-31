using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float moveSpeed = 2f;
    public int hitsToDie = 4;
    [HideInInspector] public int currentHits = 0;
    [HideInInspector] public Rigidbody2D rb;

    [Header("Target")]
    public string targetTag = "Plant";
    private Transform target;

    [Header("Animation")]
    public Animator animator;

    [Header("Knockback Settings")]
    public float knockbackRecoveryTime = 0.3f;
    private bool isKnockedBack = false;
    private Vector2 moveDirection;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        GameObject targetObj = GameObject.FindGameObjectWithTag(targetTag);
        if (targetObj != null)
        {
            target = targetObj.transform;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} couldn't find a target tagged '{targetTag}'.");
        }
    }

    private void FixedUpdate()
    {
        if (isKnockedBack || target == null) return;

        Vector2 direction = (target.position - transform.position).normalized;

        // Move in straight lines (no diagonals)
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            moveDirection = new Vector2(Mathf.Sign(direction.x), 0f);
        else
            moveDirection = new Vector2(0f, Mathf.Sign(direction.y));

        rb.linearVelocity = moveDirection * moveSpeed;

        if (animator != null)
            animator.SetBool("isWalking", rb.linearVelocity.magnitude > 0.1f);
    }

    public void ApplyKnockback(Vector2 force)
    {
        StartCoroutine(KnockbackRoutine(force));
    }

    private IEnumerator KnockbackRoutine(Vector2 force)
    {
        isKnockedBack = true;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);
        yield return new WaitForSeconds(knockbackRecoveryTime);
        isKnockedBack = false;
    }
}





