using System;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEditor.Overlays;
using UnityEngine;

public class CubeProceduralGeneration : MonoBehaviour
{
    // Public material fields to assign in the Unity Editor
    public Material waterMaterial;
    public Material grassMaterial;
    public Material stoneMaterial;
    public Material woodMaterial;
    public Material leafMaterial;
    public Material coalMaterial;
    public Material ironMaterial;
    public Material redFlowerMaterial;
    public Material yellowFlowerMaterial;
    public Material purpleFlowerMaterial;

    // Chunk size and settings
    public Int32 chunkSize = 16; // Number of blocks along each side of a chunk
    public Int32 worldHeight = 16; // Maximum height of terrain
    public Int32 worldSizeInChunks = 4; // Number of chunks along each axis in the world

    // Noise and height settings
    public float cubeSize = 1.0f; // Size of each cube
    public float heightMultiplier = 5f; // Multiplier for terrain height based on noise
    public float noiseScale = 0.1f; // Scale for Perlin noise used in terrain generation
    public Int32 waterLevel = 4; // Height below which water will be placed
    public Int32 stoneHeightThreshold = 12; // Height above which stone terrain will be generated

    // Tree generation
    [Range(0f, 1f)]
    public float treeSpawnChance = 0.1f; // Chance for trees to spawn on grass blocks

    // Flower generation
    [Range(0f, 1f)]
    public float flowerSpawnChance = 0.1f; // Chance for flowers to spawn on grass blocks
    private Vector3 flowerScale = new Vector3(0.5f, 1f, 0.5f); // Scale for flower cubes: half-width, full height

    // Ore generation
    [Range(0f, 1f)]
    public float oreSpawnChance = 0.1f; // Chance for ore clusters to spawn on stone blocks
    public Int32 maxOreClusterSize = 3; // Maximum size of an ore cluster

    // Seed for random generation
    public Int32 seed; // Random seed to ensure repeatable terrain generation

    void Start()
    {
        // Initialize random seed if not set
        if (seed == 0)
            seed = UnityEngine.Random.Range(1, 100000);

        GenerateWorld(); // Start world generation
    }

    void GenerateWorld()
    {
        // Generate chunks in a grid based on world size in chunks
        for (Int32 chunkX = 0; chunkX < worldSizeInChunks; chunkX++)
        {
            for (Int32 chunkZ = 0; chunkZ < worldSizeInChunks; chunkZ++)
            {
                GenerateChunk(chunkX, chunkZ); // Generate each chunk
            }
        }
    }

