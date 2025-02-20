using System.IO;
using UnityEngine;

public class GenerateGrabbables : MonoBehaviour
{
    public string jsonFileName = "levels.json"; // JSON file name
    public int currentLevel = 0; // Current level index
    public GameObject voxelPrefab; // Voxel prefab
    private LevelCollection levelCollection; // Level collection

    void Start()
    {
        if (voxelPrefab == null)
        {
            Debug.LogError("Voxel Prefab is not assigned!");
            return;
        }

        LoadLevelsFromJSON();
        GenerateGrabbableObjects(currentLevel);
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

    private void GenerateGrabbableObjects(int levelIndex)
    {
        Debug.Log("Generating grabbables: " + levelIndex);
        if (levelCollection == null || levelCollection.levels.Count <= levelIndex)
        {
            Debug.LogError("Invalid level index!");
            return;
        }

        LevelData currentLevel = levelCollection.levels[levelIndex];
        Vector3Int gridSize = new Vector3Int(currentLevel.fillable.size[0], currentLevel.fillable.size[1], currentLevel.fillable.size[2]);
        Vector3 gridCenter = (gridSize - Vector3.one) * 0.5f;
        Vector3 size = Vector3.one * currentLevel.voxelSize;
        foreach (var grabbable in currentLevel.grabbables)
        {
            Vector3 grabbablePosition = new Vector3(grabbable.position[0], grabbable.position[1], grabbable.position[2]);
            for (int x = 0; x < gridSize.x; x++)
                {
                    for (int y = 0; y < gridSize.y; y++)
                    {
                        for (int z = 0; z < gridSize.z; z++)
                        {
                            int index = x + gridSize.x * (y + gridSize.y * z); // Convert 3D index to 1D
                            if (grabbable.voxels[index] == 1)
                            {
                                Vector3 position = (new Vector3(x, y, z) + grabbablePosition - gridCenter) * currentLevel.voxelSize;
                                Instantiate(voxelPrefab, position, Quaternion.identity, transform).transform.localScale = size;
                            }
                        }
                    }
                }
        }
        
    }
}
