using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

//Creates the mesh that fits the bounds of Point1 and Point2
public class FogOfWarMeshMaker : MonoBehaviour
{
    [SerializeField] private Transform Point1;
    [SerializeField] private Transform Point2;

    void Start()
    {
        float3 dimensions = math.abs(Point2.transform.position - Point1.transform.position);
        
        GetComponent<MeshFilter>().mesh = CreateFOWMesh( math.max(dimensions.x, dimensions.y) );
    }

    private Mesh CreateFOWMesh( float maxDimension )
    {
        float width = maxDimension / 2;
        float height = maxDimension / 2;
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-width, -height, 0),
            new Vector3(width, -height, 0),
            new Vector3(-width, height, 0),
            new Vector3(width, height, 0)
        };
        
        int[] tris = new int[6]
        {
            // lower left triangle
            0, 2, 1,
            // upper right triangle
            2, 3, 1
        };
        
        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        
        
        
        
        Mesh mesh = new Mesh();

        
        //if # of vertices is larger than the buffer, mesh will break
        //mesh.indexFormat = IndexFormat.UInt32;
        
        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.uv = uv;
        mesh.RecalculateNormals();

        return mesh;
    }
    
}
