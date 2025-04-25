using UnityEngine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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
            button.onClick.Invoke(); // Invoke the button click event
        }
    }

    public void OnLoadLevelButton(int levelIndex)
    {
        Debug.Log("UIButtonActions: Load Level " + levelIndex);
        LevelManager.instance.LoadLevel(levelIndex, false);
        UIManager.instance.HideManageLevelUI();
    }

    public void OnRestartLevelButton()
    {
        Debug.Log("UIButtonActions: Restart Level");
        LevelManager.instance.LoadLevel(LevelManager.instance.currentLevel, false);
        UIManager.instance.HideManageLevelUI();
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
