using UnityEngine;

public class Move_enemy : MonoBehaviour
{

    public Transform target;   // The thing enemies should move toward
    public float speed = 2f;   // Movement speed
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            // Move enemy toward target
            Vector2 direction = (target.position - transform.position).normalized;
            transform.position += (Vector3)direction * speed * Time.deltaTime;
        }
    }
}
