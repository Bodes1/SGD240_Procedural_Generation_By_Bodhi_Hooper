using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateGrid : MonoBehaviour
{
    public GameObject blockGameObject;

    // Defind world size
    private int worldSizeX = 40;
    private int worldSizeZ = 40;

    private int noiseHeight = 5;

    // Space between cubes
    private float gridOffset = 1.1f;

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

                block.transform.SetParent(this.transform);
            }
        }
    }

    // Generating and getting noise values
    private float generateNoise(int x, int z, float detailScale)
    {
        float xNoise = (x + this.transform.position.x) / detailScale;
        float zNoise = (z + this.transform.position.y) / detailScale;

        return Mathf.PerlinNoise(xNoise, zNoise);
    }
}
