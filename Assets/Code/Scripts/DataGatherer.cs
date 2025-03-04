using UnityEngine;

public class DataGatherer : MonoBehaviour
{
    // Starts a timer on awake that counts the time since the game started and writes the current time in a log file whenever the log function is called
    private float timeSinceStart = 0;
    private float timeSinceLastLog = 0;
    private string logFilePath;
    private System.IO.StreamWriter logFile;
    void Awake()
    {
        if(Application.platform == RuntimePlatform.Android){ //Path to Downloads in Meta Quest 3
            logFilePath = "/storage/emulated/0/Documents/" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt";
        }
        /*else if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {*/
        else{
            logFilePath = Application.persistentDataPath + "/" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt";
        }
        logFile = new System.IO.StreamWriter(logFilePath, true);
        WriteLog("Log file created");
    }
    void Update()
    {
        timeSinceStart += Time.deltaTime;
    }
    void OnApplicationQuit()
    {
        logFile.Close();
    }
    public void WriteLog(string log)
    {
        logFile.WriteLine(System.DateTime.Now + "|" + (timeSinceStart - timeSinceLastLog) + "|" + log);
        timeSinceLastLog = timeSinceStart;
        Debug.Log("DataGatherer: " + log + " written to " + logFilePath);
    }
}
