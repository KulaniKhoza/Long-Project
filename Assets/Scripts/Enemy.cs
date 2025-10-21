using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 4;    // Hits needed before death
    public int Health;

    public Slider HealthBar;


    void Start()
    {
        Health = maxHealth;
        HealthBar.value = Health;
    }

    // Called by Attacker script when hitting this enemy
    public void TakeHit()
    {

        Health--;
        HealthBar.value = Health;

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