using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGeneration : MonoBehaviour
{
    // Chunk size and settings
    public Int32 chunkSize = 16;
    public Int32 worldHeight = 16;
    public Int32 worldSizeInChunks = 4;

    // Noise and height settings
    public float cubeSize = 1.0f;
    public float heightMultiplier = 5f;
    public float noiseScale = 0.1f;
    public Int32 waterLevel = 4;
    public Int32 stoneHeightThreshold = 12;

    // Tree generation
    [Range(0f, 1f)]
    public float treeSpawnChance = 0.1f;

    // Seed for random generation
    public Int32 seed;

    void Start()
    {
        // Initialize random seed if not set
        if (seed == 0)
            seed = UnityEngine.Random.Range(1, 100000);

        GenerateWorld();
    }

    void GenerateWorld()
    {
        for (Int32 chunkX = 0; chunkX < worldSizeInChunks; chunkX++)
        {
            for (Int32 chunkZ = 0; chunkZ < worldSizeInChunks; chunkZ++)
            {
                GenerateChunk(chunkX, chunkZ);
            }
        }
    }

    void GenerateChunk(Int32 chunkX, Int32 chunkZ)
    {
        List<CombineInstance> combineInstances = new List<CombineInstance>();
        GameObject chunkObject = new GameObject($"Chunk_{chunkX}_{chunkZ}");
        chunkObject.transform.position = new Vector3(chunkX * chunkSize * cubeSize, 0, chunkZ * chunkSize * cubeSize);

        for (Int32 x = 0; x < chunkSize; x++)
        {
            for (Int32 z = 0; z < chunkSize; z++)
            {
                // Calculate world position for each cube
                Int32 worldX = x + (chunkX * chunkSize);
                Int32 worldZ = z + (chunkZ * chunkSize);

                // Apply seed offset to the Perlin noise for randomization
                float perlinValue = Mathf.PerlinNoise((worldX + seed) * noiseScale, (worldZ + seed) * noiseScale);
                Int32 terrainHeight = Mathf.FloorToInt(perlinValue * heightMultiplier);

                // Generate terrain up to the calculated terrain height
                for (Int32 y = 0; y < worldHeight; y++)
                {
                    if (y <= terrainHeight)
                    {
                        Color blockColor = y < waterLevel ? Color.blue : (y < stoneHeightThreshold ? Color.green : Color.gray);
                        AddCubeToMesh(new Vector3(worldX * cubeSize, y * cubeSize, worldZ * cubeSize), blockColor, combineInstances);
                    }
                    else if (y < waterLevel)
                    {
                        AddCubeToMesh(new Vector3(worldX * cubeSize, y * cubeSize, worldZ * cubeSize), Color.blue, combineInstances);
                    }
                }

                // Place trees on grass blocks
                if (terrainHeight >= waterLevel && terrainHeight < stoneHeightThreshold && UnityEngine.Random.value < treeSpawnChance)
                {
                    PlaceTree(worldX, terrainHeight + 1, worldZ, combineInstances);
                }
            }
        }

        // Combine the meshes into a single chunk mesh
        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // Set to UInt32 for large meshes
        combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true);

        MeshFilter meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshFilter.mesh = combinedMesh;

        MeshRenderer renderer = chunkObject.AddComponent<MeshRenderer>();
        renderer.material = new Material(Shader.Find("Standard"));
    }

    void AddCubeToMesh(Vector3 position, Color color, List<CombineInstance> combineInstances)
    {
        // Create a temporary cube object to extract its mesh data
        GameObject tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tempCube.transform.position = position;
        tempCube.transform.localScale = Vector3.one * cubeSize;

        // Set color (this will require multiple materials if different colors are used per block type)
        Renderer tempRenderer = tempCube.GetComponent<Renderer>();
        tempRenderer.material.color = color;

        // Add to combine instances
        CombineInstance combineInstance = new CombineInstance
        {
            mesh = tempCube.GetComponent<MeshFilter>().mesh,
            transform = tempCube.transform.localToWorldMatrix
        };
        combineInstances.Add(combineInstance);

        Destroy(tempCube);  // Remove the temporary cube
    }

    void PlaceTree(Int32 x, Int32 y, Int32 z, List<CombineInstance> combineInstances)
    {
        // Tree trunk and leaf colors
        Color trunkColor = new Color(0.55f, 0.27f, 0.07f);
        Color leafColor = Color.green;

        // Place 3 cubes as the tree trunk
        for (Int32 i = 0; i < 3; i++)
        {
            AddCubeToMesh(new Vector3(x * cubeSize, (y + i) * cubeSize, z * cubeSize), trunkColor, combineInstances);
        }

        // Add a green "leaf" cube at the top
        AddCubeToMesh(new Vector3(x * cubeSize, (y + 3) * cubeSize, z * cubeSize), leafColor, combineInstances);
    }
}
