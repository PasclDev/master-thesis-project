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
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
