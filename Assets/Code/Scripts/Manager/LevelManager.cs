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
    public static float rotationTolerancePercentage = 1.00f; // 20% tolerance for rotation
    public static float distanceTolerancePercentage = 0.20f; // 20% tolerance for position
    
    private LevelCollection levelCollection; // Level collection
    private VoxelMeshGenerator voxelMeshGenerator;
    public GameObject lastLevelWindow;
    public GameObject tutorialManagerPrefab;

    //Single instance of LevelManager
    public static LevelManager instance;
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
        LoadLevelsFromJSON();
        StartCoroutine(LoadLevel(currentLevel));
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
        if (levelCollection == null || levelCollection.levels.Count <= levelIndex)
        {
            Debug.LogError("LevelManager Error: Invalid level index!");
            return;
        }
        Debug.Log("Level: " + levelIndex);
        currentLevel = levelIndex;

        LevelData currentLevelData = levelCollection.levels[levelIndex];
        voxelMeshGenerator.GenerateGrabbableObjects(currentLevelData);
        voxelMeshGenerator.GenerateFillableObject(currentLevelData);
        StatisticManager statisticsManager = StatisticManager.instance;
        statisticsManager.levelStatistic.levelId = levelIndex;
        statisticsManager.levelStatistic.numberOfFillables = 1; // Currently still only one fillable per level
        statisticsManager.levelStatistic.numberOfGrabbables = currentLevelData.grabbables.Count;
        StartCoroutine(WaitForCameraPositionChange());
    }
    //First camera height change sets the level to the camera height
    private IEnumerator WaitForCameraPositionChange()
    {
        Vector3 cameraPosition = Camera.main.transform.position;
        while (cameraPosition == Camera.main.transform.position && Camera.main.transform.position.y < 1)
        {
            yield return new WaitForSeconds(0.1f);
            cameraPosition = Camera.main.transform.position;
        }
        ResetLevelHeight();
    }
    public void FillablesFilled()
    {
        Debug.Log("LevelManager: Fillables filled!"); // Warning: Used in tutorial-logic
        if (currentLevel != 0){
            Debug.Log("LevelManager: Level completed! Loading Level: " + currentLevel+"+1");
            currentLevel++;
            StartCoroutine(LoadLevel(currentLevel));
            AudioManager.instance.Play("Level_Complete");
        }
    }
    public void TutorialFinished()
    {
        StartCoroutine(LoadLevel(1));
    }
    public IEnumerator LoadLevel(int levelIndex){
        if(levelIndex == 0){
            Instantiate(tutorialManagerPrefab, transform);
            yield break;
        }
        if(StatisticManager.instance.levelStatistic.levelId != 0){
            StatisticManager.instance.WriteLevelLog();
        }
        // Unload previous level
        foreach (Transform child in transform)
        {
            if (child.gameObject.CompareTag("Grabbable"))
                child.gameObject.GetComponent<GrabbableManager>().Despawn();
            else if (child.gameObject.CompareTag("Fillable"))
                Destroy(child.gameObject);
        }
        yield return new WaitForSeconds(0.3f);
        if (levelIndex < levelCollection.levels.Count)
        {
            Debug.Log("LevelManager: Loading Level: " + levelIndex);
            GenerateLevel(levelIndex);
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
