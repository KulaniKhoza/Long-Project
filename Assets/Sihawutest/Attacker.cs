using UnityEngine;
using System.Collections;

public class Attacker : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float attackRange = 1f;
    public float attackCooldown = 1f;
    public int killsBeforeDeath = 2;

    private GameObject targetEnemy;
    private Animator animator;
    private Rigidbody2D rb;
    private int totalKills = 0;
    private bool isAttacking = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Make sure Rigidbody doesn’t fall or rotate
        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        if (targetEnemy != null)
        {
            float distance = Vector2.Distance(rb.position, targetEnemy.transform.position);
            if (distance > attackRange)
            {
                Vector2 newPos = Vector2.MoveTowards(rb.position, targetEnemy.transform.position, moveSpeed * Time.fixedDeltaTime);
                rb.MovePosition(newPos);
            }
        }
    }

    GameObject FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }
        return closest;
    }

    IEnumerator AttackEnemy()
    {
        isAttacking = true;

        while (targetEnemy != null && Vector2.Distance(transform.position, targetEnemy.transform.position) <= attackRange)
        {
            // Trigger attack animation
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            yield return new WaitForSeconds(attackCooldown);

            // Deal damage
            if (targetEnemy != null)
            {
                Enemy enemyScript = targetEnemy.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.TakeHit();

                    // If enemy destroyed → count kill
                    if (enemyScript == null || targetEnemy == null)
                    {
                        totalKills++;
                        targetEnemy = null;

                        if (totalKills >= killsBeforeDeath)
                        {
                            Destroy(gameObject); // Attacker dies after reaching kill limit
                            yield break;
                        }
                    }
                }
            }
        }

        isAttacking = false;
    }
}
