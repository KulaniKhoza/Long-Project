using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class ChickenMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public string targetTag = "EnemyW"; // Only targets Worms
    public float attackRange = 0.3f;
    public float stoppingDistance = 0.1f;
    public float noTargetCheckInterval = 2f; // How often to check for no targets
    public float horizontalTolerance = 0.1f; // How close Y positions need to be to consider same horizontal plane

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
    private float lastTargetCheckTime = 0f;
    private SpriteRenderer spriteRenderer;

    // Track hit counts for each worm
    private System.Collections.Generic.Dictionary<GameObject, int> wormHitCounts = new System.Collections.Generic.Dictionary<GameObject, int>();

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        FindTarget();
        lastTargetCheckTime = Time.time;
    }

    private void Update()
    {
        // Check for no targets periodically
        if (Time.time - lastTargetCheckTime >= noTargetCheckInterval)
        {
            CheckForNoTargets();
            lastTargetCheckTime = Time.time;
        }

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

        // Calculate horizontal direction to target
        float horizontalDirection = target.position.x - transform.position.x;
        float horizontalDistance = Mathf.Abs(horizontalDirection);

        // Check if we're close enough to attack
        if (horizontalDistance <= attackRange && canAttack)
        {
            StartAttack();
            return;
        }

        // Movement logic - only move horizontally if not too close
        if (horizontalDistance > stoppingDistance)
        {
            moveDirection = new Vector2(Mathf.Sign(horizontalDirection), 0f);

            // Flip sprite based on movement direction
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = horizontalDirection < 0;
            }
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

    private void CheckForNoTargets()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        if (targets.Length == 0 && target == null)
        {
            Debug.Log("No targets found - chicken disappearing");
            DestroyChicken();
        }
    }

    private void DestroyChicken()
    {
        // Optional: Play disappear animation
        if (animator != null)
        {
            animator.SetTrigger("Disappear");
            Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(targetTag) && !isAttacking && canAttack)
        {
            // Only attack if the worm is in the same horizontal plane
            if (IsInSameHorizontalPlane(collision.transform))
            {
                StartAttack();
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Also check for continuous collision
        if (collision.collider.CompareTag(targetTag) && !isAttacking && canAttack)
        {
            // Only attack if the worm is in the same horizontal plane
            if (IsInSameHorizontalPlane(collision.transform))
            {
                StartAttack();
            }
        }
    }

    private bool IsInSameHorizontalPlane(Transform otherTransform)
    {
        float yDifference = Mathf.Abs(transform.position.y - otherTransform.position.y);
        return yDifference <= horizontalTolerance;
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
            DestroyChicken();
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

        // Immediately check if no targets after attack
        CheckForNoTargets();
    }

    private void FindTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        if (targets.Length > 0)
        {
            // Find closest target that's in the same horizontal plane
            GameObject closest = null;
            float closestDistance = Mathf.Infinity;
            Vector3 currentPosition = transform.position;

            foreach (GameObject targetObj in targets)
            {
                if (targetObj == null) continue;

                // Only consider targets in the same horizontal plane
                if (!IsInSameHorizontalPlane(targetObj.transform)) continue;

                // Only consider horizontal distance for targeting
                float distance = Mathf.Abs(currentPosition.x - targetObj.transform.position.x);
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
                CheckForNoTargets();
            }
        }
        else
        {
            target = null;
            Debug.Log("Chicken: No targets found");
            CheckForNoTargets();
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