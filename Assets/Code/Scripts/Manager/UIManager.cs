using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    // Singleton
    public static UIManager instance;
    public GameObject ManageLevelUI;
    public InputActionReference toggleManageLevelUIAction;
    public bool isUiVisible = false;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        toggleManageLevelUIAction.action.performed += ToggleManageLevelUI;
    }
    void OnDestroy()
    {
        toggleManageLevelUIAction.action.performed -= ToggleManageLevelUI;
    }
    private void ToggleManageLevelUI(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        ManageLevelUI.SetActive(!ManageLevelUI.activeSelf);
        isUiVisible = ManageLevelUI.activeSelf;
    }
    public void ShowManageLevelUI()
    {
        ManageLevelUI.SetActive(true);
        isUiVisible = true;
    }
    public void HideManageLevelUI()
    {
        ManageLevelUI.SetActive(false);
        isUiVisible = false;
    }
}
