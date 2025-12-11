using Unity.XR.CoreUtils;
using UnityEngine;

public class MixedRealityManager : MonoBehaviour
{
    // Singleton
    public static MixedRealityManager Instance { get; private set; }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
            for(int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
