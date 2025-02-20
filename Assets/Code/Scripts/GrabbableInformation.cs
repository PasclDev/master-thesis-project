using UnityEngine;

public class GrabbableInformation : MonoBehaviour
{
    public Grabbable grabbable;
    public float voxelSize;

    public void Initialize(Grabbable grabbable, float voxelSize)
    {
        this.grabbable = grabbable;
        this.voxelSize = voxelSize;
    }

}