    void GenerateChunk(Int32 chunkX, Int32 chunkZ)
    {
        // Create a parent GameObject for the chunk to organize all its blocks
        GameObject chunkObject = new GameObject($"Chunk_{chunkX}_{chunkZ}"); 
        chunkObject.transform.position = new Vector3(chunkX * chunkSize * cubeSize, 0, chunkZ * chunkSize * cubeSize);

        // Separate lists for each material type
        List<CombineInstance> grassCombineInstances = new List<CombineInstance>();
        List<CombineInstance> waterCombineInstances = new List<CombineInstance>();
        List<CombineInstance> stoneCombineInstances = new List<CombineInstance>();
        List<CombineInstance> woodCombineInstances = new List<CombineInstance>();
        List<CombineInstance> leafCombineInstances = new List<CombineInstance>();
        List<CombineInstance> coalCombineInstances = new List<CombineInstance>();
        List<CombineInstance> ironCombineInstances = new List<CombineInstance>();
        List<CombineInstance> redFlowerCombineInstances = new List<CombineInstance>();
        List<CombineInstance> yellowFlowerCombineInstances = new List<CombineInstance>();
        List<CombineInstance> purpleFlowerCombineInstances = new List<CombineInstance>();

        // Track placed blocks to prevent overlapping
        HashSet<Vector3> placedPositions = new HashSet<Vector3>(); // Track placed blocks

        // Loop through each block in the chunk
        for (Int32 x = 0; x < chunkSize; x++)
        {
            for (Int32 z = 0; z < chunkSize; z++)
            {
                // Calculate world position for each block
                Int32 worldX = x + (chunkX * chunkSize);
                Int32 worldZ = z + (chunkZ * chunkSize);

                // Apply noise to determine terrain height at this position
                float perlinValue = Mathf.PerlinNoise((worldX + seed) * noiseScale, (worldZ + seed) * noiseScale);
                Int32 terrainHeight = Mathf.FloorToInt(perlinValue * heightMultiplier);

                // Generate blocks up to terrain height
                for (Int32 y = 0; y < worldHeight; y++)
                {
                    Vector3 blockPosition = new Vector3(worldX * cubeSize, y * cubeSize, worldZ * cubeSize);

                    if (y <= terrainHeight)
                    {
                        if (y < waterLevel)
                        {
                            // Place water below water level
                            AddCubeToMesh(blockPosition, waterCombineInstances);
                        }
                        else if (y < stoneHeightThreshold)
                        {
                            // Place grass blocks and allow for trees and flowers
                            AddCubeToMesh(blockPosition, grassCombineInstances);
                            placedPositions.Add(blockPosition); // Mark grass position

                            // Spawn flowers on top of grass blocks with a certain probability
                            if (y == terrainHeight && UnityEngine.Random.value < flowerSpawnChance)
                            {
                                PlaceFlower(worldX, y + 1, worldZ, redFlowerCombineInstances, yellowFlowerCombineInstances, purpleFlowerCombineInstances);
                            }
                        }
                        else
                        {
                            // Place stone blocks and ores
                            AddCubeToMesh(blockPosition, stoneCombineInstances);
                            placedPositions.Add(blockPosition); // Track stone position

                            // Check for ore spawning on stone blocks
                            if (UnityEngine.Random.value < oreSpawnChance)
                            {
                                PlaceOreCluster(worldX, terrainHeight + 1, worldZ, coalCombineInstances, ironCombineInstances, placedPositions);
                            }
                        }
                    }
                    else if (y < waterLevel)
                    {
                        // Place water below terrain level up to water height
                        AddCubeToMesh(blockPosition, waterCombineInstances);
                    }
                }

                // Place trees on grass blocks
                if (terrainHeight >= waterLevel && terrainHeight < stoneHeightThreshold && UnityEngine.Random.value < treeSpawnChance)
                {
                    PlaceTree(worldX, terrainHeight + 1, worldZ, woodCombineInstances, leafCombineInstances);
                }
            }
        }

        // Combine and apply materials for each block type
        CreateCombinedMesh(chunkObject, grassCombineInstances, grassMaterial);
        CreateCombinedMesh(chunkObject, waterCombineInstances, waterMaterial);
        CreateCombinedMesh(chunkObject, stoneCombineInstances, stoneMaterial);
        CreateCombinedMesh(chunkObject, woodCombineInstances, woodMaterial);
        CreateCombinedMesh(chunkObject, leafCombineInstances, leafMaterial);
        CreateCombinedMesh(chunkObject, coalCombineInstances, coalMaterial);
        CreateCombinedMesh(chunkObject, ironCombineInstances, ironMaterial);
        CreateCombinedMesh(chunkObject, redFlowerCombineInstances, redFlowerMaterial);
        CreateCombinedMesh(chunkObject, yellowFlowerCombineInstances, yellowFlowerMaterial);
        CreateCombinedMesh(chunkObject, purpleFlowerCombineInstances, purpleFlowerMaterial);
    }

    void AddCubeToMesh(Vector3 position, List<CombineInstance> combineInstances)
    {
        // Temporary cube for mesh extraction
        GameObject tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tempCube.transform.position = position;
        tempCube.transform.localScale = Vector3.one * cubeSize;

        // Store cube mesh for combining
        CombineInstance combineInstance = new CombineInstance
        {
            mesh = tempCube.GetComponent<MeshFilter>().mesh,
            transform = tempCube.transform.localToWorldMatrix
        };
        combineInstances.Add(combineInstance);

        Destroy(tempCube);  // Cleanup temporary object
    }

