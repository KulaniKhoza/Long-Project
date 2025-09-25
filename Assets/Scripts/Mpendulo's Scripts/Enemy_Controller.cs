using UnityEngine;

public class Enemy_Controller : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject EnemytoClone;
    public Transform player;
    public float speed = 2f;   // Movement speed

    void Start()
    {
        Spawn1();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Spawn1()
    {

        //int rand = Random.Range(0, enemyPatterns.Length);
        for (int i = 0; i < 4; i++)
        {

            GameObject TheClone = Instantiate(EnemytoClone);
            TheClone.SetActive(true);
            Vector3 ClonePos = new Vector2();

            ClonePos.y = GenerateRandomY(Random.Range(0, 2));
            ClonePos.x = GenerateRandomX(Random.Range(0, 2));
            TheClone.transform.position = ClonePos;

            TheClone.GetComponent<Move_enemy>().target = player;

            Vector2 direction = (player.position - TheClone.transform.position).normalized;
            TheClone.GetComponent<Rigidbody2D>().linearVelocity = (Vector3)direction * speed;

        }
    }

    public int GenerateRandomX(int region)
    {
        float left = -8;
        float right = 8;

        int num = 0;


        if (region == 1)
        {
            num = (int)Random.Range(2f, right);
        }
        else
        {
            num = (int)Random.Range(left, -2f);
        }

        return num;

    }

    public int GenerateRandomY(int region)
    {
        int Up = -4;
        int Down = 4;

        int num = 0;


        if (region == 1)
        {
            num = (int)Random.Range(2, Up);
        }
        else
        {
            num = (int)Random.Range(Down, -2);
        }

        return num;

    }
}
