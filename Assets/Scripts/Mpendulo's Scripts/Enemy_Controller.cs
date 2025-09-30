using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Enemy_Controller : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject EnemytoClone;
    public GameObject GridObject;
    public GameObject gridOriginObject;
    //public Transform player;
    public float speed = 1f;   // Movement speed
    public List<GameObject> clones = new List<GameObject>();

    public List<List<Vector2>> grid = new List<List<Vector2>>();

    bool shouldSpawn = true;

    void Start()
    {



        int rows = GridObject.GetComponent<FarmGrid>().RowLength;
        int cols = GridObject.GetComponent<FarmGrid>().ColumnLength;

        // Build the grid with coordinates
        for (int i = 0; i < rows; i++)
        {
            grid.Add(new List<Vector2>()); // Add a new row

            for (int j = 0; j < cols; j++)
            {
                grid[i].Add(new Vector2(0, 0));
            }
        }

        // Example: Access a coordinate

        //Vector2 coord = grid[1][3];

        Vector3 gridOrigin = gridOriginObject != null ? gridOriginObject.transform.position : Vector3.zero;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Vector3 position = gridOrigin + new Vector3(col * GridObject.GetComponent<FarmGrid>().X_space, row * GridObject.GetComponent<FarmGrid>().Y_space, 0);
                grid[row].Insert(col, new Vector2(position.x, position.y));
                //Debug.Log(position);
            }
        }
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Vector2 coord = grid[row][col];
                //Debug.Log(coord);
            }
        }

        Spawn1();

    }

    // Update is called once per frame
    void Update()
    {
        if (shouldSpawn)
        {
            StartCoroutine(SpawnWaves(50f));
            shouldSpawn = false;
        }
    }

    public void Spawn1()
    {
        GameObject TheClone = Instantiate(EnemytoClone);
        TheClone.SetActive(true);

        Vector2 ClonePos = Vector2.zero;
        bool foundSpot = false;

        int maxAttempts = 20; // safety limit
        int attempts = 0;

        while (!foundSpot && attempts < maxAttempts)
        {
            attempts++;

            //int YorX = Random.Range(0, 2);
            int YorX = 1;
            //int[] choices = { 0, 5 };
            //int randomValue = choices[Random.Range(0, choices.Length)];
            int randomValue = (grid[0].Count / 2) - 1;

            if (YorX == 0)
            {
                ClonePos = grid[randomValue][GenerateRandomX()];
            }
            else
            {
                ClonePos = grid[GenerateRandomX()][randomValue];
            }

            // Check for overlap
            bool occupied = false;
            for (int i = 0; i < clones.Count; i++)
            {
                if ((Vector2)clones[i].transform.position == ClonePos)
                {
                    occupied = true;
                    break;
                }
            }

            if (!occupied) foundSpot = true;
        }

        // If no free spot found, cancel spawn
        if (!foundSpot)
        {
            Destroy(TheClone);
            Debug.LogWarning("No free spawn position found!");
            return;
        }

        // Place clone
        TheClone.transform.position = ClonePos;
        clones.Add(TheClone);

        // Assign behaviour
        //TheClone.GetComponent<Move_enemy>().target = player;

        // Schedule more spawns if under limit
        if (clones.Count < 4)
        {
            float spawnDelay = Random.Range(1f, 3f);
            StartCoroutine(SpawnEnemies(spawnDelay));
        }
    }

    public int GenerateRandomX()
    {

        int num = 0;
        float endPoint = (grid.Count) - 1;

        num = (int)Random.Range(0f, endPoint);

        return num;

    }

    private IEnumerator SpawnEnemies(float delay)
    {
        yield return new WaitForSeconds(delay);
        Spawn1();
    }

    private IEnumerator SpawnWaves(float delay)
    {
        yield return new WaitForSeconds(delay);
        Spawn1();
        shouldSpawn = true;
    }
}
