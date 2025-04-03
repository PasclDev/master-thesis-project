using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    // Singleton
    public static UIManager instance;
    public GameObject ManageLevelUI;
    public InputActionReference toggleManageLevelUIAction;
    public bool isUiVisible = false;
    public TextMeshProUGUI levelInformationText;

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
    public void SetManageLevelUIText(int levelIndex, int grabbablesAmount, int fillablesAmount){
        if (levelIndex == 0){
            levelInformationText.text = "Level: Einführung";
            return;
        }
        levelInformationText.text = "Level " + levelIndex + "\nFarbformen: " + grabbablesAmount + " | Gitterboxen: " + fillablesAmount;
    }
}
