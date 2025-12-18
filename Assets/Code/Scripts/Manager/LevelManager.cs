using System.Collections;
using UnityEngine;
public class LevelManager : MonoBehaviour
{
    public int currentLevel = 0; // Current level index
    public GameObject fillablePrefab; // fillableObject prefab
    public GameObject levelGrabbable; // LevelGrabbable element to grab the level
    public static bool isDebug = true; // Debug mode
    public static float rotationTolerancePercentage = 1.00f; // 20% tolerance for rotation
    public static float distanceTolerancePercentage = 0.20f; // 20% tolerance for position

    private LevelDataProvider levelDataProvider;
    private VoxelMeshGenerator voxelMeshGenerator;
    public GameObject lastLevelWindow;
    public GameObject tutorialManagerPrefab;

    private Vector3 lastCenterPosition = Vector3.zero; // Center of last level's fillable object, used to calculate position of next level to make it seem that the fillable is the center
    //Single instance of LevelManager
    public static LevelManager instance;
    

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        Debug.Log("LevelManager: Start");
        if (fillablePrefab == null)
        {
            Debug.LogError("LevelManager Error: fillableObject Prefab is not assigned!");
            return;
        }
        voxelMeshGenerator = GetComponent<VoxelMeshGenerator>();
        levelDataProvider = GameObject.Find("LevelDataProvider").GetComponent<LevelDataProvider>();
        UnityEngine.SceneManagement.SceneManager.sceneUnloaded += (scene) => UnloadScene(); // unload current level on scene change
    }
    public void ResetLevelTransform()
    {
        Debug.Log("LevelManager-extra: ResetLevelTransform");
        if (Camera.main == null)
        {
            Debug.LogError("LevelManager Error: Main camera not found!");
            return;
        }
        // level faces camera on y-axis
        // transform needs to rotate to look at camera position on x,z plane, but inverse (so looking away 180 degrees from camera)
        Transform camera = Camera.main.transform;
        if(transform.position.y == 0) {
        Vector3 forward = new Vector3(camera.forward.x, 0, camera.forward.z).normalized;
        transform.position = camera.position + forward * 0.5f - new Vector3(0, 0.4f, 0);
        }
        else
        {
            LevelData levelData = levelDataProvider.levelCollection.levels[currentLevel];
            if (lastCenterPosition != Vector3.zero)
            {
                transform.position = lastCenterPosition - levelData.fillable.size[1] * 0.5f * levelData.voxelSize * Vector3.up - new Vector3(0, 0.05f, 0);
                lastCenterPosition = Vector3.zero;
            }
        }
        Vector3 cameraPosition = camera.position;
        cameraPosition.y = transform.position.y; // keep level y position
        transform.LookAt(cameraPosition);
        transform.Rotate(0, 180, 0);
        
    }

    private void GenerateLevel(int levelIndex)
    {
        if (levelDataProvider.levelCollection == null || levelDataProvider.levelCollection.levels.Count <= levelIndex)
        {
            Debug.LogError("LevelManager Error: Invalid level index!");
            return;
        }
        Debug.Log("Level: " + levelIndex);
        currentLevel = levelIndex;

        LevelData currentLevelData = levelDataProvider.levelCollection.levels[levelIndex];
        voxelMeshGenerator.GenerateGrabbableObjects(currentLevelData);
        voxelMeshGenerator.GenerateFillableObject(currentLevelData.voxelSize, currentLevelData.fillable);
        StatisticManager statisticsManager = StatisticManager.instance;
        statisticsManager.levelStatistic.levelId = levelIndex;
        statisticsManager.levelStatistic.numberOfFillables = 1; // Currently still only one fillable per level
        statisticsManager.levelStatistic.numberOfGrabbables = currentLevelData.grabbables.Count;
        UIManager.instance.SetCurrentUIText(levelIndex, currentLevelData.grabbables.Count, 1);
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
        LoadLevel(currentLevel + 1); // Load next level
    }
    public void UnloadScene()
    {
        levelGrabbable.SetActive(false);
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        UnloadCurrentLevel();
    }
    public void UnloadCurrentLevel()
    {
        Debug.Log("LevelManager: Unloading Current Level: " + currentLevel);
        foreach (Transform child in transform)
        {
            if (child.gameObject.CompareTag("Grabbable"))
                child.gameObject.GetComponent<GrabbableManager>().Despawn();
            else if (child.gameObject.CompareTag("Fillable") || child.gameObject.CompareTag("Tutorial"))
                Destroy(child.gameObject);
        } 
    }
    public void LoadLevel(int levelIndex, bool isCompleted = true)
    {
        lastCenterPosition = transform.Find("Fillable_0") != null ? transform.Find("Fillable_0").position : Vector3.zero;
        Debug.Log("LevelManager: Loading Level: " + levelIndex);
        UnloadCurrentLevel();
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
            UIManager.instance.SetCurrentUIText(0, 0, 0);
            Instantiate(tutorialManagerPrefab, transform);
        }
        else if (levelIndex < levelDataProvider.levelCollection.levels.Count)// Load a normal level
        {
            GenerateLevel(levelIndex);
            levelGrabbable.SetActive(true);
        }
        else
        {
            Debug.Log("LevelManager: All levels completed!");
            if (lastLevelWindow != null)
            {
                lastLevelWindow.SetActive(true);
            }
        }
        StartCoroutine(WaitForCameraPositionChange()); // Wait for camera position change before setting level height
    }
        //First camera height change sets the level to the camera height
    public IEnumerator WaitForCameraPositionChange()
    {
        Vector3 cameraPosition = Camera.main.transform.position;
        while (cameraPosition == Camera.main.transform.position && Camera.main.transform.position.y < 1)
        {
            yield return new WaitForSeconds(0.1f);
            cameraPosition = Camera.main.transform.position;
        }
        ResetLevelTransform();
    }
}
