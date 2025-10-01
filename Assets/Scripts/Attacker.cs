using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Attacker : MonoBehaviour
{

    public float moveSpeed = 2f;
    public float attackRange = 1f;
    public float attackCooldown = 1f;
    public int killsBeforeDeath = 2;
    public Animator Animator;

    private GameObject targetEnemy;
    private Animator animator;
    private Rigidbody2D rb;
    private int totalKills = 0;
    private bool isAttacking = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }

    Vector2 move;

    void Update()
    {
        move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));


        // Rotatation
        if (move.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(move.y, move.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            rb.MoveRotation(angle);
        }

    }

    void FixedUpdate()
    {
        if (isAttacking) return;

        if (targetEnemy == null)
        {
            targetEnemy = FindClosestEnemy();
            if (targetEnemy == null) return;
        }

        float distance = Vector2.Distance(rb.position, targetEnemy.transform.position);

        if (distance > attackRange)
        {

            Vector2 newPos = Vector2.MoveTowards(rb.position, targetEnemy.transform.position, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);
        }
        else
        {

            StartCoroutine(AttackEnemy());
        }
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("pest"))
        {
            animator.SetTrigger("Attack");
        }
    }

    GameObject FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("pest");
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

        while (targetEnemy != null && Vector2.Distance(rb.position, targetEnemy.transform.position) <= attackRange)
        {

            if (animator != null)
                animator.SetTrigger("Attack");

            yield return new WaitForSeconds(attackCooldown);


            if (targetEnemy != null)
            {
                Enemy enemyScript = targetEnemy.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.TakeHit();


                    if (enemyScript.Health <= 0)
                    {
                        Destroy(targetEnemy);
                        totalKills++;
                        targetEnemy = null;


                        if (totalKills >= killsBeforeDeath)
                        {
                            Destroy(gameObject);
                            yield break;
                        }
                    }
                }
            }
        }

        isAttacking = false;
    }
}