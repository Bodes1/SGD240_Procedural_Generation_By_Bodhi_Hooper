using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateGrid : MonoBehaviour
{
    // Object to act as the ground
    [SerializeField]
    private GameObject blockGameObject;

    // Object to spawn on ground
    [SerializeField]
    private GameObject objectToSpawn;

    // Player character
    [SerializeField]
    private GameObject player;

    // Defind world size
    private int worldSizeX = 10;
    private int worldSizeZ = 10;

    // Height veriation
    private int noiseHeight = 5;

    // Space between cubes
    //private float gridOffset = 1.1f;

    // Player spawn location
    private Vector3 startPosition;

    // Contains info of player disanst from spawn for the cubes
    private Hashtable blockContainer = new Hashtable();

    // List to know where the cubes positions are
    private List<Vector3> blockPositions = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        // Looping til 'x' and 'z' match world size
        for (int x = -worldSizeX; x < worldSizeX; x++) 
        {
            for (int z = -worldSizeZ; z < worldSizeZ; z++)
            {
                Vector3 pos = new Vector3 (x * 1 + startPosition.x, generateNoise(x, z, 8f) * noiseHeight, z * 1 + startPosition.z);

                GameObject block = Instantiate (blockGameObject, pos, Quaternion.identity) as GameObject;

                blockContainer.Add(pos, block);

                blockPositions.Add(block.transform.position);

                block.transform.SetParent(this.transform);
            }
        }
        SpawnObject();
    }

    // Checks if player has move further then the world size
    private void Update()
    {
        if (Mathf.Abs(xPlayerMove) >= 1 || Mathf.Abs(zPlayerMove) >= 1)
        {
            // Add to the grid around the player
            for (int x = -worldSizeX; x < worldSizeX; x++)
            {
                for (int z = -worldSizeZ; z < worldSizeZ; z++)
                {
                    // Changed 'Vector3' grid spawn from start location to player location AND generate noise around the player
                    Vector3 pos = new Vector3(x * 1 + xPlayerLocation, generateNoise(x + xPlayerLocation, z + zPlayerLocation, 8f) * noiseHeight, z * 1 + zPlayerLocation);

                    // Checks if already has a cube there
                    if (!blockContainer.ContainsKey(pos))
                    {
                        GameObject block = Instantiate(blockGameObject, pos, Quaternion.identity) as GameObject;

                        blockContainer.Add(pos, block);

                        blockPositions.Add(block.transform.position);

                        block.transform.SetParent(this.transform);

                        SpawnMoreObject();
                    }

                    
                }
            }
            
        }
        SpawnMoreObject();
    }

    // Info on the distant the player travals in (X)
    private int xPlayerMove
    {
        get
        {
            return (int)(player.transform.position.x - startPosition.x);
        }
    }

    // Info on the distant the player travals in (Z)
    private int zPlayerMove
    {
        get
        {
            return (int)(player.transform.position.z - startPosition.z);
        }
    }


    // Amount of objects to spawn
    private void SpawnObject()
    {
        for(int i = 0; i < 20;  i++) 
        {
            GameObject toPlaceObject = Instantiate(objectToSpawn, ObjectSpawnLocation(), Quaternion.identity);
        }
    }

    // Spawn more objects when the world exspans
    private void SpawnMoreObject()
    {
        int spawnChance = Random.Range(0, 11);

        if (spawnChance == 0) 
        {
            GameObject toPlaceObject = Instantiate(objectToSpawn, ObjectSpawnLocation(), Quaternion.identity);
        }
    }

    // Info on the players position (X)
    private int xPlayerLocation
    {
        get
        {
            return (int) Mathf.Floor(player.transform.position.x);
        }
    }

    // Info on the players position (Z)
    private int zPlayerLocation
    {
        get
        {
            return (int) Mathf.Floor(player.transform.position.z);
        }
    }

    // Get a random position from the grid
    private Vector3 ObjectSpawnLocation()
    {
        int randomIndex = Random.Range(0, blockPositions.Count);

        Vector3 newPos = new Vector3(blockPositions[randomIndex].x, blockPositions[randomIndex].y + 0.5f, blockPositions[randomIndex].z);

        blockPositions.RemoveAt(randomIndex);

        return newPos;
    }

    // Generating and getting noise values
    private float generateNoise(int x, int z, float detailScale)
    {
        float xNoise = (x + this.transform.position.x) / detailScale;
        float zNoise = (z + this.transform.position.y) / detailScale;

        return Mathf.PerlinNoise(xNoise, zNoise);
    }
}
