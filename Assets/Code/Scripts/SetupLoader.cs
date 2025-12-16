using UnityEngine;
using UnityEngine.SceneManagement;
public class SetupLoader : MonoBehaviour
{
    public static SetupLoader instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            // Reload the current scene to ensure SetupScene is loaded first, prevents XR setup issues.
            var sceneName = SceneManager.GetActiveScene().name.ToString();
            SceneManager.LoadScene("SetupScene");
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
