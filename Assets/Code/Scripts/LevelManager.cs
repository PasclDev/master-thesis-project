using System.IO;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public string jsonFileName = "levels.json"; // JSON file name
    public int currentLevel = 0; // Current level index
    public GameObject fillablePrefab; // fillableObject prefab
    public static bool isDebug = true; // Debug mode
    public static float rotationTolerancePercentage = 0.20f; // 20% tolerance for rotation
    public static float distanceTolerance = 0.02f; // 2 cm tolerance for position
    
    private LevelCollection levelCollection; // Level collection
    private VoxelMeshGenerator voxelMeshGenerator;
    void Start()
    {
        if (fillablePrefab == null)
        {
            Debug.LogError("LevelManager Error: fillableObject Prefab is not assigned!");
            return;
        }
        voxelMeshGenerator = GetComponent<VoxelMeshGenerator>();
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
            ConvertRawVoxelsToVoxels(levelCollection);
            //Debug.Log(JsonUtility.ToJson(levelCollection));
        }
        else
        {
            Debug.LogError("JSON file not found: " + filePath);
        }
    }
    // converts all rawVoxels (int[]) into voxels (int[][][])
    void ConvertRawVoxelsToVoxels(LevelCollection levelCollection){
        foreach (var level in levelCollection.levels)
        {
            foreach (var grabbable in level.grabbables)
            {
                int[] rawVoxels = grabbable.rawVoxels;
                int[] size = grabbable.size;
                int[][][] voxels = new int[size[0]][][];

                for (int x = 0; x < size[0]; x++)
                {
                    voxels[x] = new int[size[1]][];
                    for (int y = 0; y < size[1]; y++)
                    {
                        voxels[x][y] = new int[size[2]];
                    }
                }
                // Fill voxels with rawVoxels
                for (int x = 0; x < size[0]; x++)
                {
                    for (int y = 0; y < size[1]; y++)
                    {
                        for (int z = 0; z < size[2]; z++)
                        {
                            int index = x + size[0] * (y + size[1] * z);
                            voxels[x][y][z] = rawVoxels[index];
                        }
                    }
                }
                grabbable.voxels = voxels;
            }
        }
    }

    void GenerateLevel(int levelIndex)
    {
        Debug.Log("Current Level: " + levelIndex);
        if (levelCollection == null || levelCollection.levels.Count <= levelIndex)
        {
            Debug.LogError("LevelManager Error: Invalid level index!");
            return;
        }

        LevelData currentLevelData = levelCollection.levels[levelIndex];
        voxelMeshGenerator.GenerateMesh(currentLevelData);
        GenerateFillableObject(currentLevelData.fillable, currentLevelData.voxelSize);
    }
    void GenerateFillableObject(Fillable fillable, float voxelSize)
    {
        GameObject fillableObject = Instantiate(fillablePrefab, transform);
        fillableObject.transform.position = voxelSize*new Vector3(fillable.position[0], fillable.position[1], fillable.position[2]); // Center it
        fillableObject.transform.localScale = new Vector3(fillable.size[0], fillable.size[1], fillable.size[2]) * voxelSize; // Scale to fit grid
        Vector3Int size = new Vector3Int(fillable.size[0], fillable.size[1], fillable.size[2]);
        fillableObject.GetComponent<FillableManager>().Initialize(size, voxelSize);
    }
}
