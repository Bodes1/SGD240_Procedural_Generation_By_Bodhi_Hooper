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

    // Ore generation
    [Range(0f, 1f)]
    public float oreSpawnChance = 0.1f;
    public Int32 maxOreClusterSize = 3;

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
        GameObject chunkObject = new GameObject($"Chunk_{chunkX}_{chunkZ}"); // Create a parent GameObject for the chunk
        chunkObject.transform.position = new Vector3(chunkX * chunkSize * cubeSize, 0, chunkZ * chunkSize * cubeSize);

        // Separate lists for each material type
        List<CombineInstance> grassCombineInstances = new List<CombineInstance>();
        List<CombineInstance> waterCombineInstances = new List<CombineInstance>();
        List<CombineInstance> stoneCombineInstances = new List<CombineInstance>();
        List<CombineInstance> woodCombineInstances = new List<CombineInstance>();
        List<CombineInstance> leafCombineInstances = new List<CombineInstance>();
        List<CombineInstance> coalCombineInstances = new List<CombineInstance>();
        List<CombineInstance> ironCombineInstances = new List<CombineInstance>();

        HashSet<Vector3> placedPositions = new HashSet<Vector3>(); // Track placed blocks

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
                    Vector3 blockPosition = new Vector3(worldX * cubeSize, y * cubeSize, worldZ * cubeSize);

                    if (y <= terrainHeight)
                    {
                        if (y < waterLevel)
                        {
                            AddCubeToMesh(blockPosition, waterCombineInstances);
                        }
                        else if (y < stoneHeightThreshold)
                        {
                            AddCubeToMesh(blockPosition, grassCombineInstances);
                        }
                        else
                        {
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
    }

    void AddCubeToMesh(Vector3 position, List<CombineInstance> combineInstances)
    {
        // Create a temporary cube object to extract its mesh data
        GameObject tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tempCube.transform.position = position;
        tempCube.transform.localScale = Vector3.one * cubeSize;

        // Add to combine instances
        CombineInstance combineInstance = new CombineInstance
        {
            mesh = tempCube.GetComponent<MeshFilter>().mesh,
            transform = tempCube.transform.localToWorldMatrix
        };
        combineInstances.Add(combineInstance);

        Destroy(tempCube);  // Remove the temporary cube
    }

    void CreateCombinedMesh(GameObject parent, List<CombineInstance> combineInstances, Material material)
    {
        if (combineInstances.Count == 0) return; // Skip if no cubes of this type

        Mesh combinedMesh = new Mesh
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 // Set to UInt32 to support large vertex counts
        };
        combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true);

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

        // Place 3 cubes as the tree trunk
        for (Int32 i = 0; i < woodPlaced; i++)
        {
            AddCubeToMesh(new Vector3(x * cubeSize, (y + i) * cubeSize, z * cubeSize), woodCombineInstances);
        }

        // Add a green "leaf" cube at the top
        AddCubeToMesh(new Vector3(x * cubeSize, (y + woodPlaced) * cubeSize, z * cubeSize), leafCombineInstances);
    }

    void PlaceOreCluster(Int32 x, Int32 y, Int32 z, List<CombineInstance> coalCombineInstances, List<CombineInstance> ironCombineInstances, HashSet<Vector3> placedPositions)
    {
        Int32 clusterSize = UnityEngine.Random.Range(1, maxOreClusterSize + 1);

        for (Int32 i = 0; i < clusterSize; i++)
        {
            // Random offset for clustered ores
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

    // Draw chunk outline
    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    for (Int32 chunkX = 0; chunkX < worldSizeInChunks; chunkX++)
    //    {
    //        for (Int32 chunkZ = 0; chunkZ < worldSizeInChunks; chunkZ++)
    //        {
    //            Vector3 chunkPosition = new Vector3(chunkX * chunkSize * cubeSize, 0, chunkZ * chunkSize * cubeSize);
    //            Gizmos.DrawWireCube(chunkPosition + new Vector3(chunkSize * cubeSize / 2, worldHeight * cubeSize / 2, chunkSize * cubeSize / 2),
    //                                new Vector3(chunkSize * cubeSize, worldHeight * cubeSize, chunkSize * cubeSize));
    //        }
    //    }
    //}
}