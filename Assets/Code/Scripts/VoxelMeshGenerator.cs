using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoxelMeshGenerator : MonoBehaviour
{
    public GameObject grabbableBlankPrefab;
    public GameObject fillableBlankPrefab;

    public void GenerateGrabbableObjects(LevelData currentLevelData)
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
            Vector3 position = transform.position + grabbablePosition * voxelSize;
            GameObject grabbableObject = Instantiate(grabbableBlankPrefab, position, Quaternion.identity, transform);
            grabbableObject.name = "Grabbable_" + i;
            Material normalMaterial = grabbableObject.GetComponent<Renderer>().material;
            Material transparentMaterial = new Material(grabbableObject.GetComponent<GrabbableManager>().transparentMaterial);
            Color newColor = grabbable.GetColor();
            newColor.a = transparentMaterial.color.a;
            normalMaterial.color = newColor;
            transparentMaterial.color = newColor;
            grabbableObject.GetComponent<GrabbableManager>().transparentMaterial = transparentMaterial;
            MeshFilter meshFilter = grabbableObject.GetComponent<MeshFilter>();
            MeshCollider meshCollider = grabbableObject.GetComponent<MeshCollider>();
            //grabbableObject.GetComponent<BoxCollider>().size = (Vector3)gridSize * voxelSize;
            grabbableObject.GetComponent<GrabbableManager>().Initialize(grabbable, voxelSize);

            // Generate Mesh
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
                            AddVoxelToMesh(voxelPosition, voxelSize, vertices, triangles, uvs);
                        }
                    }
                }
            }
            Mesh mesh = GetMesh(vertices, triangles, uvs);
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;

        }
    }

    private void AddVoxelToMesh(Vector3 position, float voxelSize, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
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
    }

    public void GenerateFillableObject(LevelData currentLevelData){
        float voxelSize = currentLevelData.voxelSize;
        Fillable fillable = currentLevelData.fillable;

        GameObject fillableObject = Instantiate(fillableBlankPrefab, transform);
        Vector3 position = transform.position + voxelSize*new Vector3(fillable.position[0], fillable.position[1], fillable.position[2]); 
        Vector3Int size = new Vector3Int(fillable.size[0], fillable.size[1], fillable.size[2]);
        Vector3 gridCenter = (Vector3)size * 0.5f;
        fillableObject.name = "Fillable_0";
        fillableObject.GetComponent<FillableManager>().Initialize(position, size, voxelSize);

        // Generate Mesh
        MeshFilter meshFilter = fillableObject.GetComponent<MeshFilter>();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        // Mesh only consists of the "outer" faces of the voxel grid, not the inner faces
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    Vector3 voxelPosition = (new Vector3(x, y, z) - gridCenter) * voxelSize;
                    bool[] outerFaces = new bool[6]; // Front, Back, Top, Bottom, Right, Left
                    if (x == 0) outerFaces[5] = true;
                    if (x == size.x - 1) outerFaces[4] = true;
                    if (y == 0) outerFaces[3] = true;
                    if (y == size.y - 1) outerFaces[2] = true;
                    if (z == 0) outerFaces[0] = true;
                    if (z == size.z - 1) outerFaces[1] = true;
                    AddOuterFacesToMesh(voxelPosition, voxelSize, vertices, triangles, uvs, outerFaces);
                }
            }
        }
        meshFilter.mesh = GetMesh(vertices, triangles, uvs);
    }
    //Only add the vertices of the outer faces of the voxel grid
    private void AddOuterFacesToMesh(Vector3 position, float voxelSize, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, bool[] outerFaces){
        int startIndex = vertices.Count;
    
        List<Vector3> cubeVertices = new List<Vector3>();
        List<int> cubeTriangles = new List<int>();
        int addedFaces = 0;
        if (outerFaces[0]){
            cubeVertices.AddRange(new Vector3[]{
                new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0)});
            cubeTriangles.AddRange(new int[]{0, 2, 1,  0, 3, 2});
            addedFaces++;
        }
        if (outerFaces[1]){
            cubeVertices.AddRange(new Vector3[]{
                new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1)});
            cubeTriangles.AddRange((new int[]{0, 1, 2,  0, 2, 3}).Select(index => index + 4 * addedFaces));
            addedFaces++;
        }
        if (outerFaces[2]){
            cubeVertices.AddRange(new Vector3[]{
                new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1), new Vector3(0, 1, 1)});
            cubeTriangles.AddRange((new int[]{0, 3, 2,  0, 2, 1}).Select(index => index + 4 * addedFaces));
            addedFaces++;
        }
        if (outerFaces[3]){
            cubeVertices.AddRange(new Vector3[]{
                new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(0, 0, 1)});
            cubeTriangles.AddRange((new int[]{0, 1, 2,  0, 2, 3}).Select(index => index + 4 * addedFaces));
            addedFaces++;
        }
        if (outerFaces[4]){
            cubeVertices.AddRange(new Vector3[]{
                new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1), new Vector3(1, 0, 1)});
            cubeTriangles.AddRange((new int[]{0, 1, 2,  0, 2, 3}).Select(index => index + 4 * addedFaces));
            addedFaces++;
        }
        if (outerFaces[5]){
            cubeVertices.AddRange(new Vector3[]{
                new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 1), new Vector3(0, 0, 1)});
            cubeTriangles.AddRange((new int[]{0, 3, 2,  0, 2, 1}).Select(index => index + 4 * addedFaces));
            addedFaces++;
        }

        Vector2[] cubeUVs = {
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1)
        };
        for (int i = 0; i < cubeVertices.Count; i++)
        {
            vertices.Add(cubeVertices[i] * voxelSize + position);
            uvs.Add(cubeUVs[i%4]);
        }
        for (int i = 0; i < cubeTriangles.Count; i++)
        {
            triangles.Add(startIndex + cubeTriangles[i]);
        }
    }

    

    private Mesh GetMesh(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }
}
