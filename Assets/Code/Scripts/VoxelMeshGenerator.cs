using System.Collections.Generic;
using UnityEngine;

public class VoxelMeshGenerator : MonoBehaviour
{
    public GameObject grabbableBlankPrefab;

    public void GenerateMesh(LevelData currentLevelData)
    {
        
        float voxelSize = currentLevelData.voxelSize;



        for (int i = 0; i < currentLevelData.grabbables.Count; i++)
        {
            Grabbable grabbable = currentLevelData.grabbables[i];
            Vector3Int gridSize = new Vector3Int(
            grabbable.size[0], 
            grabbable.size[1], 
            grabbable.size[2]
            );
            Vector3 gridCenter = (Vector3)gridSize  * 0.5f;
            Vector3 grabbablePosition = new Vector3(grabbable.position[0], grabbable.position[1], grabbable.position[2]);
            Vector3 position = transform.position + grabbablePosition * currentLevelData.voxelSize;
            GameObject grabbableObject = Instantiate(grabbableBlankPrefab, position, Quaternion.identity, transform);
            grabbableObject.name = "Grabbable_" + i;
            Material mat = grabbableObject.GetComponent<Renderer>().material;
            Color newColor = grabbable.GetColor();
            newColor.a = mat.color.a;
            mat.color = newColor;
            MeshFilter meshFilter = grabbableObject.GetComponent<MeshFilter>();
            grabbableObject.GetComponent<BoxCollider>().size = (Vector3)gridSize * voxelSize;
            grabbableObject.GetComponent<GrabbableManager>().Initialize(grabbable, voxelSize);
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    for (int z = 0; z < gridSize.z; z++)
                    {
                        if (grabbable.voxels[x][y][z] == 1)
                        {
                            Vector3 voxelPosition = (new Vector3(x, y, z)- gridCenter) * voxelSize;
                            AddVoxelMesh(voxelPosition, voxelSize, vertices, triangles, uvs);
                        }
                    }
                }
            }
            ApplyMesh(meshFilter, vertices, triangles, uvs);
        }
    }

    private (List<Vector3> vertices, List<Vector2> uvs, List<int> triangles) AddVoxelMesh(Vector3 position, float voxelSize, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        int startIndex = vertices.Count;

        // Needs to use 24 vertices instead of 8 to have different UVs for each face
        Vector3[] cubeVertices = {
            // Front face
            new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0),
            // Back face
            new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1),
            // Top face
            new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1), new Vector3(0, 1, 1),
            // Bottom face
            new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(0, 0, 1),
            // Right face
            new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1), new Vector3(1, 0, 1),
            // Left face
            new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 1), new Vector3(0, 0, 1)
        };

        int[] cubeTriangles = {
            0, 2, 1,  0, 3, 2,   // Front (Clockwise)
            4, 5, 6,  4, 6, 7,   // Back  (Clockwise)
            8, 11, 10,  8, 10, 9, // Top  (Clockwise)
            12, 13, 14,  12, 14, 15, // Bottom (Clockwise)
            16, 17, 18,  16, 18, 19, // Right  (Clockwise)
            20, 23, 22,  20, 22, 21  // Left  (Clockwise)
        };

        // Different UVs for each face
        Vector2[] cubeUVs = {
            // Front face
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
            // Back face
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
            // Top face
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
            // Bottom face
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
            // Right face
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
            // Left face
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1)
        };

        for (int i = 0; i < cubeVertices.Length; i++)
        {
            vertices.Add(cubeVertices[i] * voxelSize + position);
            uvs.Add(cubeUVs[i%4]);
            
        }
        for (int i = 0; i < cubeTriangles.Length; i++)
        {
            triangles.Add(startIndex + cubeTriangles[i]);
        }
        return (vertices, uvs, triangles);
    }

    private void ApplyMesh(MeshFilter meshFilter, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
}
