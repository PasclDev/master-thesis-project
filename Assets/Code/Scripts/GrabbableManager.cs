using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[System.Serializable]
public class InsideFillable
{
    public GameObject fillableObject;
    public int[][][] rotatedVoxelMatrix;
    public Vector3Int gridOffset;
}
[System.Serializable]
public class DebugObjects
{
    public GameObject center;
    public GameObject matrixOrigin;
    public TextMeshPro rotationText;
}
public class GrabbableManager : MonoBehaviour
{
    public Grabbable grabbable;
    public float voxelSize;
    public InsideFillable insideFillable;
    public bool isGrabbed = true;
    public int isInHand = 0; // 1 for left hand, 2 for right hand
    public DebugObjects debugObjects;
    public Material transparentMaterial;
    private Material defaultMaterial;

    private FillableManager lastTouchedFillable;
    public void Initialize(Grabbable grabbable, float voxelSize)
    {
        this.grabbable = grabbable;
        this.voxelSize = voxelSize;
        defaultMaterial = GetComponent<Renderer>().material;
        //Debug.Log(transform.forward + " IS FORWARD" + transform.up + " IS UP" + transform.right + " IS RIGHT");
        
    }

    public void FixedUpdate()
    {
        if(LevelManager.isDebug && isGrabbed){
            debugObjects.center.transform.position = gameObject.transform.position;
            Vector3Int grabbableGridSize = new Vector3Int(grabbable.size[0], grabbable.size[1], grabbable.size[2]);
            Vector3 grabbableRotation = transform.rotation.eulerAngles;
            float rotationTolerancePercentage = 0.1f; // 10% tolerance
            (bool isValidRotation, Vector3 up, Vector3 right, Vector3 forward) = RotationHelper.IsValidRotation(transform, rotationTolerancePercentage);
            debugObjects.matrixOrigin.GetComponent<Renderer>().material.color = isValidRotation ? Color.green : Color.red;
            (int rotatedX, int rotatedY, int rotatedZ) = RotationHelper.RotateDimensionSize(grabbableGridSize.x,grabbableGridSize.y, grabbableGridSize.z, up, forward);
            Vector3Int rotatedGridSize = new Vector3Int(rotatedX, rotatedY, rotatedZ);
            debugObjects.rotationText.text = "Rotation: " + transform.rotation.eulerAngles.ToString("F0") + "\nRounded: " + grabbableRotation.ToString("F0")+ "\nUp: " + up.ToString("F0")+ "\nRight: " + right.ToString("F0")+ "\nForward: " + forward.ToString("F0")+"\nRotated Grid Size:"+Vector3Int.RoundToInt(rotatedGridSize).ToString();
            debugObjects.center.transform.up = up;
            debugObjects.matrixOrigin.transform.position = transform.position - 0.5f * voxelSize * (Vector3)rotatedGridSize; // Center - half of voxel size * rotatedGridSize
        }
    }
    public void Despawn(){
        StartCoroutine(DespawnCoroutine());
    }
    private IEnumerator DespawnCoroutine()
    {
        // Animator sidenote: It needs "Apply Root Motion" so that the object is movable with XR. This is the reason why the objects spawn with semi-random rotations.
        Animator animator = GetComponent<Animator>();
        animator.SetTrigger("Despawn");
        // Wait for the animation to finish
        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Fillable"))
        {
            Debug.Log("Grabbable: Has entered " + other.name);
            lastTouchedFillable = other.GetComponent<FillableManager>();
        }
    }
    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Fillable")){
            lastTouchedFillable = null;
        }
    }
    public void OnSelectExit(){
        isGrabbed = false;
        isInHand = 0;
        SetVisualization(false);
        Debug.Log("Grabbable: "+gameObject.name + "has been dropped!");
        if(lastTouchedFillable != null){
            lastTouchedFillable.CheckIfGrabbableFitsFillable(gameObject);
        }
        else{
            Debug.Log("Grabbable: Is not inside any Fillable");
        }
        if(LevelManager.isDebug){
            debugObjects.center.SetActive(false);
            debugObjects.matrixOrigin.SetActive(false);
        }
    }
    // Triggers whenever the grabbable gets grabbed by the player
    public void OnSelectEnter(SelectEnterEventArgs args){
        isGrabbed = true;
        isInHand = args.interactorObject.transform.name.Contains("Left") ? 1 : 2;
        Debug.Log("Grabbable: "+gameObject.name + "has been picked up! In hand: "+(isInHand == 1 ? "Left" : "Right"));
        if (null != insideFillable && null != insideFillable.fillableObject)
        {
            FillableManager fillableManager = insideFillable.fillableObject.GetComponent<FillableManager>();
            fillableManager.RemoveGrabbableFromFillable(gameObject);
        }
        if(LevelManager.isDebug){
            debugObjects.center.SetActive(true);
            debugObjects.matrixOrigin.SetActive(true);
        }
    }

    public void OnActivate(ActivateEventArgs args){
        Debug.Log("Grabbable: "+gameObject.name + "has been activated!"+args.interactorObject.transform.name);
        if((args.interactorObject.transform.name.Contains("Left") && isInHand == 1) || (args.interactorObject.transform.name.Contains("Right") && isInHand == 2)){
            SetVisualization(true);
        }
    }
    public void OnDeactivate(DeactivateEventArgs args){
        Debug.Log("Grabbable: "+gameObject.name + "has been deactivated!");
        if((args.interactorObject.transform.name.Contains("Left") && isInHand == 1) || (args.interactorObject.transform.name.Contains("Right") && isInHand == 2)){
            SetVisualization(false);
        }
    }
    public void SetVisualization(bool isSeethrough){
        Debug.Log("Grabbable: "+gameObject.name + "has been toggled! Seethrough: "+isSeethrough);
        GetComponent<Renderer>().material = isSeethrough ? transparentMaterial : defaultMaterial;
    }
}
