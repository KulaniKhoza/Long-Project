using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Fence_Health : MonoBehaviour
{

    public int Health = 100;

    public Slider HealthBar;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        HealthBar.value = Health;

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
            Debug.Log(Health);

        }
        //Health--;
    }
}
