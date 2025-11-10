using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Fence_Health : MonoBehaviour
{
    public int Health;
    public Image HealthBarImage; // Changed from Slider to Image

    void Start()
    {
        Health = 6;
    }

    void Update()
    {
        // Update the health bar image fill amount
        if (HealthBarImage != null)
        {
            HealthBarImage.fillAmount = Health / 6f;
        }

        if (Health <= 0)
        {
            Destroy(gameObject, 1f);
            //gameObject.SetActive(false);
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Enemy" && Health >= 0)
        {
            Health--;
            //Debug.Log(Health);
        }
    }
}