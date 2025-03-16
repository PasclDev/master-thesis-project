using System.Collections;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Meta;


// https://docs.unity3d.com/Packages/com.unity.xr.meta-openxr@2.1/manual/features/boundary-visibility.html

// Also doable with AndroidManifest.xml, but this is more flexible and doesnt need a fully manual Manifest file
public class DisableBoundary : MonoBehaviour
{
    public bool disableBoundary = true;
    void Start()
    {
        if (Application.platform != RuntimePlatform.Android) return;
        if (disableBoundary == false)
        {
            Debug.Log("Boundary: Boundary Visibility is enabled!");
            return;
        }  
        StartCoroutine(TryRequestBoundaryVisibility());
    }
    IEnumerator TryRequestBoundaryVisibility()
    {
        yield return new WaitForSeconds(2); // Needs the delay to find BoundaryVisibilityFeature
        var feature = OpenXRSettings.Instance.GetFeature<BoundaryVisibilityFeature>();
        if (feature == null)
        {
            Debug.Log("Boundary: Boundary Visibility Feature not found!");
            yield break;
        }
        var result = feature.TryRequestBoundaryVisibility(
            XrBoundaryVisibility.VisibilitySuppressed);
        if ((int)result ==
            BoundaryVisibilityFeature.XR_BOUNDARY_VISIBILITY_SUPPRESSION_NOT_ALLOWED_META)
        {
            // The runtime did not accept the request to suppress the boundary.
            // Your app must render passthrough to suppress boundary visibility.
            Debug.Log("Boundary: Boundary Visibility Suppression not allowed!");
            yield break;
        }
        if (result == UnityEngine.XR.OpenXR.NativeTypes.XrResult.FunctionUnsupported)
        {
            Debug.LogWarning("Boundary: Suppression is not supported on this device.");
            yield break;
        }
        if (result < 0)
        {
            // XrResult values less than zero are errors.
            // Handle error here.
            Debug.Log("Boundary: Boundary Visibility Suppression error! "+result);
        }
    }
}