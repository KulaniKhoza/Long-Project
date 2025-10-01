using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 4;    // Hits needed before death
    public int Health;

    void Start()
    {
        Health = maxHealth;
    }

    // Called by Attacker script when hitting this enemy
    public void TakeHit()
    {
        Health--;

        if (Health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}