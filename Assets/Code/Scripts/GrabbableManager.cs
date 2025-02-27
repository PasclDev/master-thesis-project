using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;

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
    public bool isGrabbed = false;
    public DebugObjects debugObjects;

    private FillableManager lastTouchedFillable;
    private float distanceTolerance = LevelManager.distanceTolerance;

    public void Initialize(Grabbable grabbable, float voxelSize)
    {
        this.grabbable = grabbable;
        this.voxelSize = voxelSize;
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
    public void OnSelectEnter(){
        isGrabbed = true;
        Debug.Log("Grabbable: "+gameObject.name + "has been picked up!");
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
}
