using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralCubeGeneration : MonoBehaviour
{
    // Chunk size
    public int chunkSize = 16;
    public int worldHeight = 16;

    // Number of chunks in the world (along X and Z axes)
    public int worldSizeInChunks = 4;

    // Cube size
    public float cubeSize = 1.0f;

    // Controls terrain height variation
    public float heightMultiplier = 5f;
    public float noiseScale = 0.1f;

    // Thresholds for block types
    public int waterLevel = 4;  // Y level for water
    public int stoneHeightThreshold = 12;  // Y level for stone generation

    void Start()
    {
        GenerateWorld();
    }

    void GenerateWorld()
    {
        for (int chunkX = 0; chunkX < worldSizeInChunks; chunkX++)
        {
            for (int chunkZ = 0; chunkZ < worldSizeInChunks; chunkZ++)
            {
                GenerateChunk(chunkX, chunkZ);
            }
        }
    }

    void GenerateChunk(int chunkX, int chunkZ)
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                // Calculate world position for each cube
                int worldX = x + (chunkX * chunkSize);
                int worldZ = z + (chunkZ * chunkSize);

                // Generate height using Perlin noise
                float perlinValue = Mathf.PerlinNoise(worldX * noiseScale, worldZ * noiseScale);
                int terrainHeight = Mathf.FloorToInt(perlinValue * heightMultiplier);

                // Generate vertical cubes (like a column)
                for (int y = 0; y < terrainHeight; y++)
                {
                    if (y <= terrainHeight)
                    {
                        // Determine block type based on height
                        if (y < waterLevel)
                        {
                            // Below water level: Generate water
                            CreateCube(worldX, y, worldZ, Color.blue);  // Blue for water
                        }
                        else if (y < stoneHeightThreshold)
                        {
                            // Below stone height threshold: Generate dirt
                            CreateCube(worldX, y, worldZ, Color.green);  // Green for dirt
                        }
                        else
                        {
                            // Above stone height threshold: Generate stone
                            CreateCube(worldX, y, worldZ, Color.gray);  // Gray for stone
                        }
                    }
                }
            }
        }
    }

    void CreateCube(int x, int y, int z, Color blockColor)
    {
        // Create a new cube at the specified position
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(x * cubeSize, y * cubeSize, z * cubeSize);

        // Set the cube's color
        Renderer cubeRenderer = cube.GetComponent<Renderer>();
        cubeRenderer.material.color = blockColor;
    }
}
