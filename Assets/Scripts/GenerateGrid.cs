using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateGrid : MonoBehaviour
{
    public GameObject blockGameObject;

    // Defind world size
    private int worldSizeX = 10;
    private int worldSizeZ = 10;

    private int gridOffset = 2;

    // Start is called before the first frame update
    void Start()
    {
        // Looping til 'x' and 'z' match world size
        for (int x = 0; x < worldSizeX; x++) 
        {
            for (int z = 0; z < worldSizeZ; z++)
            {
                Vector3 pos = new Vector3 (x * gridOffset, 0, z * gridOffset);

                GameObject block = Instantiate (blockGameObject, pos, Quaternion.identity) as GameObject;

                block.transform.SetParent(this.transform);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
