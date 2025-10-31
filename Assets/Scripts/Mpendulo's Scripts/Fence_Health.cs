using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Fence_Health : MonoBehaviour
{
    public int Health = 100;
    public Image HealthBarImage; // Changed from Slider to Image

    void Start()
    {
        
    }

    void Update()
    {
        // Update the health bar image fill amount
        if (HealthBarImage != null)
        {
            HealthBarImage.fillAmount = Health / 100f;
        }

        if (Health <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "pest" && Health >= 0)
        {
            Health--;
            //Debug.Log(Health);
        }
    }
}