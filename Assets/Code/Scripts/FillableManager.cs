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
    public GameObject filledVoxelVisual;

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
        
        //Check tolerances, if rotation is within tolerance, and if position is within tolerance
        Vector3 grabbableRotation = grabbableObject.transform.rotation.eulerAngles;
        if (grabbableRotation.x % 90 >= degreeTolerance && grabbableRotation.x % 90 <= 90 - degreeTolerance ||
            grabbableRotation.y % 90 >= degreeTolerance && grabbableRotation.y % 90 <= 90 - degreeTolerance ||
            grabbableRotation.z % 90 >= degreeTolerance && grabbableRotation.z % 90 <= 90 - degreeTolerance)
        {
            Debug.Log("Rotation is not within tolerance");
            return;
        }else{
            //Round each rotation to 90, 180, 270, or 0, no 360!
            grabbableRotation.x = Mathf.RoundToInt(grabbableRotation.x / 90) * 90;
            grabbableRotation.y = Mathf.RoundToInt(grabbableRotation.y / 90) * 90;
            grabbableRotation.z = Mathf.RoundToInt(grabbableRotation.z / 90) * 90;
        }
        // TODO: Distance tolerance
        
        Vector3Int gridOffset = new Vector3Int(Mathf.RoundToInt(startingPointDifference.x / voxelSize), Mathf.RoundToInt(startingPointDifference.y / voxelSize), Mathf.RoundToInt(startingPointDifference.z / voxelSize));
        if (gridOffset.x < 0 || gridOffset.y < 0 || gridOffset.z < 0)
        {
            Debug.Log("Grabbable is outside Fillable");
            return;
        }
        // Check if Grabbable is overlapping with another Grabbable
        int[][][] rotatedVoxels = RotationHelper.RotateMatrix(grabbable.voxels, (int)grabbableRotation.x/90%4, (int)grabbableRotation.y/90%4, (int)grabbableRotation.z/90%4);
        for (int x = 0; x < rotatedVoxels.Length; x++)
        {
            for (int y = 0; y < rotatedVoxels[x].Length; y++)
            {
                for (int z = 0; z < rotatedVoxels[x][y].Length; z++)
                {
                    if (rotatedVoxels[x][y][z] == 1)
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
        AddGrabbableToFillable(grabbableObject, gridOffset, grabbableRotation, rotatedVoxels);
    }
    public void AddGrabbableToFillable(GameObject grabbableObject, Vector3Int gridOffset, Vector3 grabbableRotation, int[][][] rotatedVoxels){
        GrabbableInformation grabbableInformation = grabbableObject.GetComponent<GrabbableInformation>();
        grabbableInformation.insideFillable = new(){
            fillableObject = gameObject,
            rotatedVoxelMatrix = rotatedVoxels,
            gridOffset = gridOffset
        };
        // Disable the BoxCollider of the Grabbable and move it to the corresponding position
        grabbableObject.transform.rotation = Quaternion.Euler(grabbableRotation);
        grabbableObject.transform.position = transform.position - 0.5f * voxelSize * (Vector3)gridSize + (Vector3)gridOffset * voxelSize + 0.5f * voxelSize * new Vector3(grabbableInformation.grabbable.size[0], grabbableInformation.grabbable.size[1], grabbableInformation.grabbable.size[2]);
        
        GameObject visualizerParent = new GameObject(grabbableObject.GetInstanceID().ToString());
        visualizerParent.transform.position = transform.position - (transform.localScale / 2) + (Vector3.one * voxelSize / 2);
        visualizerParent.transform.rotation = transform.rotation;
        visualizerParent.transform.parent = transform;
        // Mark the fillableGrid with 1s
        for (int x = 0; x < rotatedVoxels.Length; x++)
        {
            for (int y = 0; y < rotatedVoxels[x].Length; y++)
            {
                for (int z = 0; z < rotatedVoxels[x][y].Length; z++)
                {
                    if (rotatedVoxels[x][y][z] == 1)
                    {
                        Vector3Int fillableGridPosition = new Vector3Int(x + gridOffset.x, y + gridOffset.y, z + gridOffset.z);
                        Instantiate(filledVoxelVisual, visualizerParent.transform.position + (Vector3)fillableGridPosition * voxelSize, Quaternion.identity, visualizerParent.transform).transform.localScale = new Vector3(voxelSize*0.9f, voxelSize*0.9f, voxelSize*0.9f);
                        fillableGrid[fillableGridPosition.x][fillableGridPosition.y][fillableGridPosition.z] = 1;
                    }
                }
            }
        }
        DebugFillableGrid();
        Debug.Log("Grid Offset: " + gridOffset);
    }
    public void RemoveGrabbableFromFillable(GameObject grabbableObject){
        GrabbableInformation grabbableInformation = grabbableObject.GetComponent<GrabbableInformation>();
        Vector3Int gridOffset = grabbableInformation.insideFillable.gridOffset;
        // Enable the BoxCollider of the Grabbable and move it to the corresponding position
        grabbableObject.GetComponent<BoxCollider>().enabled = true;
        grabbableObject.transform.position = transform.position - 0.5f * voxelSize * (Vector3)gridSize + (Vector3)gridOffset * voxelSize + 0.5f * voxelSize * new Vector3(grabbableInformation.grabbable.size[0], grabbableInformation.grabbable.size[1], grabbableInformation.grabbable.size[2]);
        // Mark the fillableGrid with 0s
        for (int x = 0; x < grabbableInformation.insideFillable.rotatedVoxelMatrix.Length; x++)
        {
            for (int y = 0; y < grabbableInformation.insideFillable.rotatedVoxelMatrix[x].Length; y++)
            {
                for (int z = 0; z < grabbableInformation.insideFillable.rotatedVoxelMatrix[x][y].Length; z++)
                {
                    if (grabbableInformation.insideFillable.rotatedVoxelMatrix[x][y][z] == 1)
                    {
                        Vector3Int fillableGridPosition = new Vector3Int(x + gridOffset.x, y + gridOffset.y, z + gridOffset.z);
                        fillableGrid[fillableGridPosition.x][fillableGridPosition.y][fillableGridPosition.z] = 0;
                    }
                }
            }
        }
        grabbableInformation.insideFillable = null;
        Destroy(transform.Find(grabbableObject.GetInstanceID().ToString()).gameObject); // Destroys the visualizerParent
        //DebugFillableGrid();
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
