using System.IO;
using UnityEngine;

public class GenerateFillable : MonoBehaviour
{
    public string jsonFileName = "levels.json"; // JSON file name
    public int currentLevel = 0; // Current level index
    public GameObject borderPrefab; // Border prefab
    private LevelCollection levelCollection; // Level collection

    void Start()
    {
        if (borderPrefab == null)
        {
            Debug.LogError("Border Prefab is not assigned!");
            return;
        }

        LoadLevelsFromJSON();
        GenerateFillGridObject(currentLevel);
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

    void GenerateFillGridObject(int levelIndex)
    {
        Debug.Log("Generating level: " + levelIndex);
        if (levelCollection == null || levelCollection.levels.Count <= levelIndex)
        {
            Debug.LogError("Invalid level index!");
            return;
        }

        LevelData currentLevel = levelCollection.levels[levelIndex];
        GenerateFillableObject(currentLevel.fillable, currentLevel.voxelSize);
    }
    void GenerateFillableObject(Fillable fillable, float voxelSize)
    {
        GameObject border = Instantiate(borderPrefab, transform);
        border.transform.position = voxelSize*new Vector3(fillable.position[0], fillable.position[1], fillable.position[2]); // Center it
        border.transform.localScale = new Vector3(fillable.size[0], fillable.size[1], fillable.size[2]) * voxelSize; // Scale to fit grid
    }
}
