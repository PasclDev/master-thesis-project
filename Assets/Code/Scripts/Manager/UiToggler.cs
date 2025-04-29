using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIToggler : MonoBehaviour
{
    public GameObject UI;
    public InputActionReference toggleUIAction;

    void Awake()
    {
        if (UI == null)
        {
            UI = this.gameObject;
        }
        toggleUIAction.action.performed += ToggleUI;
    }
    void OnDestroy()
    {
        toggleUIAction.action.performed -= ToggleUI;
    }
    private void ToggleUI(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        UI.SetActive(!UI.activeSelf);
    }
    public void ShowUI()
    {
        UI.SetActive(true);
    }
    public void HideUI()
    {
        UI.SetActive(false);
    }
}
