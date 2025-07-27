using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class InteractorDebug : MonoBehaviour
{
    public string interactorName = "InteractorRight";
    public GameObject currentHover;

    public void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (
            args.interactableObject != null
            && args.interactableObject.transform.CompareTag("Grabbable")
        )
        {
            currentHover = args.interactableObject.transform.gameObject;
            LogCurrentHovers();
        }
    }

    public void OnHoverExit(HoverExitEventArgs args)
    {
        if (
            args.interactableObject != null
            && args.interactableObject.transform.CompareTag("Grabbable")
        )
        {
            currentHover = null;
            LogCurrentHovers();
        }
    }

    private void LogCurrentHovers()
    {
        Debug.Log($"{interactorName}: Hover: {currentHover}");
        if (currentHover != null)
        {
            Vector3 distance = currentHover.transform.position - transform.position;
            // Distance to closest point on collider
            Vector3 closestPoint = currentHover
                .GetComponent<MeshCollider>()
                .ClosestPointOnBounds(transform.position);
            Debug.Log($"{interactorName} Distance Square: {distance.magnitude}");
            Debug.Log(
                $"{interactorName} Closest Point: {(closestPoint - transform.position).magnitude}"
            );
        }
        else
        {
            Debug.Log($"{interactorName} Distance Square: null");
            Debug.Log($"{interactorName} Closest Point: null");
        }
    }
}
