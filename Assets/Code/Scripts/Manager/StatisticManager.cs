using UnityEngine;


public class StatisticManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelStatistic
    {
        public int tutorialStep = 0;
        public int levelId = 0;
        public int numberOfFillables = 0;
        public int numberOfGrabbables = 0;
        public float timeTilFirstGrab = 0;
        public int numberOfGrabs = 0;
        public int numberOfSnapsToFillables = 0;
        public int numberOfFillableTransparency = 0;
        public int numberOfGrabbableTransparency = 0;

    }
    // Starts a timer on awake that counts the time since the game started and writes the current time in a log file whenever the log function is called
    private float timeSinceStart = 0;
    private float timeSinceLastLog = 0;
    private string logFilePath;
    private System.IO.StreamWriter logFile;
    public static StatisticManager instance;
    public LevelStatistic levelStatistic = new LevelStatistic();
    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        // Logging
        if (Application.platform == RuntimePlatform.Android)
        { //Path to Downloads in Meta Quest 3
            logFilePath = "/storage/emulated/0/Documents/" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".csv";
        }
        /*else if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {*/
        else
        {
            logFilePath = Application.persistentDataPath + "/" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".csv";
        }
        logFile = new System.IO.StreamWriter(logFilePath, true);
        logFile.WriteLine("TimeSinceStart;TutorialStep;LevelID;NumberOfFillables;NumberOfGrabbables;LevelCompleted;TimeToComplete;TimeTilFirstGrab;NumberOfGrabs;NumberOfSnapsToFillables;NumberOfFillableTransparency;NumberOfGrabbableTransparency");
    }
    void Update()
    {
        timeSinceStart += Time.deltaTime;
    }
    void OnDestroy()
    {
        WriteLevelLog(false);
        logFile.Close();
    }
    void OnApplicationQuit()
    {
        WriteLevelLog(false);
        logFile.Close();
    }
    public void WriteLevelLog(bool isLevelComplete = true)
    {
        logFile.WriteLine(
            timeSinceStart.ToString("F6")                               // TimeSinceStart 
            + ";" + levelStatistic.tutorialStep                         // TutorialStep  
            + ";" + levelStatistic.levelId                              // LevelID  
            + ";" + levelStatistic.numberOfFillables                    // NumberOfFillables
            + ";" + levelStatistic.numberOfGrabbables                   // NumberOfGrabbables
            + ";" + isLevelComplete                                     // LevelCompleted 
            + ";" + (timeSinceStart - timeSinceLastLog).ToString("F6")  // Time to complete
            + ";" + levelStatistic.timeTilFirstGrab.ToString("F6")      // TimeTilFirstGrab
            + ";" + levelStatistic.numberOfGrabs                        // NumberOfGrabs
            + ";" + levelStatistic.numberOfSnapsToFillables             // NumberOfSnapsToFillables
            + ";" + levelStatistic.numberOfFillableTransparency         // NumberOfFillableTransparency
            + ";" + levelStatistic.numberOfGrabbableTransparency        // NumberOfGrabbableTransparency
        );
        timeSinceLastLog = timeSinceStart;
        levelStatistic = new LevelStatistic();
        Debug.Log("StatisticManager: Log written to " + logFilePath);
    }
    public void SetTimeTilFirstGrab()
    {
        if (levelStatistic.timeTilFirstGrab == 0)
        {
            levelStatistic.timeTilFirstGrab = timeSinceStart - timeSinceLastLog;
        }
    }
}
