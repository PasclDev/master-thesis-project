using System.Collections.Generic;
using System.Linq;
using UnityEngine;





public class FillableManager : MonoBehaviour
{
    private Vector3Int gridSize;
    private float voxelSize;
    public int[][][] fillableGrid;
    public GameObject filledVoxelVisual;

    public void Initialize(Vector3 position, Vector3Int gridSize, float voxelSize)
    {
        transform.position = position; 
        //transform.localScale = (Vector3)gridSize * voxelSize* 1.01f; // Scale to fit grid, make it slightly bigger
        GetComponent<BoxCollider>().size = (Vector3)gridSize * voxelSize;
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
        Grabbable grabbable = grabbableObject.GetComponent<GrabbableManager>().grabbable;
        // Check if Grabbable fits in Fillable, for example (1, 4, 1) Fillable and (4, 1, 1) Grabbable would fit, as Grabbable can be rotated
        int[] fillableSizeList = new int[]{gridSize.x, gridSize.y, gridSize.z}.OrderByDescending(x => x).ToArray();
        int[] grabbableSizeList = grabbable.size.OrderByDescending(x => x).ToArray();
        for (int i = 0; i < 3; i++)
        {
            if (fillableSizeList[i] < grabbableSizeList[i])
            {
                Debug.Log("Fillable: Grabbable cannot fit in Fillable (Check 1)");
                return;
            }
        }

        //Check rotation tolerance, if rotation is within tolerance, and if position is within tolerance
        (bool isValidRotation, Vector3 up, Vector3 right, Vector3 forward) = RotationHelper.IsValidRotation(grabbableObject.transform, LevelManager.rotationTolerancePercentage);
        if (!isValidRotation){
            Debug.Log("Fillable: Rotation is not within tolerance");
            return;
        }
        // Rotate the Grabbable gridSize
        (int rotatedGrabbableSizeX, int rotatedGrabbableSizeY, int rotatedGrabbableSizeZ) = RotationHelper.RotateDimensionSize(grabbable.size[0], grabbable.size[1], grabbable.size[2], up, forward);
        Vector3Int rotatedGridSize = new Vector3Int(rotatedGrabbableSizeX, rotatedGrabbableSizeY, rotatedGrabbableSizeZ);
        if (rotatedGridSize.x > gridSize.x || rotatedGridSize.y > gridSize.y || rotatedGridSize.z > gridSize.z)
        {
            Debug.Log("Fillable: Grabbable cannot fit in Fillable (Check 2)");
            return;
        }

        // TODO: Distance tolerance
        
        Vector3 fillablePosition = transform.position;
        Vector3 fillableStartingPoint = fillablePosition - 0.5f * voxelSize * (Vector3)gridSize;
        Vector3 grabbablePosition = grabbableObject.transform.position;
        Vector3 grabbableStartingPoint = grabbablePosition - 0.5f * voxelSize * (Vector3)rotatedGridSize;
        Vector3 startingPointDifference = grabbableStartingPoint - fillableStartingPoint;
        Quaternion newRotation = RotationHelper.OrientationToQuaternion(up, forward, right);

        // TODO: rework
        Vector3Int gridOffset = new Vector3Int(Mathf.RoundToInt(startingPointDifference.x / voxelSize), Mathf.RoundToInt(startingPointDifference.y / voxelSize), Mathf.RoundToInt(startingPointDifference.z / voxelSize));
        if (gridOffset.x < 0 || gridOffset.y < 0 || gridOffset.z < 0)
        {
            Debug.Log("Fillable: Grabbable is outside Fillable (Check 1)");
            return;
        }
        // Check if Grabbable is overlapping with another Grabbable
        int[][][] rotatedVoxels = RotationHelper.RotateMatrix(grabbable.voxels, up, right, forward);
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
                            Debug.Log("Fillable: Grabbable is outside Fillable (Check 2)");
                            return;
                        }
                        if (fillableGrid[fillableGridPosition.x][fillableGridPosition.y][fillableGridPosition.z] == 1)
                        {
                            Debug.Log("Fillable: Grabbable is overlapping with another Grabbable");
                            return;
                        }
                    }
                }
            }
        }
        // If we reach here, Grabbable fits in Fillable
        AddGrabbableToFillable(grabbableObject, gridOffset, grabbableObject.transform.rotation.eulerAngles, rotatedVoxels, rotatedGridSize, newRotation);
    }
    public void AddGrabbableToFillable(GameObject grabbableObject, Vector3Int gridOffset, Vector3 grabbableRotation, int[][][] rotatedVoxels, Vector3 rotatedVoxelGridSize, Quaternion newRotation){
        GrabbableManager grabbableInformation = grabbableObject.GetComponent<GrabbableManager>();
        grabbableInformation.insideFillable = new(){
            fillableObject = gameObject,
            rotatedVoxelMatrix = rotatedVoxels,
            gridOffset = gridOffset
        };
        grabbableInformation.SetInsideMaterial(true);
        // Move it to the corresponding position
        grabbableObject.transform.rotation = newRotation;
        grabbableObject.transform.position = transform.position - 0.5f * voxelSize * (Vector3)gridSize + (Vector3)gridOffset * voxelSize + 0.5f * voxelSize * rotatedVoxelGridSize;
        
        /* Debug Visualizer
        GameObject visualizerParent = new GameObject(grabbableObject.GetInstanceID().ToString());
        visualizerParent.transform.position = transform.position - (transform.localScale / 2) + (Vector3.one * voxelSize / 2);
        visualizerParent.transform.rotation = transform.rotation;
        visualizerParent.transform.parent = transform;*/
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
                        //Debug Visualizer: Instantiate(filledVoxelVisual, visualizerParent.transform.position + (Vector3)fillableGridPosition * voxelSize, Quaternion.identity, visualizerParent.transform).transform.localScale = new Vector3(voxelSize*0.9f, voxelSize*0.9f, voxelSize*0.9f);
                        fillableGrid[fillableGridPosition.x][fillableGridPosition.y][fillableGridPosition.z] = 1;
                    }
                }
            }
        }
        //DebugFillableGrid();
        Debug.Log("Fillable: "+grabbableObject.name+" Grid Offset: " + gridOffset);
        CheckIfFillableIsFilled();
    }
    public void RemoveGrabbableFromFillable(GameObject grabbableObject){
        GrabbableManager grabbableInformation = grabbableObject.GetComponent<GrabbableManager>();
        grabbableInformation.SetInsideMaterial(false);
        Vector3Int gridOffset = grabbableInformation.insideFillable.gridOffset;
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
        // Visualizer Destroy: Destroy(transform.Find(grabbableObject.GetInstanceID().ToString()).gameObject); // Destroys the visualizerParent
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
                    Debug.Log("Fillable: Grid Position: " + x + ", " + y + ", " + z + " Value: " + fillableGrid[x][y][z]);
                }
            }
        }
    }
    private void CheckIfFillableIsFilled(){
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                for (int z = 0; z < gridSize.z; z++)
                {
                    if (fillableGrid[x][y][z] == 0)
                    {
                        return;
                    }
                }
            }
        }
        Debug.Log("Fillable: Fillable is filled!");
        LevelManager.instance.FillablesFilled();
    }
}
