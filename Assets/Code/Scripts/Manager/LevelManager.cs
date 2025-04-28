using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
public class LevelManager : MonoBehaviour
{
    public string jsonFileName = "levels.json"; // JSON file name
    public int currentLevel = 0; // Current level index
    public GameObject fillablePrefab; // fillableObject prefab
    public static bool isDebug = true; // Debug mode
    public static float rotationTolerancePercentage = 1.00f; // 20% tolerance for rotation
    public static float distanceTolerancePercentage = 0.20f; // 20% tolerance for position

    private LevelCollection levelCollection; // Level collection
    private VoxelMeshGenerator voxelMeshGenerator;
    public GameObject lastLevelWindow;
    public GameObject tutorialManagerPrefab;

    // Extra variables for the study
    private int[] timeStressCrucialLevel = new int[] { 10, 2 }; // Level that is crucial for the study (currently 10) that get put to the start of the queue if little time is left
    public List<int> levelQueue = new List<int>(); // Queue of levels to be loaded. If current level is 2, next in queue would be 3, 4, etc. Only exists to smoothly change level order in the study.
    private List<int> completedLevel = new List<int>(); // List of completed level.
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
        Debug.Log("LevelManager: Start");
        if (fillablePrefab == null)
        {
            Debug.LogError("LevelManager Error: fillableObject Prefab is not assigned!");
            return;
        }
        voxelMeshGenerator = GetComponent<VoxelMeshGenerator>();
        LoadLevelsFromJSON();
    }
    void Start()
    {

    }
    public void ResetLevelHeight()
    {
        transform.position = new Vector3(transform.position.x, Camera.main.transform.position.y - 0.4f, transform.position.z); // Level is upwards, so this height is the lowest point of the level - 0.05f (due to Height Change Interactable). Starting at Eye-Height - 0.4f.
    }
    public void LoadLevelsFromJSON()
    {
        Debug.Log("LevelManager: Loading levels from JSON");
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonFileName);
        string jsonData = "";
        if (Application.platform == RuntimePlatform.Android)
        {
            // streamingAssets are compressed in android (not readable with File).
            try
            {
                UnityWebRequest reader = UnityWebRequest.Get(filePath);
                reader.SendWebRequest();
                while (!reader.isDone) { }
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
        }
        else if (File.Exists(filePath))
        {
            jsonData = File.ReadAllText(filePath);
        }
        else
        {
            Debug.LogError("LevelManager Error: JSON file not found: " + filePath);
        }
        levelCollection = JsonUtility.FromJson<LevelCollection>(jsonData);
        ConvertRawVoxelsToVoxels(levelCollection);
        //Debug.Log("LevelJson: "+JsonUtility.ToJson(levelCollection));
        Debug.Log("LevelManager: Levels loaded from JSON, level count: " + levelCollection.levels.Count);
    }
    // converts all rawVoxels (int[]) into voxels (int[][][])
    void ConvertRawVoxelsToVoxels(LevelCollection levelCollection)
    {
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
        voxelMeshGenerator.GenerateFillableObject(currentLevelData.voxelSize, currentLevelData.fillable);
        StatisticManager statisticsManager = StatisticManager.instance;
        statisticsManager.levelStatistic.levelId = levelIndex;
        statisticsManager.levelStatistic.numberOfFillables = 1; // Currently still only one fillable per level
        statisticsManager.levelStatistic.numberOfGrabbables = currentLevelData.grabbables.Count;
        UIManager.instance.SetManageLevelUIText(levelIndex, currentLevelData.grabbables.Count, 1);
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
        if (currentLevel != 0)
        {
            LoadNextLevel();
            AudioManager.instance.Play("Level_Complete");
        }
        Debug.Log("LevelManager: Fillables filled!"); // Warning: Used in tutorial-logic, after currentLevel check! (prevents bug where next level is loaded while this function is called)
    }
    public void TutorialFinished()
    {
        Debug.Log("LevelManager: Tutorial finished! Loading Level: 1");

        LoadNextLevel();
    }
    public void LoadNextLevel()
    {
        if (levelQueue.Count > 0)
        {
            LoadLevel(levelQueue.FirstOrDefault());
            return;
        }
        else LoadLevel(currentLevel + 1); // Load next level

    }
    public void LoadLevel(int levelIndex, bool isCompleted = true)
    {
        if (isCompleted)
        {
            if (!completedLevel.Contains(currentLevel)) // If level is not already completed
            {
                completedLevel.Add(currentLevel);
            }
        }
        if ((levelQueue.Count == 0) || levelIndex != levelQueue.FirstOrDefault()) // If loaded level is not the first in the queue or queue is empty
        {
            levelQueue.Clear();
            // Add every number from levelIndex to the last level to the queue
            for (int i = levelIndex + 1; i < levelCollection.levels.Count; i++)
            {
                if (!completedLevel.Contains(i)) // If level is not already completed
                    levelQueue.Add(i);
            }
        }
        else if (levelQueue.Count > 0)// If loaded level is the first in the queue
        {
            levelQueue.RemoveAt(0); // Remove the first element from the queue
        }
        Debug.Log("LevelManager: Loading Level: " + levelIndex + "| Queue: " + string.Join(", ", levelQueue) + "| Completed: " + string.Join(", ", completedLevel));
        // Unload previous level
        foreach (Transform child in transform)
        {
            if (child.gameObject.CompareTag("Grabbable"))
                child.gameObject.GetComponent<GrabbableManager>().Despawn();
            else if (child.gameObject.CompareTag("Fillable") || child.gameObject.CompareTag("Tutorial"))
                Destroy(child.gameObject);
        }
        //Reset lastLevel Window
        lastLevelWindow.SetActive(false);
        //Write Level Log
        if (StatisticManager.instance.levelStatistic.levelId != 0)
        {
            StatisticManager.instance.WriteLevelLog(isCompleted);
        }
        // Handle loading the tutorial
        if (levelIndex == 0)
        {
            currentLevel = 0;
            UIManager.instance.SetManageLevelUIText(0, 0, 0);
            Instantiate(tutorialManagerPrefab, transform);
            return;
        }
        // Load a normal level
        if (levelIndex < levelCollection.levels.Count)
        {
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
    private void SetLevelToFrontOfQueue(int levelIndex)
    {
        if (levelQueue.Contains(levelIndex))
        {
            levelQueue.Remove(levelIndex);
        }
        levelQueue.Insert(0, levelIndex);
    }
    public void MoveCrucialLevelsToFrontOfQueue() // Gets called when time is running out to make sure every crucial level is played
    {
        foreach (int level in timeStressCrucialLevel)
        {
            if (completedLevel.Contains(level) || currentLevel == level) // If level is already completed or is currently doing said level, skip it
                continue;
            SetLevelToFrontOfQueue(level);
        }
    }
}
