using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class OutlineGrabInteractable : XRGrabInteractable
{
    private bool isGrabbing = false;
    public Outline outline;
    public bool initializeOutlineOnStart = true;
    void Start()
    {
        if (initializeOutlineOnStart) InitializeOutline();
    }
    // For FarbFormen Objects it needs to get initialized from GrabbableManager after mesh generation //
    public void InitializeOutline()
    {
        outline = gameObject.AddComponent<Outline>();
        outline.OutlineWidth = 5;
        outline.enabled = false; // Disable the outline by default
    }
    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        base.OnSelectEntering(args);
        isGrabbing = true;
        outline.OutlineColor = Color.white;
        outline.enabled = true;
        Debug.Log("Grabbable: Grabbed"); // Warning: Used in tutorial-logic
    }

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        base.OnSelectExiting(args);
        isGrabbing = false;
        outline.enabled = false; // Disable the outline when not grabbing
        /*         
        StartCoroutine(WaitAndCheckHover());
        IEnumerator WaitAndCheckHover()
        {
            yield return new WaitForEndOfFrame();
            if (!isHovered)
            {
                outline.enabled = false;
            }
        }*/
        // Check distance to XR Origin and clamp if necessary
        GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
        if (xrOrigin != null)
        {
            float distance = Vector3.Distance(transform.position, xrOrigin.transform.position);
            if (distance > 2f)
            {
                // Clamp position to 4 units from XR Origin
                Vector3 direction = (transform.position - xrOrigin.transform.position).normalized;
                transform.position = xrOrigin.transform.position + direction * 2f;
            }
        }
        
        Debug.Log("Grabbable: Released");
    }
    protected override void OnHoverEntering(HoverEnterEventArgs args)
    {
        base.OnHoverEntering(args);
        outline.OutlineColor = Color.gray;
        outline.enabled = true; // Enable the outline when hovering
        Debug.Log("Grabbable: Hovering"); // Warning: Used in tutorial-logic
    }
    protected override void OnHoverExiting(HoverExitEventArgs args)
    {
        base.OnHoverExiting(args);
        if (!isGrabbing)
            outline.enabled = false; // Disable the outline when not hovering
        Debug.Log("Grabbable: Not Hovering"); // Warning: Used in tutorial-logic
    }
}



