using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


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

    public GameObject gameOverScreen;



    public Slider HealthBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameOverScreen.SetActive(false);

        Health = maxHealth;
        HealthBar.value = Health;

        target = transform.position;
        speed = 0.5f;

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
            Destroy(gameObject, 1f);
            destroyed = true;
            //gameOverScreen.SetActive(true);
            //Time.timeScale = 0f;
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

        if (target != null)
        {
            // Move enemy toward target
            Vector2 direction = (target - transform.position).normalized;
            transform.position += (Vector3)direction * speed * Time.deltaTime;

            if (direction == Vector2.zero && shouldMove)
            {
                Move("Right");
            }

        }


    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Fence")
        {
            //Move("Left");
            target = enemy.grid[posY][posX + 1];
            posX++;

            Health--;
            HealthBar.value = Health;


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

    public void move2()
    {

    }


}
