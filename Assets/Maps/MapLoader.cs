using System.IO;
using UnityEngine;
using System;

public class MapLoader : MonoBehaviour
{
    public string mapFilePath = "/Maps/fundaNoWalls.txt";
    public GameObject wallPrefab;
    public GameObject cornerPrefab;
    public GameObject doorPrefab;
    public GameObject windowPrefab;
    public GameObject stairsPrefab;
    public GameObject holePrefab;
    public GameObject polePrefab;
    public GameObject columnPrefab;
    public GameObject floorPrefab;

    public GameObject floor_9Prefab;

    void CreateFloor(int xSize, int zSize, int nfloor)
    {

        // for (int i = nfloor - 1; i >= 0; i--)
        // {
        //     for (int x = 0; x < xSize; x++)
        //     {
        //         for (int z = 0; z < zSize; z++)
        //         {
        //             // Vector2 pos = new Vector2(x, z);
        //             // GameObject newObject = Instantiate(floorPrefab, pos, Quaternion.identity);
        //             // newObject.transform.SetParent(gameObject.transform);
        //         }
        //     }
        // }

        GameObject floor_9 = Instantiate(floor_9Prefab, new Vector2(xSize/2, zSize/2), Quaternion.identity);
        SpriteRenderer sprite = floor_9.GetComponent<SpriteRenderer>();
        sprite.size = new Vector2(xSize, zSize);
        floor_9.transform.SetParent(gameObject.transform);
    }

    private void Start()
    {
        // Read the text file
        //string mapData = File.ReadAllText(mapFilePath);

        int nfloor, x, z;
        char c;

        StreamReader pfile;
        //string filename = mapFilePath;
        string filename = Path.Combine(Application.persistentDataPath, mapFilePath);
        pfile = new StreamReader(filename);

        var parts = pfile.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        x = Int32.Parse(parts[0]);
        z = Int32.Parse(parts[1]);
        nfloor = Int32.Parse(parts[2]);

        CreateFloor(x, z, nfloor);

        for (int i = nfloor - 1; i >= 0; i--)
        {
            for (int j = 0; j < z; j++)
            {
                for (int k = 0; k < x; k++)
                {
                    c = (char)pfile.Read();
                    // Debug.Log(c);
                    if (c == '+')
                    {
                        // Create a corner object
                        GameObject corner = Instantiate(cornerPrefab, new Vector2(k, j), Quaternion.identity);  
                        corner.transform.SetParent(gameObject.transform);
                    }

                    else if (c == '|')
                    {
                        // Create a wall object
                        GameObject wall = Instantiate(wallPrefab, new Vector2(k, j + (float)0.0), Quaternion.identity);
                        //if (x > 0 && line[x - 1] == '-')
                        //{
                            // Set the rotation of the wall object
                            wall.transform.rotation = Quaternion.Euler(0, 0, 90);
                            wall.transform.SetParent(gameObject.transform);
                        //}
                    }
                    else if (c == '-')
                    {
                        // Create a wall object
                        GameObject wall = Instantiate(wallPrefab, new Vector2(k+ (float)0.0, j), Quaternion.identity);
                        // Set the rotation of the wall object
                        wall.transform.rotation = Quaternion.Euler(0, 0, 0);
                        wall.transform.SetParent(gameObject.transform);
                    }

                    else if (c == '#')
                    {
                        //Create window object
                        GameObject window = Instantiate(windowPrefab, new Vector2(k, j), Quaternion.identity);
                        // Set the rotation of the window object
                        window.transform.rotation = Quaternion.Euler(0, 0, 0);
                        window.transform.SetParent(gameObject.transform);
                    }

                    else if (c == 'n')
                    {
                        // Create a door object
                        GameObject door = Instantiate(doorPrefab, new Vector2(k, j), Quaternion.identity);

                        // Set the rotation of the door object
                        door.transform.rotation = Quaternion.Euler(0, 0, 0);
                        door.transform.SetParent(gameObject.transform);
                    }

                    else if (c == 'c')
                    {
                        // Create a door object
                        GameObject door = Instantiate(doorPrefab, new Vector2(k, j), Quaternion.identity);

                        // Set the rotation of the door object
                        door.transform.rotation = Quaternion.Euler(0, 0, 90);
                        door.transform.SetParent(gameObject.transform);
                    }

                    else if (c == '<' || c == '>' || c == '^' || c == 'v')
                    {
                        // TODO : FIX STAIRS
                        GameObject stairs = Instantiate(stairsPrefab, new Vector2(k, j), Quaternion.identity);
                        stairs.transform.SetParent(gameObject.transform);
                    }

                    else if (c == 'o')
                    {
                        GameObject hole = Instantiate(holePrefab, new Vector2(k, j), Quaternion.identity);
                        hole.transform.SetParent(gameObject.transform);
                    }

                    else if (c == '.')
                    {
                        GameObject pole = Instantiate(polePrefab, new Vector2(k, j), Quaternion.identity);
                        pole.transform.SetParent(gameObject.transform);
                    }

                    else if (c == 'x')
                    {
                        GameObject column = Instantiate(columnPrefab, new Vector2(k, j), Quaternion.identity);
                        column.transform.SetParent(gameObject.transform);
                    }

                    else if (c == ' ')
                    {
                        //Do nothing since floor is already laid out?
                    }
                    else
                    {
                        k--;
                    }
                }
                c = (char)pfile.Read();
            }
        }
    }
}