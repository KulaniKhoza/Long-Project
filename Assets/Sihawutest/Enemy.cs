using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 4;    // Hits needed before death
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    // Called by Attacker script when hitting this enemy
    public void TakeHit()
    {
        currentHealth--;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
