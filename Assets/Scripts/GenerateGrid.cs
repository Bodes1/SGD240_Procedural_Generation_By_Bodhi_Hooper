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

    // Defind world size
    private int worldSizeX = 40;
    private int worldSizeZ = 40;

    // Height veriation
    private int noiseHeight = 5;

    // Space between cubes
    private float gridOffset = 1.1f;

    // List to know where the cubes positions are
    private List<Vector3> blockPositions = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        // Looping til 'x' and 'z' match world size
        for (int x = 0; x < worldSizeX; x++) 
        {
            for (int z = 0; z < worldSizeZ; z++)
            {
                Vector3 pos = new Vector3 (x * gridOffset, generateNoise(x, z, 8f) * noiseHeight, z * gridOffset);

                GameObject block = Instantiate (blockGameObject, pos, Quaternion.identity) as GameObject;

                blockPositions.Add(block.transform.position);

                block.transform.SetParent(this.transform);
            }
        }
        SpawnObject();
    }

    // Amount of objects to spawn
    private void SpawnObject()
    {
        for(int i = 0; i < 20;  i++) 
        {
            GameObject toPlaceObject = Instantiate(objectToSpawn, ObjectSpawnLocation(), Quaternion.identity);
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
