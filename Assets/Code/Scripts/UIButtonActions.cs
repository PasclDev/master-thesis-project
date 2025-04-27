using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIButtonActions : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public InputActionReference leftGrabAction;
    public InputActionReference rightGrabAction;
    private bool isHovered = false;
    private Button button;

    void OnEnable() => AddActions();

    void OnDisable() => RemoveActions();

    public void Awake()
    {
        button = GetComponent<Button>();
    }

    public void AddActions()
    {
        leftGrabAction.action.performed += OnGrabAction;
        rightGrabAction.action.performed += OnGrabAction;
    }

    public void RemoveActions()
    {
        isHovered = false;
        leftGrabAction.action.performed -= OnGrabAction;
        rightGrabAction.action.performed -= OnGrabAction;
    }

    // button.IsHighlighted() is a protected method, so new Button script with : Button would be needed to access it, this would mean each button component would need to be reimplemented, so this is a workaround
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    private void OnGrabAction(InputAction.CallbackContext context)
    {
        if (context.performed && isHovered)
        {
            isHovered = false;
            button.onClick.Invoke(); // Invoke the button click event
        }
    }

    public void OnLoadLevelButton(int levelIndex)
    {
        if (SceneManager.GetActiveScene().name != "MainGameScene")
        {
            SceneManager.sceneLoaded += OnMainGameSceneLoaded;
            SceneManager.LoadScene("MainGameScene");
        }
        else
        {
            LoadLevel(levelIndex);
        }

        void OnMainGameSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "MainGameScene")
            {
                SceneManager.sceneLoaded -= OnMainGameSceneLoaded;
                LoadLevel(levelIndex);
            }
        }

        void LoadLevel(int level)
        {
            Debug.Log("UIButtonActions: Load Level " + level);
            LevelManager.instance.LoadLevel(level, false);
            UIManager.instance.HideManageLevelUI();
        }
    }

    public void OnRestartLevelButton()
    {
        Debug.Log("UIButtonActions: Restart Level");
        LevelManager.instance.LoadLevel(LevelManager.instance.currentLevel, false);
        UIManager.instance.HideManageLevelUI();
    }
    public void OnResetCreateLevelButton()
    {
        GameObject.Find("CreateLevelManager").GetComponent<CreateLevelManager>().ResetLevel();
        UIManager.instance.HideManageLevelUI();
    }
    public void OnSaveLevelButton()
    {
        GameObject.Find("CreateLevelManager").GetComponent<CreateLevelManager>().SaveLevel();
        UIManager.instance.HideManageLevelUI();
    }
    public void OnLoadSceneButton(string sceneName)
    {
        Debug.Log("UIButtonActions: Load Scene " + sceneName);
        SceneManager.LoadScene(sceneName);
        //SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        //SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

    }
    public void OnQuitApplicationButton()
    {
        Debug.Log("UIButtonActions: Quit Application");
        Application.Quit();
        //Stop runtime in editor mode
        if (Application.isEditor)
        {
#if UNITY_EDITOR // UnityEditor is not available in build, so a preprocessor directive is used
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
