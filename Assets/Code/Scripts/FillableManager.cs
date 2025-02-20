using System.Linq;
using UnityEngine;

public class FillableManager : MonoBehaviour
{
    private Vector3Int gridSize;
    private float voxelSize;
    public int[][][] fillableGrid;

    public void Initialize(Vector3Int gridSize, float voxelSize)
    {
        this.gridSize = gridSize;
        this.voxelSize = voxelSize;
        InitializeFillableGrid();
    }
    public void InitializeFillableGrid(){
        fillableGrid = new int[gridSize.x][][];
        for (int x = 0; x < gridSize.x; x++)
        {
            fillableGrid[x] = new int[gridSize.y][];
            for (int y = 0; y < gridSize.y; y++)
            {
                fillableGrid[x][y] = new int[gridSize.z];
            }
        }
    }

    public void CheckIfGrabbableFitsFillable(GameObject grabbableObject){
        Grabbable grabbable = grabbableObject.GetComponent<GrabbableInformation>().grabbable;
        Vector3Int grabbableSize = new Vector3Int(grabbable.voxels.Length, grabbable.voxels[0].Length, grabbable.voxels[0][0].Length);
        // Check if Grabbable fits in Fillable, for example (1, 4, 1) Fillable and (4, 1, 1) Grabbable would fit, as Grabbable can be rotated
        int[] fillableSizeList = new int[]{gridSize.x, gridSize.y, gridSize.z}.OrderByDescending(x => x).ToArray();
        int[] grabbableSizeList = new int[]{grabbableSize.x, grabbableSize.y, grabbableSize.z}.OrderByDescending(x => x).ToArray();
        for (int i = 0; i < 3; i++)
        {
            if (fillableSizeList[i] < grabbableSizeList[i])
            {
                Debug.Log("Grabbable cannot not fit in Fillable");
                return;
            }
        }
        Vector3 fillablePosition = transform.position;
        Vector3 grabbablePosition = grabbableObject.transform.position;


    }
}
