using UnityEngine;

[System.Serializable]
public class InsideFillable
{
    public GameObject fillableObject;
    public int[][][] rotatedVoxelMatrix;
    public Vector3Int gridOffset;
}
public class GrabbableInformation : MonoBehaviour
{
    public Grabbable grabbable;
    public float voxelSize;
    public InsideFillable insideFillable;

    private FillableManager lastTouchedFillable;

    public void Initialize(Grabbable grabbable, float voxelSize)
    {
        this.grabbable = grabbable;
        this.voxelSize = voxelSize;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Grabbable has entered " + other.name);
        if(other.CompareTag("Fillable"))
        {
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
        Debug.Log(gameObject.name + "has been dropped!");
        if(lastTouchedFillable != null){
            lastTouchedFillable.CheckIfGrabbableFitsFillable(gameObject);
        }
        else{
            Debug.Log("Grabbable is not inside any Fillable");
        }
    }
    // Triggers whenever the grabbable gets grabbed by the player
    public void OnSelectEnter(){
        Debug.Log(gameObject.name + "has been picked up!");
        if (null != insideFillable && null != insideFillable.fillableObject)
        {
            FillableManager fillableManager = insideFillable.fillableObject.GetComponent<FillableManager>();
            fillableManager.RemoveGrabbableFromFillable(gameObject);
        }
    }
}
