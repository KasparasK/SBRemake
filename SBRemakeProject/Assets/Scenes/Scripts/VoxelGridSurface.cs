using UnityEngine;
using System.Collections.Generic;

public class VoxelGridSurface : MonoBehaviour
{

    private Mesh mesh;

    private List<Vector3> vertices;
    private List<int> triangles;

    private int[] rowCacheMax, rowCacheMin;
    private int edgeCacheMin, edgeCacheMax;

    public void Initialize(int resolution)
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "VoxelGridSurface Mesh";
        vertices = new List<Vector3>();
        triangles = new List<int>();
        rowCacheMax = new int[resolution * 2 + 1];
        rowCacheMin = new int[resolution * 2 + 1];
    }

}

