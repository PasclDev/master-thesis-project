using System.IO;
using UnityEngine;

public class GenerateObjectGrid : MonoBehaviour
{
    public string jsonFileName = "levels.json"; // JSON file name
    public int currentLevel = 0; // Current level index
    public GameObject voxelPrefab; // Voxel prefab
    public GameObject borderPrefab; // Border prefab
    private LevelCollection levelCollection; // Level collection

    void Start()
    {
        if (voxelPrefab == null)
        {
            Debug.LogError("Voxel Prefab is not assigned!");
            return;
        }

        LoadLevelsFromJSON();
        GenerateLevel(currentLevel);
    }

    void LoadLevelsFromJSON()
    {
        
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonFileName);
        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            levelCollection = JsonUtility.FromJson<LevelCollection>(jsonData);
            Debug.Log(JsonUtility.ToJson(levelCollection));
        }
        else
        {
            Debug.LogError("JSON file not found: " + filePath);
        }
    }

    void GenerateLevel(int levelIndex)
    {
        Debug.Log("Generating level: " + levelIndex);
        if (levelCollection == null || levelCollection.levels.Count <= levelIndex)
        {
            Debug.LogError("Invalid level index!");
            return;
        }

        LevelData currentLevel = levelCollection.levels[levelIndex];
        Vector3 voxelSize = voxelPrefab.transform.localScale; // Get prefab scale
        Vector3Int gridSize = new Vector3Int(currentLevel.gridSize[0], currentLevel.gridSize[1], currentLevel.gridSize[2]);
        Vector3 gridCenter = (gridSize - Vector3.one) * 0.5f * voxelSize.x;

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                for (int z = 0; z < gridSize.z; z++)
                {
                    int index = x + gridSize.x * (y + gridSize.y * z); // Convert 3D index to 1D
                    if (currentLevel.voxels[index] == 1)
                    {
                        Vector3 position = new Vector3(x, y, z) * voxelSize.x - gridCenter;
                        Instantiate(voxelPrefab, position, Quaternion.identity, transform);
                    }
                }
            }
        }

        GenerateBorder(gridSize, gridCenter, voxelSize);
    }
    void GenerateBorder(Vector3 gridSize, Vector3 gridCenter, Vector3 voxelSize)
    {
        GameObject border = Instantiate(borderPrefab, transform);
        border.transform.position = transform.position; // Center it
        border.transform.localScale = new Vector3(gridSize.x, gridSize.y, gridSize.z) * voxelSize.x; // Scale to fit grid
    }
}
