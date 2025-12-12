using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.Networking;

public class LevelDataProvider : MonoBehaviour
{
    private string jsonFileName = "levels.json"; // JSON file name
    public LevelCollection levelCollection; // Level collection
    public static LevelDataProvider instance;
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
        LoadLevelsFromJSON();
    }
    
    public void LoadLevelsFromJSON()
    {
        Debug.Log("LevelDataProvider: Loading levels from JSON");
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
                    Debug.LogError("LevelDataProvider: Error - Failed to load JSON file: " + reader.error);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("LevelDataProvider: Error - Failed to load JSON file: " + e.Message);
            }
        }
        else if (File.Exists(filePath))
        {
            // In case it is readable with File (like in Editor).
            jsonData = File.ReadAllText(filePath);
        }
        else
        {
            Debug.LogError("LevelDataProvider: Error - JSON file not found: " + filePath);
        }
        levelCollection = JsonUtility.FromJson<LevelCollection>(jsonData);
        ConvertRawVoxelsToVoxels(levelCollection);
        //Debug.Log("LevelJson: "+JsonUtility.ToJson(levelCollection));
        Debug.Log("LevelDataProvider: Levels loaded from JSON, level count: " + levelCollection.levels.Count);
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
}
