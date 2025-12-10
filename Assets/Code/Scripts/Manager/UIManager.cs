using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    // Singleton
    public static UIManager instance;
    public GameObject UILevelList;
    public GameObject UILevelListNoCurLevel;
    public GameObject UIMainMenu;
    public GameObject UICreateLevel;
    private GameObject currentUI;
    public InputActionReference toggleCurrentUIAction;
    public bool isUiVisible = false;
    public TextMeshProUGUI levelInformationText;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(this);
        toggleCurrentUIAction.action.performed += ToggleCurrentUIInput;
        SceneManager.activeSceneChanged += ChangeCurrentUI;
        // Case: Depending on the scene, change the current UI
        currentUI = UIMainMenu;
    }
    void OnDestroy()
    {
        toggleCurrentUIAction.action.performed -= ToggleCurrentUIInput;
    }
    private void ChangeCurrentUI(Scene _, Scene loading)
    {
        string name = loading.name;
        name = name.Substring(0, name.Length-5);
        ChangeCurrentUI(name);
    }
    public void ChangeCurrentUI(string name)
    {
        UILevelList.SetActive(false);
        UICreateLevel.SetActive(false);
        UIMainMenu.SetActive(false);
        UILevelListNoCurLevel.SetActive(false);
        switch (name)
        {
            case "CreateLevel":
                currentUI = UICreateLevel;
                break;
            case "LevelList":
            case "MainGame":
                currentUI = UILevelList;
                break;
            case "MainMenu":
                currentUI = UIMainMenu;
                ShowCurrentUI();
                break;
            case "LevelListNoCurLevel":
            default: 
                currentUI = UILevelListNoCurLevel;
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
    public void HideCurrentUI()
    {
        currentUI.SetActive(false);
        isUiVisible = false;
    }
    public void ShowUI(string UIName)
    {
        switch (UIName)
        {
            case "CreateLevel":
                UICreateLevel.SetActive(true);
                break;
            case "LevelList":
                UILevelList.SetActive(true);
                break;
            case "MainMenu":
                UIMainMenu.SetActive(true);
                break;
            case "LevelListWithNoCurrentLevel":
                UILevelListNoCurLevel.SetActive(true);
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
