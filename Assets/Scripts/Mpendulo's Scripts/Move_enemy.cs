using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class Move_enemy : MonoBehaviour
{


    public Rigidbody2D pest;
    public Vector3 target;   // The thing enemies should move toward
    public float speed = 1f;   // Movement speed
    public int posX = -1;
    public int posY = -1;
    int columns;
    int Rows;
    public int Health;
    public int maxHealth = 4;

    public float lowerLimit = 4f;
    public float UpperLimit = 7f;

    public GameObject Controller;
    public Enemy_Controller enemy;
    public bool destroyed = false;
    bool shouldMove = false;
    bool DontMove = false;
    private Color originalColor;
    public SpriteRenderer spriteRenderer;

    public GameObject gameOverScreen;



    public Slider HealthBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameOverScreen.SetActive(false);

        Health = maxHealth;
        HealthBar.value = Health;

        target = transform.position;
        speed = 0.2f;

        // Initialize components
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        originalColor = spriteRenderer.color;

        pest = GetComponent<Rigidbody2D>();
        enemy = Controller.GetComponent<Enemy_Controller>();
        columns = (enemy.grid[0].Count / 2) - 1;
        Rows = (enemy.grid.Count) - 1;

        if (transform.position.x == enemy.grid[3][0].x)
        {

            posX = 0;
            StartCoroutine(MovementDelay("Left"));
            int i = 0;
            while (posY == -1)
            {
                if (enemy.grid[i][0].y == transform.position.y)
                {
                    posY = i;

                }

                i++;
            }
        }
        else if (transform.position.x == enemy.grid[3][columns].x)
        {

            posX = columns;
            StartCoroutine(MovementDelay("Right"));
            int i = 0;
            while (posY == -1)
            {
                if (enemy.grid[i][0].y == transform.position.y)
                {
                    posY = i;
                }

                i++;
            }
        }
        else if (transform.position.y == enemy.grid[Rows][3].y)
        {

            posY = Rows;
            StartCoroutine(MovementDelay("Up"));
            int i = 0;
            while (posX == -1)
            {
                if (enemy.grid[0][i].x == transform.position.x)
                {
                    posX = i;
                }

                i++;
            }
        }
        else if (transform.position.y == enemy.grid[0][3].y)
        {

            posY = 0;
            StartCoroutine(MovementDelay("Down"));
            int i = 0;
            while (posX == -1)
            {
                if (enemy.grid[0][i].x == transform.position.x)
                {
                    posX = i;
                }

                i++;
            }
        }


    }

    // Update is called once per frame
    void Update()
    {
        HealthBar.value = Health;

        if (posX != -1 && posX == 0 && !destroyed)
        {
            //enemy.clones.RemoveAt(0);
            for (int i = 0; i < enemy.clones.Count; i++)
            {
                if (enemy.clones[i].transform.position == transform.position)
                {
                    enemy.clones.RemoveAt(i);
                    break;
                }
            }
            StartCoroutine(destroyDelay());
            Destroy(gameObject, 7f);
            destroyed = true;

        }

        if (Health <= 0)
        {
            for (int i = 0; i < enemy.clones.Count; i++)
            {
                if (enemy.clones[i].transform.position == transform.position)
                {
                    enemy.clones.RemoveAt(i);
                    break;
                }
            }
            Destroy(gameObject, 1f);
            destroyed = true;
        }




    }

    public void FixedUpdate()
    {
        if (target != null && !DontMove)
        {
            // Move enemy toward target
            Vector2 direction = (target - transform.position).normalized;
            Vector2 direction2 = (target - transform.position);
            transform.position += (Vector3)direction * speed * Time.fixedDeltaTime;

            //Debug.Log(direction2.x);
            if (direction2.x >= -0.5 && direction2.x <= 0.2 && shouldMove)
            {
                Move("Right");
            }

        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Attacker")
        {
            StartCoroutine(DamageEffect());
            //target = enemy.grid[posY][posX + 1];
            //posX++;
        }

        if (collision.transform.tag == "Fence")
        {
            //Move("Left");
            target = enemy.grid[posY][posX + 1];
            posX++;


        }

        if (collision.transform.tag == "Crops")
        {
            Crops cropHealth = collision.gameObject.GetComponent<Crops>();

            if (cropHealth != null)
            {

                target = enemy.grid[posY][posX + 1];
                posX++;
                cropHealth.TakeDamage(1);

            }
        }

    }
    private IEnumerator MovementDelay(string direction)
    {
        //yield return new WaitForSeconds(Random.Range(lowerLimit, UpperLimit));
        yield return new WaitForSeconds(10f);
        Move(direction);
        shouldMove = true;
    }

    public void Move(string direction)
    {
        if (direction == "Left" && posX < columns)
        {
            target = enemy.grid[posY][posX + 1];
            posX++;
            //StartCoroutine(MovementDelay(direction));

        }
        else if (direction == "Right" && posX > 0)
        {

            target = enemy.grid[posY][posX - 1];
            posX--;
            //StartCoroutine(MovementDelay(direction));

        }
        else if (direction == "Up" && posY > 0)
        {
            target = enemy.grid[posY - 1][posX];
            posY--;
            //StartCoroutine(MovementDelay(direction));

        }
        else if (direction == "Down" && posY < Rows)
        {
            target = enemy.grid[posY + 1][posX];
            posY++;
            //StartCoroutine(MovementDelay(direction));

        }



    }
    private IEnumerator destroyDelay()
    {
        yield return new WaitForSeconds(6.9f);
        gameOverScreen.SetActive(true);
        Time.timeScale = 0f;


    }

    public IEnumerator DamageEffect()
    {
        DontMove = true;
        // Flash red a few times
        for (int i = 0; i < 6; i++)
        {
            spriteRenderer.color = Color.red;
            Health--;
            yield return new WaitForSeconds(0.335f);

            spriteRenderer.color = originalColor;

            yield return new WaitForSeconds(0.335f);

        }

    }



}
