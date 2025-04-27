using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class YAxisGrabInteractable : XRGrabInteractable
{
    private Vector3 grabOffset;
    private bool isGrabbing = false;

    public Outline HeightInteractableOutline; // Reference to the outline component

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
        HeightInteractableOutline.OutlineColor = Color.white;
        HeightInteractableOutline.enabled = true;
        // Get the interactors transform (controller position)
        grabOffset = transform.position - args.interactorObject.transform.position;
        Debug.Log("HeightInteractable: Grabbed"); // Warning: Used in tutorial-logic
    }

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        base.OnSelectExiting(args);
        isGrabbing = false;
        HeightInteractableOutline.enabled = false; // Disable the outline when not grabbing
        Debug.Log("HeightInteractable: Released");
    }
    protected override void OnHoverEntering(HoverEnterEventArgs args)
    {
        base.OnHoverEntering(args);
        HeightInteractableOutline.OutlineColor = Color.gray;
        HeightInteractableOutline.enabled = true; // Enable the outline when hovering
        Debug.Log("HeightInteractable: Hovering"); // Warning: Used in tutorial-logic
    }
    protected override void OnHoverExiting(HoverExitEventArgs args)
    {
        base.OnHoverExiting(args);
        if (!isGrabbing)
            HeightInteractableOutline.enabled = false; // Disable the outline when not hovering
        Debug.Log("HeightInteractable: Not Hovering"); // Warning: Used in tutorial-logic
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



