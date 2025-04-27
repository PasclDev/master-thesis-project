using UnityEngine;
using UnityEngine.InputSystem;

public class RecenterSelf : MonoBehaviour
{
    public Transform head;
    public Transform origin;
    public Transform target;

    private Vector3 offset;
    private Vector3 targetForward;
    private Vector3 cameraForward;
    private float angle;
    public InputActionReference recenterSelfAction;

    private void Awake()
    {
        recenterSelfAction.action.performed += Recenter;
    }
    private void OnDestroy()
    {
        recenterSelfAction.action.performed -= Recenter;
    }

    public void Recenter(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }
        //From https://stackoverflow.com/questions/76297143/in-unity-openxr-environment-how-to-reset-the-player-position-to-center
        offset = head.position - origin.position;
        offset.y = 0;
        origin.position = target.position - offset;

        targetForward = target.forward;
        targetForward.y = 0;
        cameraForward = head.forward;
        cameraForward.y = 0;

        angle = Vector3.SignedAngle(cameraForward, targetForward, Vector3.up);

        origin.RotateAround(head.position, Vector3.up, angle);
        if (LevelManager.instance != null)
            LevelManager.instance.ResetLevelHeight();
    }
}