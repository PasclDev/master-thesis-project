using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class InteractorDebug : MonoBehaviour
{
    public string interactorName = "InteractorRight";
    public List<GameObject> currentHovers = new List<GameObject>();
    public GameObject currentHover;

    public void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (
            args.interactableObject != null
            && !currentHovers.Contains(args.interactableObject.transform.gameObject)
            && args.interactableObject.transform.CompareTag("Grabbable")
        )
        {
            currentHovers.Add(args.interactableObject.transform.gameObject);
            currentHover = args.interactableObject.transform.gameObject;
            LogCurrentHovers();
        }
    }

    public void OnHoverExit(HoverExitEventArgs args)
    {
        if (
            args.interactableObject != null
            && !currentHovers.Contains(args.interactableObject.transform.gameObject)
            && args.interactableObject.transform.CompareTag("Grabbable")
        )
        {
            currentHovers.Remove(args.interactableObject.transform.gameObject);
            currentHover = null;
            LogCurrentHovers();
        }
    }

    private void LogCurrentHovers()
    {
        string hoverList = string.Join(", ", currentHovers.ConvertAll(obj => obj.name));
        Debug.Log($"{interactorName}: Hover List: {hoverList}");
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
