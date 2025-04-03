using UnityEngine;

public class UIManager : MonoBehaviour
{
    // Singleton
    public static UIManager instance;
    public GameObject ManageLevelUI;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    public void ShowManageLevelUI()
    {
        ManageLevelUI.SetActive(true);
    }
    public void HideManageLevelUI()
    {
        ManageLevelUI.SetActive(false);
    }
}
