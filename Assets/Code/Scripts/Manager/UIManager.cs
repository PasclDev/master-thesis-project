using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    // Singleton
    public static UIManager instance;
    public GameObject UILevelMenu;
    public GameObject UIMainMenu;
    public GameObject UICreateLevel;
    private GameObject sceneMainUI;
    private GameObject currentOpenUI;
    public GameObject CurrentLevelStatsSegment;
    public InputActionReference toggleCurrentUIAction;
    public TextMeshProUGUI levelInformationText;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(this);
        HideAllUI();
        ChangeSceneMainUI(SceneManager.GetSceneAt(0).name);
        toggleCurrentUIAction.action.performed += ToggleUIInput;
        SceneManager.activeSceneChanged += ChangeSceneMainUI;
    }
    void OnDestroy()
    {
        toggleCurrentUIAction.action.performed -= ToggleUIInput;
    }
    private void ChangeSceneMainUI(Scene _, Scene loading)
    {
        string name = loading.name;
        Debug.Log("UI: New Active Scene is: "+name+". Changing Default UI accordingly");
        ChangeSceneMainUI(name);
    }
    public void ChangeSceneMainUI(string name)
    {
        HideCurrentUI();
        if (name.EndsWith("Scene")) name = name.Substring(0, name.Length-5);
        CurrentLevelStatsSegment.SetActive(name == "MainGame");
        switch (name)
        {
            case "CreateLevel":
                sceneMainUI = UICreateLevel;
                break;
            case "LevelMenu":
            case "MainGame":
                sceneMainUI = UILevelMenu;
                break;
            case "MainMenu":
            default: 
                sceneMainUI = UIMainMenu;
                ShowSceneMainUI();
                break;
        }
        
    }
    private void ToggleUIInput(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        ToggleUi();
    }
    public void ToggleUi()
    {
        if(currentOpenUI != null)
        {
            HideCurrentUI();
        }
        else
        {
            ShowSceneMainUI();
        }
        
    }
 
    public void ShowSceneMainUI()
    {
        sceneMainUI.SetActive(true);
        currentOpenUI = sceneMainUI;
    }
    // Use only if the button is currentUI unique, like save level for the createLevel menu
    public void HideCurrentUI()
    {
        if(currentOpenUI == null) return;
        currentOpenUI.SetActive(false);
        currentOpenUI = null;
    }
    // Only needed at initialization, as some UIs could be active while working on them
    public void HideAllUI()
    {
        UILevelMenu.SetActive(false);
        UICreateLevel.SetActive(false);
        UIMainMenu.SetActive(false);
    }
    public void ShowUI(string UIName)
    {
        CurrentLevelStatsSegment.SetActive(UIName == "MainGame");
        switch (UIName)
        {
            case "CreateLevel":
                currentOpenUI = UICreateLevel;
                break;
            case "LevelMenu":
                currentOpenUI = UILevelMenu;
                break;
            case "MainMenu":
                currentOpenUI = UIMainMenu;
                break;
        }
        currentOpenUI.SetActive(true);
    }
    public void SetCurrentUIText(int levelIndex, int grabbablesAmount, int fillablesAmount)
    {
        if (levelIndex == 0)
        {
            levelInformationText.text = "Level: Einführung";
            return;
        }
        levelInformationText.text = "Level " + levelIndex + "\nFarbformen: " + grabbablesAmount + " | Gitterboxen: " + fillablesAmount;
    }
    public void SetCurrentUIText(string text)
    {
        levelInformationText.text = text;
    }
}