    void CreateCombinedMesh(GameObject parent, List<CombineInstance> combineInstances, Material material)
    {
        if (combineInstances.Count == 0) return; // Skip empty combine lists

        // Create combined mesh
        Mesh combinedMesh = new Mesh
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 // Large vertex support
        };
        combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true);

        // Create GameObject for combined mesh and apply material
        GameObject combinedObject = new GameObject(material.name + "_Mesh");
        combinedObject.transform.parent = parent.transform;

        MeshFilter meshFilter = combinedObject.AddComponent<MeshFilter>();
        meshFilter.mesh = combinedMesh;

        MeshRenderer renderer = combinedObject.AddComponent<MeshRenderer>();
        renderer.material = material;
    }

    void PlaceTree(Int32 x, Int32 y, Int32 z, List<CombineInstance> woodCombineInstances, List<CombineInstance> leafCombineInstances)
    {
        Int32 woodPlaced = UnityEngine.Random.Range(2, 5);

        // Build tree trunk with 2-4 wood cubes
        for (Int32 i = 0; i < woodPlaced; i++)
        {
            AddCubeToMesh(new Vector3(x * cubeSize, (y + i) * cubeSize, z * cubeSize), woodCombineInstances);
        }

        // Add leaf at top of tree
        AddCubeToMesh(new Vector3(x * cubeSize, (y + woodPlaced) * cubeSize, z * cubeSize), leafCombineInstances);
    }

    void PlaceFlower(int x, int y, int z, List<CombineInstance> redFlowerCombineInstances, List<CombineInstance> yellowFlowerCombineInstances, List<CombineInstance> purpleFlowerCombineInstances)
    {
        Vector3 flowerPosition = new Vector3(x * cubeSize, y * cubeSize, z * cubeSize);

        // Temporary cube for flower mesh
        GameObject tempFlower = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tempFlower.transform.position = flowerPosition;
        tempFlower.transform.localScale = flowerScale;

        // Randomize flower color and add to respective list
        float randomValue = UnityEngine.Random.value;
        CombineInstance combineInstance = new CombineInstance
        {
            mesh = tempFlower.GetComponent<MeshFilter>().mesh,
            transform = tempFlower.transform.localToWorldMatrix
        };

        if (randomValue < 0.33f)
            redFlowerCombineInstances.Add(combineInstance);
        else if (randomValue < 0.66f)
            yellowFlowerCombineInstances.Add(combineInstance);
        else
            purpleFlowerCombineInstances.Add(combineInstance);

        Destroy(tempFlower);  // Cleanup
    }

    void PlaceOreCluster(Int32 x, Int32 y, Int32 z, List<CombineInstance> coalCombineInstances, List<CombineInstance> ironCombineInstances, HashSet<Vector3> placedPositions)
    {
        Int32 clusterSize = UnityEngine.Random.Range(1, maxOreClusterSize + 1);

        for (Int32 i = 0; i < clusterSize; i++)
        {
            // Random offset for cluster placement
            Int32 offsetX = UnityEngine.Random.Range(-1, 2);
            Int32 offsetY = UnityEngine.Random.Range(-1, 2);
            Int32 offsetZ = UnityEngine.Random.Range(-1, 2);

            Vector3 orePosition = new Vector3((x + offsetX) * cubeSize, (y + offsetY) * cubeSize, (z + offsetZ) * cubeSize);

            // Check if this position is already used and if there is a stone block below it
            Vector3 belowPosition = new Vector3(orePosition.x, orePosition.y - cubeSize, orePosition.z);

            if (!placedPositions.Contains(orePosition) && placedPositions.Contains(belowPosition))
            {

                // Decide if this ore block should be coal or iron
                if (UnityEngine.Random.value < 0.9f) // 90% chance for coal
                {
                    AddCubeToMesh(orePosition, coalCombineInstances);
                }
                else // 10% chance for iron
                {
                    AddCubeToMesh(orePosition, ironCombineInstances);
                }

                // Mark this position as occupied to avoid overlap
                placedPositions.Add(orePosition);
            }
        }
    }
}
