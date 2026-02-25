using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AdditiveSceneLoader : MonoBehaviour
{
    public static AdditiveSceneLoader Instance;

    private string currentScene;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    IEnumerator LoadSceneRoutine(string sceneName)
    {
        // Unload current content scene first
        if (!string.IsNullOrEmpty(currentScene))
        {
            yield return SceneManager.UnloadSceneAsync(currentScene);
        }

        // Small delay helps XR background rebinding
        yield return null;

        // Load new scene additively
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        currentScene = sceneName;

        // Stabilize XR passthrough / AR camera background
        yield return null;
        ForceARBackgroundRefresh();
    }

    void ForceARBackgroundRefresh()
    {
        var bg = FindFirstObjectByType<UnityEngine.XR.ARFoundation.ARCameraBackground>();
        if (bg != null)
        {
            bg.enabled = false;
            bg.enabled = true;
        }
    }
}