using UnityEngine;
using System.Collections;

public class Move_enemy : MonoBehaviour
{


    public Rigidbody2D pest;
    public Transform target;   // The thing enemies should move toward
    public float speed = 2f;   // Movement speed

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pest = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            // Move enemy toward target
            //Vector2 direction = (target.position - transform.position).normalized;
            //transform.position += (Vector3)direction * speed * Time.deltaTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 direction = (target.position - transform.position).normalized;
        pest.linearVelocity = (Vector3)direction * -speed;

        StartCoroutine(ResetVelocityAfterDelay(1f));

    }
    private IEnumerator ResetVelocityAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Reset velocity
        Vector2 direction = (target.position - transform.position).normalized;
        pest.linearVelocity = (Vector3)direction * speed;
    }


}
