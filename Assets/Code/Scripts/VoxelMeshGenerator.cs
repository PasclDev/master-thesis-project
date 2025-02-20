using System.Collections.Generic;
using UnityEngine;

public class VoxelMeshGenerator : MonoBehaviour
{
    public GameObject grabbableBlankPrefab;

    public void GenerateMesh(LevelData currentLevelData)
    {
        
        float voxelSize = currentLevelData.voxelSize;

        Vector3Int gridSize = new Vector3Int(
            currentLevelData.fillable.size[0], 
            currentLevelData.fillable.size[1], 
            currentLevelData.fillable.size[2]
        );

        Vector3 gridCenter = (Vector3)gridSize  * 0.5f;

        foreach (var grabbable in currentLevelData.grabbables)
        {
            Vector3 grabbablePosition = new Vector3(grabbable.position[0], grabbable.position[1], grabbable.position[2]);
            Vector3 position = grabbablePosition * currentLevelData.voxelSize;
            GameObject grabbableObject = Instantiate(grabbableBlankPrefab, position, Quaternion.identity, transform);
            MeshFilter meshFilter = grabbableObject.GetComponent<MeshFilter>();
            MeshCollider meshCollider = grabbableObject.GetComponent<MeshCollider>();
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    for (int z = 0; z < gridSize.z; z++)
                    {
                        int index = x + gridSize.x * (y + gridSize.y * z);
                        if (grabbable.voxels[index] == 1)
                        {
                            Vector3 voxelPosition = (new Vector3(x, y, z)- gridCenter) * voxelSize;
                            AddVoxelMesh(voxelPosition, voxelSize, vertices, triangles, uvs);
                        }
                    }
                }
            }
            ApplyMesh(meshFilter, meshCollider, vertices, triangles, uvs);
        }
    }

    private (List<Vector3> vertices, List<Vector2> uvs, List<int> triangles) AddVoxelMesh(Vector3 position, float voxelSize, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        int startIndex = vertices.Count;

        Vector3[] cubeVertices = {
            new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0),
            new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1)
        };

        int[] cubeTriangles = {
            0, 2, 1, 0, 3, 2, // Front
            5, 6, 4, 6, 7, 4, // Back
            3, 7, 2, 7, 6, 2, // Top
            0, 1, 4, 1, 5, 4, // Bottom
            1, 2, 5, 2, 6, 5, // Right
            0, 4, 3, 3, 4, 7  // Left
        };

        for (int i = 0; i < cubeVertices.Length; i++)
        {
            vertices.Add(cubeVertices[i] * voxelSize + position);
            uvs.Add(new Vector2(cubeVertices[i].x, cubeVertices[i].y));
        }

        for (int i = 0; i < cubeTriangles.Length; i++)
        {
            triangles.Add(startIndex + cubeTriangles[i]);
        }
        return (vertices, uvs, triangles);
    }

    private void ApplyMesh(MeshFilter meshFilter, MeshCollider meshCollider, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }
}
