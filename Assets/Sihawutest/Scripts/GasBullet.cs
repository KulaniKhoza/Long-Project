using UnityEngine;

public class GasBullet : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float bulletDamage = 1f;
    public string enemyTag = "Enemy";
    public GameObject hitEffect;

    private Vector2 moveDirection;
    private float moveSpeed;
    private PesticideAttacker pesticideOwner;
    private Rigidbody2D bulletRigidbody;

    public void Initialize(Vector2 direction, float speed, PesticideAttacker owner)
    {
        moveDirection = direction;
        moveSpeed = speed;
        pesticideOwner = owner;
        bulletRigidbody = GetComponent<Rigidbody2D>();

        // Flip sprite based on direction
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction.x < 0;
        }

        // Start moving using linearVelocity (newer Unity version)
        if (bulletRigidbody != null)
        {
            bulletRigidbody.linearVelocity = direction * speed;
        }

        // Auto-destroy after 5 seconds to prevent infinite travel
        Destroy(gameObject, 5f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(enemyTag))
        {
            Debug.Log($"Gas bullet hit: {collision.gameObject.name}");

            // Apply damage to enemy
            //EnemyMovement enemy = collision.GetComponent<EnemyMovement>();

            Move_enemy enemy = collision.GetComponent<Move_enemy>();

            if (enemy != null)
            {
                //enemy.currentHits += (int)bulletDamage;
                //Debug.Log($"Enemy hit! Current hits: {enemy.currentHits}/{enemy.hitsToDie}");
                StartCoroutine(enemy.DamageEffect2());

                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                Color c = sr.color;
                c.a = 0f;        // Set alpha (opacity) to 0
                sr.color = c;


                // Check if enemy dies
                if (enemy.Health <= 1)
                {
                    //Destroy(collision.gameObject);
                    pesticideOwner?.OnEnemyKilled();
                    Debug.Log("Enemy destroyed by gas bullet");
                }
            }

            // Play hit effect
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }

            // Destroy projectile
            Destroy(gameObject, 0.25f);
        }
    }
}