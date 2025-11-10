using UnityEngine;
using System.Collections;

public class PesticideAttacker : MonoBehaviour
{
    [Header("Attack Settings")]
    public string targetTag = "Enemy";
    public float attackRange = 10f;
    public float attackCooldown = 6f;
    public int maxKills = 2;
    public float noTargetCheckInterval = 3f;

    [Header("Projectile Settings")]
    public GameObject gasProjectile;
    public Transform firePoint;
    public float projectileSpeed = 8f;

    [Header("Animation")]
    public Animator animator;

    [Header("Detection Settings")]
    public float detectionPlaneHeight = 1f; // Height above pesticide for detection
    public float yTolerance = 0.5f; // How close Y positions need to be

    private Transform currentTarget;
    private bool canAttack = true;
    private int currentKills = 0;
    private float lastTargetCheckTime = 0f;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        Debug.Log("Pesticide Attacker spawned at: " + transform.position);

        // Give time for enemies to spawn, then find target
        Invoke("DelayedStart", 1f);
    }

    private void DelayedStart()
    {
        FindTarget();
        lastTargetCheckTime = Time.time;

        if (currentTarget == null)
        {
            Debug.LogWarning("No target found on startup. Will keep searching...");
        }
    }

    private void Update()
    {
        // Check for no targets periodically
        if (Time.time - lastTargetCheckTime >= noTargetCheckInterval)
        {
            CheckForNoTargets();
            lastTargetCheckTime = Time.time;
        }

        // If we don't have a target, try to find one
        if (currentTarget == null)
        {
            FindTarget();
        }
        else
        {
            // Check if target is still valid
            if (!IsTargetValid(currentTarget))
            {
                Debug.Log("Target became invalid, finding new one");
                currentTarget = null;
                FindTarget();
                return;
            }

            // Flip sprite to face target
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = currentTarget.position.x < transform.position.x;
            }

            // Auto-attack when cooldown is complete
            if (canAttack)
            {
                StartAttack();
            }
        }
    }

    private bool IsTargetValid(Transform target)
    {
        if (target == null) return false;
        if (!target.gameObject.activeInHierarchy) return false;

        float distance = Mathf.Abs(target.position.x - transform.position.x);
        bool inRange = distance <= attackRange;
        bool samePlane = IsInSameHorizontalPlane(target);

        Debug.Log($"Target {target.name} - Distance: {distance}, InRange: {inRange}, SamePlane: {samePlane}");

        return inRange && samePlane;
    }

    private void FindTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        Debug.Log($"Searching for targets with tag '{targetTag}'. Found {targets.Length} objects.");

        if (targets.Length > 0)
        {
            // Find closest target in the same horizontal plane and in range
            GameObject closest = null;
            float closestDistance = Mathf.Infinity;
            Vector3 currentPosition = transform.position;
            int validTargetsCount = 0;

            foreach (GameObject targetObj in targets)
            {
                if (targetObj == null) continue;

                // Check if target is valid
                if (IsTargetValid(targetObj.transform))
                {
                    float distance = Mathf.Abs(currentPosition.x - targetObj.transform.position.x);
                    Debug.Log($"Valid target: {targetObj.name} at position {targetObj.transform.position}, distance: {distance}");
                    validTargetsCount++;

                    if (distance < closestDistance)
                    {
                        closest = targetObj;
                        closestDistance = distance;
                    }
                }
                else
                {
                    Debug.Log($"Invalid target: {targetObj.name} at position {targetObj.transform.position}");
                    Debug.Log($"Pesticide position: {transform.position}, Y difference: {Mathf.Abs(transform.position.y - targetObj.transform.position.y)}");
                }
            }

            Debug.Log($"Total valid targets found: {validTargetsCount}");

            if (closest != null)
            {
                currentTarget = closest.transform;
                Debug.Log($"Pesticide locked onto: {currentTarget.name} at position {currentTarget.position}");
            }
            else
            {
                currentTarget = null;
                Debug.Log("No valid targets found");
            }
        }
        else
        {
            currentTarget = null;
            Debug.Log("No objects found with tag: " + targetTag);
        }
    }

    private bool IsInSameHorizontalPlane(Transform otherTransform)
    {
        if (otherTransform == null) return false;

        // Calculate the detection plane position (pesticide position + detection height)
        float detectionPlaneY = transform.position.y + detectionPlaneHeight;

        // Check if target is within tolerance of the detection plane
        float yDifference = Mathf.Abs(detectionPlaneY - otherTransform.position.y);
        bool inSamePlane = yDifference <= yTolerance;

        Debug.Log($"Detection plane Y: {detectionPlaneY}, Target Y: {otherTransform.position.y}, Difference: {yDifference}, Tolerance: {yTolerance}, InSamePlane: {inSamePlane}");

        return inSamePlane;
    }

    private void StartAttack()
    {
        if (!canAttack || currentTarget == null)
        {
            Debug.Log($"Cannot attack: canAttack={canAttack}, target={currentTarget}");
            return;
        }

        Debug.Log("Starting attack sequence");
        canAttack = false;

        // Play spray animation
        if (animator != null)
        {
            animator.SetTrigger("Spray");
            Debug.Log("Playing spray animation");
        }

        // Shoot gas bullet after a short delay to sync with animation
        Invoke("ShootGas", 0.3f);

        // Start cooldown for next attack
        StartCoroutine(AttackCooldown());
    }

    private void ShootGas()
    {
        if (gasProjectile == null)
        {
            Debug.LogError("Gas Projectile is not assigned in the inspector!");
            return;
        }

        if (firePoint == null)
        {
            Debug.LogError("Fire Point is not assigned in the inspector!");
            return;
        }

        if (currentTarget == null)
        {
            Debug.LogWarning("Target disappeared before shooting");
            return;
        }

        Debug.Log($"Shooting gas bullet at {currentTarget.name}");

        // Create the gas bullet
        GameObject projectile = Instantiate(gasProjectile, firePoint.position, Quaternion.identity);

        // Get the GasBullet component and initialize it
        GasBullet gasScript = projectile.GetComponent<GasBullet>();
        if (gasScript != null)
        {
            // Calculate direction to target
            Vector2 direction = (currentTarget.position - firePoint.position).normalized;
            gasScript.Initialize(direction, projectileSpeed, this);
        }
    }

    private System.Collections.IEnumerator AttackCooldown()
    {
        Debug.Log($"Attack cooldown started. Next attack in {attackCooldown} seconds");
        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
        Debug.Log("Attack cooldown complete. Ready to attack again.");
    }

    // Called by GasBullet when it kills an enemy
    public void OnEnemyKilled()
    {
        currentKills++;
        Debug.Log($"Pesticide kill count: {currentKills}/{maxKills}");

        // Find a new target since current one died
        currentTarget = null;
        FindTarget();

        if (currentKills >= maxKills)
        {
            Debug.Log("Pesticide reached max kills, destroying...");
            DestroyPesticide();
        }
    }

    private void CheckForNoTargets()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        bool foundValidTarget = false;

        foreach (GameObject targetObj in targets)
        {
            if (targetObj != null && IsTargetValid(targetObj.transform))
            {
                foundValidTarget = true;
                Debug.Log($"Valid target still exists: {targetObj.name}");
                break;
            }
        }

        if (!foundValidTarget && currentTarget == null)
        {
            Debug.Log("No targets in range - pesticide disappearing");
            DestroyPesticide();
        }
    }

    private void DestroyPesticide()
    {
        Debug.Log("Destroying pesticide attacker");
        // Play disappear animation if available
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

    // Gizmos for visualization - Now matches actual detection logic
    private void OnDrawGizmosSelected()
    {
        // Calculate detection plane position (matches the detection logic)
        Vector3 detectionPlaneCenter = transform.position + Vector3.up * detectionPlaneHeight;

        // Draw attack range as a circle at detection plane height
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(detectionPlaneCenter, attackRange);

        // Draw detection plane as a thick horizontal line
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Vector3 planeStart = detectionPlaneCenter + Vector3.left * attackRange;
        Vector3 planeEnd = detectionPlaneCenter + Vector3.right * attackRange;

        // Draw the tolerance range above and below the detection plane
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Vector3 toleranceTop = detectionPlaneCenter + Vector3.up * yTolerance;
        Vector3 toleranceBottom = detectionPlaneCenter + Vector3.down * yTolerance;

        Gizmos.DrawLine(toleranceTop + Vector3.left * attackRange, toleranceTop + Vector3.right * attackRange);
        Gizmos.DrawLine(toleranceBottom + Vector3.left * attackRange, toleranceBottom + Vector3.right * attackRange);

        // Draw line to target when in play mode
        if (Application.isPlaying && currentTarget != null)
        {
            Gizmos.color = Color.red;
            Vector3 startPos = firePoint != null ? firePoint.position : detectionPlaneCenter;
            Gizmos.DrawLine(startPos, currentTarget.position);

            // Draw small sphere at target
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(currentTarget.position, 0.3f);
        }
    }

    private void OnDrawGizmos()
    {
        // Always show the detection plane in scene view
        Vector3 detectionPlaneCenter = transform.position + Vector3.up * detectionPlaneHeight;

        // Draw subtle detection plane
        Gizmos.color = new Color(0, 1, 0, 0.1f);
        Gizmos.DrawWireSphere(detectionPlaneCenter, attackRange);
    }
}