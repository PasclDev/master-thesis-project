using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class OutlineGrabInteractable : XRGrabInteractable
{
    private bool isGrabbing = false;
    public Outline HeightInteractableOutline; // Reference to the outline component

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        base.OnSelectEntering(args);
        isGrabbing = true;
        HeightInteractableOutline.OutlineColor = Color.white;
        HeightInteractableOutline.enabled = true;
        Debug.Log("Grabbable: Grabbed"); // Warning: Used in tutorial-logic
    }

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        base.OnSelectExiting(args);
        isGrabbing = false;
        HeightInteractableOutline.enabled = false; // Disable the outline when not grabbing
        Debug.Log("Grabbable: Released");
    }
    protected override void OnHoverEntering(HoverEnterEventArgs args)
    {
        base.OnHoverEntering(args);
        HeightInteractableOutline.OutlineColor = Color.gray;
        HeightInteractableOutline.enabled = true; // Enable the outline when hovering
        Debug.Log("Grabbable: Hovering"); // Warning: Used in tutorial-logic
    }
    protected override void OnHoverExiting(HoverExitEventArgs args)
    {
        base.OnHoverExiting(args);
        if (!isGrabbing)
            HeightInteractableOutline.enabled = false; // Disable the outline when not hovering
        Debug.Log("Grabbable: Not Hovering"); // Warning: Used in tutorial-logic
    }
}



