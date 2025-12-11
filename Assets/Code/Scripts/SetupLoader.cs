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
            SceneManager.LoadSceneAsync("SetupScene", LoadSceneMode.Additive);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
