using UnityEngine;
using UnityEngine.InputSystem;

public class EnviromentManager : MonoBehaviour
{
    public GameObject enviroment;
    public InputActionReference toggleEnviromentAction;
    private void Awake()
    {
        toggleEnviromentAction.action.performed += ToggleEnviroment;
    }
    private void OnDestroy()
    {
        toggleEnviromentAction.action.performed -= ToggleEnviroment;
    }
    public void ToggleEnviroment(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        enviroment.SetActive(!enviroment.activeSelf);
    }
}
