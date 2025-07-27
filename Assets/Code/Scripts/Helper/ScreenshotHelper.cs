using UnityEngine;
// Saves the current view of the camera as a transparent picture
public class ScreenshotHelper : MonoBehaviour
{
    LevelManager levelManager;
    public Camera camera;
    void Start()
    {

        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        if (levelManager == null)
        {
            Debug.LogError("ScreenshotHelper: LevelManager not found in the scene.");
        }
        levelManager.LoadLevel(0);
    }

    // Update is called once per frame
    void Update()
    {
        var keyboard = UnityEngine.InputSystem.Keyboard.current; // simple implementation without using the whole Input System, more of a quick and dirty solution
        if (keyboard != null)
        {
            if (keyboard.pKey.wasPressedThisFrame)
            {
                SaveCurrentViewAsPicture();
            }
            if (keyboard.oKey.wasPressedThisFrame)
            {
                levelManager.LoadLevel(levelManager.currentLevel + 1);
            }
        }
    }

    // Removed OnEnable and OnDisable as they are not needed for polling input

    void SaveCurrentViewAsPicture()
    {
        RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        camera.targetTexture = renderTexture;
        camera.Render();

        RenderTexture.active = renderTexture;
        Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        texture.Apply();

        camera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        byte[] bytes = texture.EncodeToPNG();
        Destroy(texture);

        System.IO.File.WriteAllBytes(Application.persistentDataPath + "/" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png", bytes);
        Debug.Log("ScreenshotHelper:Saved camera view as picture at: " + Application.persistentDataPath);
    }
}
