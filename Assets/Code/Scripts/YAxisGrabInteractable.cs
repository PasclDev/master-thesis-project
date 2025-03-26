using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class YAxisGrabInteractable : XRGrabInteractable
{
    private Vector3 grabOffset;
    private bool isGrabbing = false;

    protected override void Awake()
    {
        base.Awake(); // Call base Awake method so that this function doesn't override the original one
        trackPosition = false;  // Fully disable XR's default movement
        trackRotation = false;  // Disable rotation
    }

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        base.OnSelectEntering(args);
        isGrabbing = true;

        // Get the interactors transform (controller position)
        grabOffset = transform.position - args.interactorObject.transform.position;
        Debug.Log("HeightInteractable: Grabbed"); // Warning: Used in tutorial-logic
    }

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        base.OnSelectExiting(args);
        isGrabbing = false;
        Debug.Log("HeightInteractable: Released");
    }

    private void Update()
    {
        if (isGrabbing)
        {
            var interactor = interactorsSelecting.FirstOrDefault();
            if (interactor != null)
            {
                // Smooth movement to match the controller's Y position
                Vector3 targetPosition = interactor.transform.position + grabOffset;
                transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, targetPosition.y, transform.position.z), 15f * Time.deltaTime);
            }
        }
    }
}



