using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHits = 4;
    private int currentHits;
    public bool isAttacking = false;

    public Animator animator;

    public void TakeDamage(int amount)
    {
        currentHits += amount;

        if (currentHits >= maxHits)
        {
            Die();
        }
    }

    public void StartAttack()
    {
        isAttacking = true;
        Invoke(nameof(StopAttack), 1.5f); // attack lasts 1.5 seconds, adjust to match animation
    }

    void StopAttack()
    {
        isAttacking = false;
    }

    void Die()
    {
        if (animator != null)
            animator.SetTrigger("Die");

        Destroy(gameObject, 0.5f); // small delay for death animation
    }
}