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
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Crops"))
        {
            Crops crop = other.GetComponent<Crops>();
            if (crop != null)
            {
                crop.TakeDamage(1); // This will automatically play the damage effect

                // You can also check health status
                if (crop.GetHealthStatus() == Crops.HealthStatus.Critical)
                {
                    // Enemy might prioritize critically damaged crops
                }
            }
        }
    }
}