using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
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
    public Material defaultMaterial;
    public Material insideMaterial; // Material for when the object is inside a fillable. Pastel-like version of the default material
    private int lastMaterial = 0; //0 is off, 1 is to defaultMaterial, 2 is to insideMaterial, 3 is transparentMaterial
    private int targetMaterial = 0; 

    private FillableManager lastTouchedFillable;
    public void Initialize(Grabbable grabbable, float voxelSize)
    {
        this.grabbable = grabbable;
        this.voxelSize = voxelSize;
        defaultMaterial = new Material(GetComponent<Renderer>().material);
        GenerateInsideMaterial(defaultMaterial);
        //Debug.Log(transform.forward + " IS FORWARD" + transform.up + " IS UP" + transform.right + " IS RIGHT");
        
    }

    public void FixedUpdate()
    {
        if(lastMaterial != targetMaterial){ // If the material is not the target material
            if (lastMaterial == 3 || targetMaterial == 3){ //if it was transparent before, instantly change it to the target material, as transparency is not lerpable
                GetComponent<Renderer>().material = GetMaterialById(targetMaterial);
                lastMaterial = targetMaterial;
            } else {
                GetComponent<Renderer>().material.Lerp(GetComponent<Renderer>().material, GetMaterialById(targetMaterial), 0.1f);
                if(GetComponent<Renderer>().material.color == GetMaterialById(targetMaterial).color){
                    Debug.Log ("SetMaterial: "+lastMaterial+" to "+targetMaterial+" has been completed");
                    lastMaterial = targetMaterial;
                }
            }
        }
        if(LevelManager.isDebug && isGrabbed){
            //Always display center at center of the object
            debugObjects.center.transform.position = gameObject.transform.position;

            // Display the matrix origin at the bottom left corner of the object and display if it is a valid rotation
            Vector3Int grabbableGridSize = new Vector3Int(grabbable.size[0], grabbable.size[1], grabbable.size[2]);
            Vector3 grabbableRotation = transform.rotation.eulerAngles;
            (bool isValidRotation, Vector3 up, Vector3 right, Vector3 forward) = RotationHelper.IsValidRotation(transform, LevelManager.rotationTolerancePercentage);
            debugObjects.matrixOrigin.GetComponent<Renderer>().material.color = isValidRotation ? Color.green : Color.red;

            (int rotatedX, int rotatedY, int rotatedZ) = RotationHelper.RotateDimensionSize(grabbableGridSize.x,grabbableGridSize.y, grabbableGridSize.z, up, forward);
            Vector3Int rotatedGridSize = new Vector3Int(rotatedX, rotatedY, rotatedZ);
            debugObjects.rotationText.text = "Rotation: " + transform.rotation.eulerAngles.ToString("F0") + "\nRounded: " + grabbableRotation.ToString("F0")+ "\nUp: " + up.ToString("F0")+ "\nRight: " + right.ToString("F0")+ "\nForward: " + forward.ToString("F0")+"\nRotated Grid Size:"+Vector3Int.RoundToInt(rotatedGridSize).ToString();
            debugObjects.center.transform.up = up;
            debugObjects.matrixOrigin.transform.position = transform.position - 0.5f * voxelSize * (Vector3)rotatedGridSize; // Center - half of voxel size * rotatedGridSize
        }
    }
    private Material GetMaterialById(int id){
        switch(id){
            case 1:
                return defaultMaterial;
            case 2:
                return insideMaterial;
            case 3:
                return transparentMaterial;
            default:
                return defaultMaterial;
        }
    }
    public void SetMaterial(bool isTransparent, bool isInsideFillable){
        if (lastMaterial != targetMaterial) lastMaterial = targetMaterial;
        if (isTransparent)
        {
            targetMaterial = 3;
        }
        else if (isInsideFillable)
        {
            targetMaterial = 2;
        }
        else
        {
            targetMaterial = 1;
        }
        Debug.Log ("SetMaterial: "+lastMaterial+" to "+targetMaterial);
    }
    private void GenerateInsideMaterial(Material material){
        insideMaterial = new Material(material);
        Color.RGBToHSV(material.color, out float h, out float s, out float v);
        insideMaterial.color = Color.HSVToRGB(h, s * 0.50f + 0.1f , 1); // "Pastel" the color
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

    /* ---- Event Handler ---- */
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Fillable"))
        {
            Debug.Log("GrabbableLastTouched: Has entered " + other.name);
            lastTouchedFillable = other.GetComponent<FillableManager>();
        }
    }
    // Needs to be done in OnTriggerStay because OnTriggerExit triggering on meshes can also happen when only part of the mesh is outside the fillable, this ensures that the last touched fillable is always the correct state
    void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Fillable")){
            Debug.Log("GrabbableLastTouched: Is Inside " + other.name);
            if (lastTouchedFillable == null)
            {
                lastTouchedFillable = other.GetComponent<FillableManager>();
            }
        }
    }
    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Fillable")){
            Debug.Log("GrabbableLastTouched: Has exited " + other.name);
            lastTouchedFillable = null;
        }
    }

    
    // Triggers whenever the grabbable gets grabbed by the player
    public void OnSelectEnter(SelectEnterEventArgs args){
        isGrabbed = true;
        isInHand = args.interactorObject.transform.name.Contains("Left") ? 1 : 2;
        StatisticManager.instance.levelStatistic.numberOfGrabs++;
        StatisticManager.instance.SetTimeTilFirstGrab();
        SetMaterial(false, false);
        Debug.Log("Grabbable: "+gameObject.name + "has been picked up! In hand: "+(isInHand == 1 ? "Left" : "Right")); // Warning: Used in tutorial-logic
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
    public void OnSelectExit(){
        isGrabbed = false;
        isInHand = 0;
        SetMaterial(false, false);
        Debug.Log("Grabbable: "+gameObject.name + " has been dropped!");
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

    public void OnActivate(ActivateEventArgs args){
        Debug.Log("Grabbable: "+gameObject.name + " has been activated! "+args.interactorObject.transform.name); // Warning: Used in tutorial-logic
        if((args.interactorObject.transform.name.Contains("Left") && isInHand == 1) || (args.interactorObject.transform.name.Contains("Right") && isInHand == 2)){
            StatisticManager.instance.levelStatistic.numberOfGrabbableTransparency++;
            SetMaterial(true, false);
        }
    }
    public void OnDeactivate(DeactivateEventArgs args){
        Debug.Log("Grabbable: "+gameObject.name + " has been deactivated!");
        if((args.interactorObject.transform.name.Contains("Left") && isInHand == 1) || (args.interactorObject.transform.name.Contains("Right") && isInHand == 2)){
            SetMaterial(false, false);
        }
    }
}
