using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshController : MonoBehaviour
{
    [SerializeField, Range(1.5f, 5f)]
    private float radius = 2f;

    [SerializeField, Range(0.5f, 5f)]
    private float deformationStength = 2f;

    private Mesh mesh;

    private Vector3[] verticies, modifiedVerts;


    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponentInChildren<MeshFilter>().mesh;

        verticies = mesh.vertices;

        modifiedVerts = mesh.vertices;
    }

    private void RecalcuateMesh()
    {
        mesh.vertices = modifiedVerts;

        // Hope this fixes its self
        //GetComponentInChildren<MeshCollider>().sharedMesh = mesh;

        mesh.RecalculateNormals();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
