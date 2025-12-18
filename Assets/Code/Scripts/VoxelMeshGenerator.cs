using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoxelMeshGenerator : MonoBehaviour
{
    public GameObject grabbableBlankPrefab;
    public GameObject fillableBlankPrefab;
    public GameObject fillableMissingHighlightPrefab;
    public GameObject createdGrabbableBlankPrefab; // Only used for level creation

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
            Vector3 gridCenter = (Vector3)gridSize * 0.5f;
            Vector3 grabbablePosition = new Vector3(
                grabbable.position[0],
                grabbable.position[1] + (currentLevelData.fillable.size[1] * 0.5f), // Offset for fillable height
                grabbable.position[2]
            );
            Vector3 position =
                transform.position              // Position of LevelManager / this script
                + transform.rotation * (grabbablePosition * voxelSize) // (turn voxel movement amount into world space coordinates, rotated)
                + transform.rotation * new Vector3(0, 0.05f, 0);     // Offset for Height Change Interactable
            GameObject grabbableObject = Instantiate(
                grabbableBlankPrefab,
                position,
                transform.rotation,
                transform
            );
            grabbableObject.name = "Grabbable_" + i;
            Material normalMaterial = grabbableObject.GetComponent<Renderer>().material;
            Material transparentMaterial = new Material(
                grabbableObject.GetComponent<GrabbableManager>().transparentMaterial
            );
            Color newColor = grabbable.GetColor();
            newColor.a = transparentMaterial.color.a;
            normalMaterial.color = newColor;
            transparentMaterial.color = newColor;
            grabbableObject.GetComponent<GrabbableManager>().transparentMaterial =
                transparentMaterial;
            MeshFilter meshFilter = grabbableObject.GetComponent<MeshFilter>();
            MeshCollider meshCollider = grabbableObject.GetComponent<MeshCollider>();
            //grabbableObject.GetComponent<BoxCollider>().size = (Vector3)gridSize * voxelSize;

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
                            Vector3 voxelPosition = (new Vector3(x, y, z) - gridCenter) * voxelSize;
                            bool[] outerFaces = new bool[6]; // Front, Back, Top, Bottom, Right, Left
                            if (x == 0 || grabbable.voxels[x - 1][y][z] == 0)
                                outerFaces[5] = true;
                            if (x == gridSize.x - 1 || grabbable.voxels[x + 1][y][z] == 0)
                                outerFaces[4] = true;
                            if (y == 0 || grabbable.voxels[x][y - 1][z] == 0)
                                outerFaces[3] = true;
                            if (y == gridSize.y - 1 || grabbable.voxels[x][y + 1][z] == 0)
                                outerFaces[2] = true;
                            if (z == 0 || grabbable.voxels[x][y][z - 1] == 0)
                                outerFaces[0] = true;
                            if (z == gridSize.z - 1 || grabbable.voxels[x][y][z + 1] == 0)
                                outerFaces[1] = true;
                            AddOuterFacesToMesh(
                                voxelPosition,
                                voxelSize,
                                vertices,
                                triangles,
                                uvs,
                                outerFaces
                            );
                        }
                    }
                }
            }
            Mesh mesh = GetMesh(vertices, triangles, uvs);
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
            meshCollider.convex = true; // Enable convex to allow interaction from inside
            // Initialize GrabbableManager after mesh is generated so that the outline is generated correctly //
            grabbableObject.GetComponent<GrabbableManager>().Initialize(grabbable, voxelSize);
            // Save mesh as an asset for fixed usage //
            /* string path = "Assets/Resources/TutorialMeshes/Grabbable_" + i + ".asset";
            UnityEditor.AssetDatabase.CreateAsset(mesh, path); */
        }
    }

    public GameObject GenerateFillableObject(float voxelSize, Fillable fillable)
    {
        GameObject fillableObject = Instantiate(fillableBlankPrefab, transform.position, transform.rotation, transform);
        Vector3 position =
            transform.position
            + transform.rotation * (voxelSize
            * new Vector3(
                fillable.position[0],
                fillable.position[1] + (fillable.size[1] * 0.5f),
                fillable.position[2]))
            + transform.rotation * new Vector3(0, 0.05f, 0); // Offset for Height Change Interactable
        Vector3Int size = new Vector3Int(fillable.size[0], fillable.size[1], fillable.size[2]);
        Vector3 gridCenter = (Vector3)size * 0.5f;
        fillableObject.name = "Fillable_0";
        fillableObject.GetComponent<FillableManager>().Initialize(position, size, voxelSize, this);

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
                    if (x == 0)
                        outerFaces[5] = true;
                    if (x == size.x - 1)
                        outerFaces[4] = true;
                    if (y == 0)
                        outerFaces[3] = true;
                    if (y == size.y - 1)
                        outerFaces[2] = true;
                    if (z == 0)
                        outerFaces[0] = true;
                    if (z == size.z - 1)
                        outerFaces[1] = true;
                    AddOuterFacesToMesh(
                        voxelPosition,
                        voxelSize,
                        vertices,
                        triangles,
                        uvs,
                        outerFaces
                    );
                }
            }
        }
        meshFilter.mesh = GetMesh(vertices, triangles, uvs);
        return fillableObject;
    }

    public void GenerateFillableMissingHighlight(
        Transform fillableObject,
        Vector3Int gridSize,
        float voxelSize,
        int[][][] voxels
    )
    {
        GameObject fillableMissingHighlight = Instantiate(
            fillableMissingHighlightPrefab,
            fillableObject
        );
        fillableMissingHighlight.name = "FillableMissingHighlight";

        // Generate Mesh
        MeshFilter meshFilter = fillableMissingHighlight.GetComponent<MeshFilter>();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        // Mesh only consists of the "outer" faces of the voxel grid, not the inner faces
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                for (int z = 0; z < gridSize.z; z++)
                {
                    if (voxels[x][y][z] == 0) // For each missing voxel
                    {
                        Vector3 voxelPosition =
                            (new Vector3(x, y, z) - ((Vector3)gridSize * 0.5f)) * voxelSize;
                        bool[] outerFaces = new bool[6]; // Front, Back, Top, Bottom, Right, Left
                        if (x == 0 || voxels[x - 1][y][z] == 1)
                            outerFaces[5] = true;
                        if (x == gridSize.x - 1 || voxels[x + 1][y][z] == 1)
                            outerFaces[4] = true;
                        if (y == 0 || voxels[x][y - 1][z] == 1)
                            outerFaces[3] = true;
                        if (y == gridSize.y - 1 || voxels[x][y + 1][z] == 1)
                            outerFaces[2] = true;
                        if (z == 0 || voxels[x][y][z - 1] == 1)
                            outerFaces[0] = true;
                        if (z == gridSize.z - 1 || voxels[x][y][z + 1] == 1)
                            outerFaces[1] = true;
                        AddOuterFacesToMesh(
                            voxelPosition,
                            voxelSize,
                            vertices,
                            triangles,
                            uvs,
                            outerFaces
                        );
                    }
                }
            }
        }
        meshFilter.mesh = GetMesh(vertices, triangles, uvs);

        //fillableMissingHighlight.GetComponent<FillableMissingHighlightManager>().Initialize(position, size, voxelSize);
    }
    public void GenerateCreatedGrabbableObject(
        Vector3 position,
        Vector3Int gridSize,
        float voxelSize,
        int[][][] voxels,
        Color color,
        int id
    )
    {
        GameObject createdGrabbableObject = Instantiate(
            createdGrabbableBlankPrefab,
            position,
            transform.rotation,
            transform
        );
        createdGrabbableObject.name = "CreatedGrabbable_" + id;
        Material normalMaterial = createdGrabbableObject.GetComponent<Renderer>().material;
        normalMaterial.color = color;
        MeshFilter meshFilter = createdGrabbableObject.GetComponent<MeshFilter>();

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
                    if (voxels[x][y][z] == id)
                    {
                        Vector3 voxelPosition = (new Vector3(x, y, z) - (Vector3)gridSize * 0.5f) * voxelSize;
                        bool[] outerFaces = new bool[6]; // Front, Back, Top, Bottom, Right, Left
                        if (x == 0 || voxels[x - 1][y][z] != id)
                            outerFaces[5] = true;
                        if (x == gridSize.x - 1 || voxels[x + 1][y][z] != id)
                            outerFaces[4] = true;
                        if (y == 0 || voxels[x][y - 1][z] != id)
                            outerFaces[3] = true;
                        if (y == gridSize.y - 1 || voxels[x][y + 1][z] != id)
                            outerFaces[2] = true;
                        if (z == 0 || voxels[x][y][z - 1] != id)
                            outerFaces[0] = true;
                        if (z == gridSize.z - 1 || voxels[x][y][z + 1] != id)
                            outerFaces[1] = true;
                        AddOuterFacesToMesh(
                            voxelPosition,
                            voxelSize,
                            vertices,
                            triangles,
                            uvs,
                            outerFaces
                        );
                    }
                }
            }
        }
        meshFilter.mesh = GetMesh(vertices, triangles, uvs);
    }
    public void UpdateCreatedGrabbableObject(
        GameObject createdGrabbableObject,
        Vector3Int gridSize,
        float voxelSize,
        int[][][] voxels,
        int id
    )
    {
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
                    if (voxels[x][y][z] == id)
                    {
                        Vector3 voxelPosition = (new Vector3(x, y, z) - (Vector3)gridSize * 0.5f) * voxelSize;
                        bool[] outerFaces = new bool[6]; // Front, Back, Top, Bottom, Right, Left
                        if (x == 0 || voxels[x - 1][y][z] != id)
                            outerFaces[5] = true;
                        if (x == gridSize.x - 1 || voxels[x + 1][y][z] != id)
                            outerFaces[4] = true;
                        if (y == 0 || voxels[x][y - 1][z] != id)
                            outerFaces[3] = true;
                        if (y == gridSize.y - 1 || voxels[x][y + 1][z] != id)
                            outerFaces[2] = true;
                        if (z == 0 || voxels[x][y][z - 1] != id)
                            outerFaces[0] = true;
                        if (z == gridSize.z - 1 || voxels[x][y][z + 1] != id)
                            outerFaces[1] = true;
                        AddOuterFacesToMesh(
                            voxelPosition,
                            voxelSize,
                            vertices,
                            triangles,
                            uvs,
                            outerFaces
                        );
                    }
                }
            }
        }
        createdGrabbableObject.GetComponent<MeshFilter>().mesh = GetMesh(vertices, triangles, uvs);
    }


    //Only add the vertices of the outer faces of the voxel grid
    private void AddOuterFacesToMesh(
        Vector3 position,
        float voxelSize,
        List<Vector3> vertices,
        List<int> triangles,
        List<Vector2> uvs,
        bool[] outerFaces
    )
    {
        int startIndex = vertices.Count;

        List<Vector3> cubeVertices = new List<Vector3>();
        List<int> cubeTriangles = new List<int>();
        int addedFaces = 0;
        if (outerFaces[0])
        {
            cubeVertices.AddRange(
                new Vector3[]
                {
                    new Vector3(0, 0, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(1, 1, 0),
                    new Vector3(0, 1, 0),
                }
            );
            cubeTriangles.AddRange(new int[] { 0, 2, 1, 0, 3, 2 });
            addedFaces++;
        }
        if (outerFaces[1])
        {
            cubeVertices.AddRange(
                new Vector3[]
                {
                    new Vector3(0, 0, 1),
                    new Vector3(1, 0, 1),
                    new Vector3(1, 1, 1),
                    new Vector3(0, 1, 1),
                }
            );
            cubeTriangles.AddRange(
                (new int[] { 0, 1, 2, 0, 2, 3 }).Select(index => index + 4 * addedFaces)
            );
            addedFaces++;
        }
        if (outerFaces[2])
        {
            cubeVertices.AddRange(
                new Vector3[]
                {
                    new Vector3(0, 1, 0),
                    new Vector3(1, 1, 0),
                    new Vector3(1, 1, 1),
                    new Vector3(0, 1, 1),
                }
            );
            cubeTriangles.AddRange(
                (new int[] { 0, 3, 2, 0, 2, 1 }).Select(index => index + 4 * addedFaces)
            );
            addedFaces++;
        }
        if (outerFaces[3])
        {
            cubeVertices.AddRange(
                new Vector3[]
                {
                    new Vector3(0, 0, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(1, 0, 1),
                    new Vector3(0, 0, 1),
                }
            );
            cubeTriangles.AddRange(
                (new int[] { 0, 1, 2, 0, 2, 3 }).Select(index => index + 4 * addedFaces)
            );
            addedFaces++;
        }
        if (outerFaces[4])
        {
            cubeVertices.AddRange(
                new Vector3[]
                {
                    new Vector3(1, 0, 0),
                    new Vector3(1, 1, 0),
                    new Vector3(1, 1, 1),
                    new Vector3(1, 0, 1),
                }
            );
            cubeTriangles.AddRange(
                (new int[] { 0, 1, 2, 0, 2, 3 }).Select(index => index + 4 * addedFaces)
            );
            addedFaces++;
        }
        if (outerFaces[5])
        {
            cubeVertices.AddRange(
                new Vector3[]
                {
                    new Vector3(0, 0, 0),
                    new Vector3(0, 1, 0),
                    new Vector3(0, 1, 1),
                    new Vector3(0, 0, 1),
                }
            );
            cubeTriangles.AddRange(
                (new int[] { 0, 3, 2, 0, 2, 1 }).Select(index => index + 4 * addedFaces)
            );
            addedFaces++;
        }

        Vector2[] cubeUVs =
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1),
        };
        for (int i = 0; i < cubeVertices.Count; i++)
        {
            vertices.Add(cubeVertices[i] * voxelSize + position);
            uvs.Add(cubeUVs[i % 4]);
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
