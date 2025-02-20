using System.Collections.Generic;
using System.Linq;
using UnityEngine;





public class FillableManager : MonoBehaviour
{
    private Vector3Int gridSize;
    private float voxelSize;
    private float degreeTolerance = 5f; //5 degree in both directions. Max would be 45 degree
    private float distanceTolerance = 0.02f; // 2 cm tolerance for position
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
        // Check if Grabbable fits in Fillable, for example (1, 4, 1) Fillable and (4, 1, 1) Grabbable would fit, as Grabbable can be rotated
        int[] fillableSizeList = new int[]{gridSize.x, gridSize.y, gridSize.z}.OrderByDescending(x => x).ToArray();
        int[] grabbableSizeList = grabbable.size.OrderByDescending(x => x).ToArray();
        for (int i = 0; i < 3; i++)
        {
            if (fillableSizeList[i] < grabbableSizeList[i])
            {
                Debug.Log("Grabbable cannot not fit in Fillable");
                return;
            }
        }
        Vector3 fillablePosition = transform.position;
        Vector3 fillableStartingPoint = fillablePosition - 0.5f * voxelSize * (Vector3)gridSize;
        Vector3 grabbablePosition = grabbableObject.transform.position;
        Vector3Int grabbableGridSize = new Vector3Int(grabbable.size[0], grabbable.size[1], grabbable.size[2]);
        Vector3 grabbableStartingPoint = grabbablePosition - 0.5f * voxelSize * (Vector3)grabbableGridSize;

        Vector3 startingPointDifference = grabbableStartingPoint - fillableStartingPoint;
        
        //TODO: Check with tolerances

        Vector3Int gridOffset = new Vector3Int(Mathf.RoundToInt(startingPointDifference.x / voxelSize), Mathf.RoundToInt(startingPointDifference.y / voxelSize), Mathf.RoundToInt(startingPointDifference.z / voxelSize));
        if (gridOffset.x < 0 || gridOffset.y < 0 || gridOffset.z < 0)
        {
            Debug.Log("Grabbable is outside Fillable");
            return;
        }
        // Check if Grabbable is overlapping with another Grabbable
        for (int x = 0; x < grabbableGridSize.x; x++)
        {
            for (int y = 0; y < grabbableGridSize.y; y++)
            {
                for (int z = 0; z < grabbableGridSize.z; z++)
                {
                    if (grabbable.voxels[x][y][z] == 1)
                    {
                        Vector3Int fillableGridPosition = new Vector3Int(x + gridOffset.x, y + gridOffset.y, z + gridOffset.z);
                        if (fillableGridPosition.x >= gridSize.x || fillableGridPosition.y >= gridSize.y || fillableGridPosition.z >= gridSize.z)
                        {
                            Debug.Log("Grabbable is outside Fillable");
                            return;
                        }
                        if (fillableGrid[fillableGridPosition.x][fillableGridPosition.y][fillableGridPosition.z] == 1)
                        {
                            Debug.Log("Grabbable is overlapping with another Grabbable");
                            return;
                        }
                    }
                }
            }
        }
        // If we reach here, Grabbable fits in Fillable
        // Mark the fillableGrid with 1s
        for (int x = 0; x < grabbableGridSize.x; x++)
        {
            for (int y = 0; y < grabbableGridSize.y; y++)
            {
                for (int z = 0; z < grabbableGridSize.z; z++)
                {
                    if (grabbable.voxels[x][y][z] == 1)
                    {
                        Vector3Int fillableGridPosition = new Vector3Int(x + gridOffset.x, y + gridOffset.y, z + gridOffset.z);
                        fillableGrid[fillableGridPosition.x][fillableGridPosition.y][fillableGridPosition.z] = 1;
                    }
                }
            }
        }
        AddGrabbableToFillable(grabbableObject, gridOffset);
    }
    public void AddGrabbableToFillable(GameObject grabbableObject, Vector3Int gridOffset){
        GrabbableInformation grabbableInformation = grabbableObject.GetComponent<GrabbableInformation>();
        grabbableInformation.insideFillable = new(){
            fillableObject = gameObject,
            gridOffset = gridOffset
        };
        // Disable the BoxCollider of the Grabbable and move it to the corresponding position
        grabbableObject.transform.rotation = Quaternion.identity;
        grabbableObject.transform.position = transform.position - 0.5f * voxelSize * (Vector3)gridSize + (Vector3)gridOffset * voxelSize + 0.5f * voxelSize * new Vector3(grabbableInformation.grabbable.size[0], grabbableInformation.grabbable.size[1], grabbableInformation.grabbable.size[2]);
        DebugFillableGrid();
        Debug.Log("Grid Offset: " + gridOffset);
    }
    public void RemoveGrabbableFromFillable(GameObject grabbableObject){
        GrabbableInformation grabbableInformation = grabbableObject.GetComponent<GrabbableInformation>();
        Vector3Int gridOffset = grabbableInformation.insideFillable.gridOffset;
        grabbableInformation.insideFillable = null;
        // Enable the BoxCollider of the Grabbable and move it to the corresponding position
        grabbableObject.GetComponent<BoxCollider>().enabled = true;
        grabbableObject.transform.position = transform.position - 0.5f * voxelSize * (Vector3)gridSize + (Vector3)gridOffset * voxelSize + 0.5f * voxelSize * new Vector3(grabbableInformation.grabbable.size[0], grabbableInformation.grabbable.size[1], grabbableInformation.grabbable.size[2]);
        // Mark the fillableGrid with 0s
        Grabbable grabbable = grabbableObject.GetComponent<GrabbableInformation>().grabbable;
        Vector3Int grabbableGridSize = new Vector3Int(grabbable.size[0], grabbable.size[1], grabbable.size[2]);
        for (int x = 0; x < grabbableGridSize.x; x++)
        {
            for (int y = 0; y < grabbableGridSize.y; y++)
            {
                for (int z = 0; z < grabbableGridSize.z; z++)
                {
                    if (grabbable.voxels[x][y][z] == 1)
                    {
                        Vector3Int fillableGridPosition = new Vector3Int(x + gridOffset.x, y + gridOffset.y, z + gridOffset.z);
                        fillableGrid[fillableGridPosition.x][fillableGridPosition.y][fillableGridPosition.z] = 0;
                    }
                }
            }
        }
        DebugFillableGrid();
        Debug.Log("Grid Offset: " + gridOffset);
    }
    // Debug the fillableGrid as a function
    public void DebugFillableGrid(){
        //prints out each value of fillableGrid
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                for (int z = 0; z < gridSize.z; z++)
                {
                    Debug.Log("Fillable Grid Position: " + x + ", " + y + ", " + z + " Value: " + fillableGrid[x][y][z]);
                }
            }
        }
    }
}
