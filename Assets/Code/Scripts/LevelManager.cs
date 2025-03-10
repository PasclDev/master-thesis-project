using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
public class LevelManager : MonoBehaviour
{
    public string jsonFileName = "levels.json"; // JSON file name
    public int currentLevel = 0; // Current level index
    public GameObject fillablePrefab; // fillableObject prefab
    public static bool isDebug = false; // Debug mode
    public static float rotationTolerancePercentage = 0.20f; // 20% tolerance for rotation
    public static float distanceTolerancePercentage = 0.20f; // 20% tolerance for position
    
    private LevelCollection levelCollection; // Level collection
    private VoxelMeshGenerator voxelMeshGenerator;
    public GameObject lastLevelWindow;

    //Single instance of LevelManager
    public static LevelManager instance;
    private DataGatherer dataGatherer;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        Debug.Log ("LevelManager: Start");
        if (fillablePrefab == null)
        {
            Debug.LogError("LevelManager Error: fillableObject Prefab is not assigned!");
            return;
        }
        voxelMeshGenerator = GetComponent<VoxelMeshGenerator>();
        dataGatherer = GetComponent<DataGatherer>();
        LoadLevelsFromJSON();
        GenerateLevel(currentLevel); 
    }
    public void ResetLevelHeight(){
        transform.position = new Vector3(transform.position.x, Camera.main.transform.position.y - (levelCollection.levels[currentLevel].fillable.size[1] * levelCollection.levels[currentLevel].voxelSize), transform.position.z);
    }
    public void LoadLevelsFromJSON()
    {
        Debug.Log("LevelManager: Loading levels from JSON");
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonFileName);
        string jsonData = "";
        if (Application.platform == RuntimePlatform.Android){
            // streamingAssets are compressed in android (not readable with File).
            try
            {
                UnityWebRequest reader = UnityWebRequest.Get(filePath);
                reader.SendWebRequest();
                while (!reader.isDone) {}
                if (reader.result == UnityWebRequest.Result.Success)
                {
                    jsonData = reader.downloadHandler.text;
                }
                else
                {
                    Debug.LogError("LevelManager Error: Failed to load JSON file: " + reader.error);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("LevelManager Error: Failed to load JSON file: " + e.Message);
            }
        } else if (File.Exists(filePath))
        {
            jsonData = File.ReadAllText(filePath);
        }else{
            Debug.LogError("LevelManager Error: JSON file not found: " + filePath);
        }
        levelCollection = JsonUtility.FromJson<LevelCollection>(jsonData);
        ConvertRawVoxelsToVoxels(levelCollection);
        //Debug.Log("LevelJson: "+JsonUtility.ToJson(levelCollection));
        Debug.Log("LevelManager: Levels loaded from JSON, level count: "+levelCollection.levels.Count);
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

    private void GenerateLevel(int levelIndex)
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
        StartCoroutine(WaitForCameraPositionChange());
    }
    //First camera height change sets the level to the camera height
    private IEnumerator WaitForCameraPositionChange()
    {
        Vector3 cameraPosition = Camera.main.transform.position;
        while (cameraPosition == Camera.main.transform.position && Camera.main.transform.position.y < 1)
        {
            yield return null;
        }
        ResetLevelHeight();
    }
    private void GenerateFillableObject(Fillable fillable, float voxelSize)
    {
        GameObject fillableObject = Instantiate(fillablePrefab, transform);
        fillableObject.transform.position = transform.position + voxelSize*new Vector3(fillable.position[0], fillable.position[1], fillable.position[2]); // Center it
        fillableObject.transform.localScale = new Vector3(fillable.size[0], fillable.size[1], fillable.size[2]) * voxelSize; // Scale to fit grid
        Vector3Int size = new Vector3Int(fillable.size[0], fillable.size[1], fillable.size[2]);
        fillableObject.GetComponent<FillableManager>().Initialize(size, voxelSize);
    }
    public void FillablesFilled()
    {
        Debug.Log("LevelManager: Fillables filled!");
        NextLevel();
    }
    public void NextLevel(){
        dataGatherer.WriteLog("Level " + currentLevel + " completed");
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        if (currentLevel+1 < levelCollection.levels.Count)
        {
            currentLevel++;

            GenerateLevel(currentLevel);
        }
        else
        {
            Debug.Log("LevelManager: All levels completed!");
            if (lastLevelWindow != null)
            {
                lastLevelWindow.SetActive(true);
            }
        }
    }
}
