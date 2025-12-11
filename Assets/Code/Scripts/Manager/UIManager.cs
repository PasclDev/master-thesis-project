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
    private GameObject currentUI;
    public GameObject CurrentLevelStatsSegment;
    public InputActionReference toggleCurrentUIAction;
    public bool isUiVisible = false;
    public TextMeshProUGUI levelInformationText;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(this);
        ChangeCurrentUI(SceneManager.GetSceneAt(0).name);
        toggleCurrentUIAction.action.performed += ToggleCurrentUIInput;
        SceneManager.activeSceneChanged += ChangeCurrentUI;
    }
    void OnDestroy()
    {
        toggleCurrentUIAction.action.performed -= ToggleCurrentUIInput;
    }
    private void ChangeCurrentUI(Scene _, Scene loading)
    {
        string name = loading.name;
        Debug.Log("UI: New Active Scene is: "+name+". Changing Default UI accordingly");
        ChangeCurrentUI(name);
    }
    public void ChangeCurrentUI(string name)
    {
        HideAllUI();
        if (name.EndsWith("Scene")) name = name.Substring(0, name.Length-5);
        CurrentLevelStatsSegment.SetActive(name == "MainGame");
        switch (name)
        {
            case "CreateLevel":
                currentUI = UICreateLevel;
                break;
            case "LevelMenu":
            case "MainGame":
                currentUI = UILevelMenu;
                break;
            case "MainMenu":
            default: 
                currentUI = UIMainMenu;
                ShowCurrentUI();
                break;
        }
        
    }
    private void ToggleCurrentUIInput(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        ToggleCurrentUi();
    }
    public void ToggleCurrentUi()
    {
        currentUI.SetActive(!currentUI.activeSelf);
        isUiVisible = currentUI.activeSelf;
    }
 
    public void ShowCurrentUI()
    {
        currentUI.SetActive(true);
        isUiVisible = true;
    }
    public void HideAllUI()
    {
        UILevelMenu.SetActive(false);
        UICreateLevel.SetActive(false);
        UIMainMenu.SetActive(false);
    }
    // Use only if the button is currentUI unique, like save level for the createLevel menu
    public void HideCurrentUI()
    {
        currentUI.SetActive(false);
        isUiVisible = false;
    }
    public void ShowUI(string UIName)
    {
        CurrentLevelStatsSegment.SetActive(UIName == "MainGame");
        switch (UIName)
        {
            case "CreateLevel":
                UICreateLevel.SetActive(true);
                break;
            case "LevelMenu":
                UILevelMenu.SetActive(true);
                break;
            case "MainMenu":
                UIMainMenu.SetActive(true);
                break;
        }
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
