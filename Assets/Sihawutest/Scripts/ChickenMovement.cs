using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class ChickenMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public string targetTag = "EnemyW"; // Only targets Worms
    public float attackRange = 0.3f; // Reduced from minDistanceToTarget
    public float stoppingDistance = 0.1f;

    [Header("Animation")]
    public Animator animator;

    [Header("Attack Settings")]
    public int maxKills = 2;
    public int hitsToKill = 4; // Worm dies after 4 pecks
    public float attackCooldown = 0.5f;

    private Rigidbody2D rb;
    private Transform target;
    private Vector2 moveDirection;
    private bool isAttacking = false;
    private bool canAttack = true;
    private int currentKills = 0;

    // Track hit counts for each worm
    private System.Collections.Generic.Dictionary<GameObject, int> wormHitCounts = new System.Collections.Generic.Dictionary<GameObject, int>();

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        FindTarget();
    }

    private void Update()
    {
        // Debug visualization
        if (target != null)
            Debug.DrawLine(transform.position, target.position, Color.red);

        if (target == null)
        {
            FindTarget();
        }

        // No target or attacking - stop movement
        if (target == null || isAttacking)
        {
            moveDirection = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            UpdateAnimation(false);
            return;
        }

        // Calculate direction to target
        Vector2 direction = target.position - transform.position;
        float distance = direction.magnitude;

        // Check if we're close enough to attack
        if (distance <= attackRange && canAttack)
        {
            StartAttack();
            return;
        }

        // Movement logic - only move if not too close
        if (distance > stoppingDistance)
        {
            if (Mathf.Abs(direction.y) > 0.1f)
                moveDirection = new Vector2(0f, Mathf.Sign(direction.y)).normalized;
            else
                moveDirection = new Vector2(Mathf.Sign(direction.x), 0f).normalized;
        }
        else
        {
            moveDirection = Vector2.zero;
        }

        UpdateAnimation(moveDirection.magnitude > 0.1f);
    }

    private void FixedUpdate()
    {
        if (!isAttacking && target != null && moveDirection.magnitude > 0.1f)
            rb.linearVelocity = moveDirection * moveSpeed;
        else
            rb.linearVelocity = Vector2.zero;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(targetTag) && !isAttacking && canAttack)
        {
            StartAttack();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Also check for continuous collision
        if (collision.collider.CompareTag(targetTag) && !isAttacking && canAttack)
        {
            StartAttack();
        }
    }

    private void StartAttack()
    {
        if (!canAttack || target == null) return;

        isAttacking = true;
        canAttack = false;
        rb.linearVelocity = Vector2.zero;

        // Play peck animation
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetTrigger("Peck");
        }

        // Damage the target worm
        GameObject wormObject = target.gameObject;

        // Track hits for this worm
        if (!wormHitCounts.ContainsKey(wormObject))
        {
            wormHitCounts[wormObject] = 0;
        }

        wormHitCounts[wormObject]++;
        int currentHits = wormHitCounts[wormObject];

        Debug.Log($"Chicken pecked worm! Hit {currentHits}/{hitsToKill}");

        // Check if worm should die
        if (currentHits >= hitsToKill)
        {
            // Remove from dictionary before destroying
            wormHitCounts.Remove(wormObject);
            Destroy(wormObject);
            currentKills++;

            Debug.Log($"Worm killed! Total kills: {currentKills}/{maxKills}");

            // Check if chicken should be destroyed
            CheckChickenDestruction();
        }

        // Resume movement after attack
        StartCoroutine(ResumeMovementAfterAttack());
    }

    private void CheckChickenDestruction()
    {
        int remainingWorms = GameObject.FindGameObjectsWithTag(targetTag).Length;

        if (currentKills >= maxKills || (currentKills >= 1 && remainingWorms == 0))
        {
            Debug.Log("Chicken completed its mission, destroying...");
            Destroy(gameObject, 0.3f);
        }
    }

    private IEnumerator ResumeMovementAfterAttack()
    {
        // Wait for attack animation
        if (animator != null)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            float waitTime = state.length > 0 ? state.length * 0.8f : 0.5f;
            yield return new WaitForSeconds(waitTime);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        isAttacking = false;

        // Brief cooldown before next attack
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;

        // Find new target (current one might be dead or pushed away)
        FindTarget();
    }

    private void FindTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        if (targets.Length > 0)
        {
            // Find closest target
            GameObject closest = null;
            float closestDistance = Mathf.Infinity;
            Vector3 currentPosition = transform.position;

            foreach (GameObject targetObj in targets)
            {
                if (targetObj == null) continue;

                float distance = Vector3.Distance(currentPosition, targetObj.transform.position);
                if (distance < closestDistance)
                {
                    closest = targetObj;
                    closestDistance = distance;
                }
            }

            if (closest != null)
            {
                target = closest.transform;
                Debug.Log($"Chicken found new target: {target.name}");
            }
            else
            {
                target = null;
            }
        }
        else
        {
            target = null;
            Debug.Log("Chicken: No targets found");
        }
    }

    private void UpdateAnimation(bool isWalking)
    {
        if (animator != null)
            animator.SetBool("isWalking", isWalking);
    }

    // Public method to manually set target (useful for external control)
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // Public method to check if chicken is active
    public bool IsActive()
    {
        return !isAttacking && target != null;
    }

    // Getters for external scripts
    public int GetCurrentKills() => currentKills;
    public int GetMaxKills() => maxKills;
    public Transform GetCurrentTarget() => target;
}