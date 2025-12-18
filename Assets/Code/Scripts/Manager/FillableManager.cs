using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;


// TODO: Previously, the Rotation of the Fillable was fixed to be (0,0,0). Rework to allow rotated Fillables!
public class FillableManager : MonoBehaviour
{
    private Vector3Int gridSize;
    private float voxelSize;
    public int[][][] fillableGrid;
    public GameObject filledVoxelVisual;
    public InputActionReference leftTriggerAction;
    public InputActionReference rightTriggerAction;

    private VoxelMeshGenerator voxelMeshGenerator;
    public List<GrabbableManager> currentGrabbableObjects = new List<GrabbableManager>();
    private bool isCurrentlyHighlighted = false;

    public void Initialize(Vector3 position, Vector3Int gridSize, float voxelSize, VoxelMeshGenerator voxelMeshGenerator)
    {
        transform.position = position;
        Vector3 size = (Vector3)gridSize * voxelSize;
        GetComponent<BoxCollider>().size = size;
        this.gridSize = gridSize;
        this.voxelSize = voxelSize;
        this.voxelMeshGenerator = voxelMeshGenerator;
        InitializeFillableGrid();
        LevelManager.instance.transform.Find("LevelGrabbable").localScale = new Vector3(size.z, size.x, 0.02f); // Adjusted to match rotated LevelGrabbable
    }
    void OnEnable()
    {
        leftTriggerAction.action.performed += LeftTriggerPressed;
        leftTriggerAction.action.canceled += LeftTriggerPressed;
        rightTriggerAction.action.performed += RightTriggerPressed;
        rightTriggerAction.action.canceled += RightTriggerPressed;
    }
    void OnDisable()
    {
        leftTriggerAction.action.performed -= LeftTriggerPressed;
        leftTriggerAction.action.canceled -= LeftTriggerPressed;
        rightTriggerAction.action.performed -= RightTriggerPressed;
        rightTriggerAction.action.canceled -= RightTriggerPressed;
    }
    public void InitializeFillableGrid()
    {
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

    public void CheckIfGrabbableFitsFillable(GameObject grabbableObject)
    {
        Grabbable grabbable = grabbableObject.GetComponent<GrabbableManager>().grabbable;
        // Check if Grabbable fits in Fillable, for example (1, 4, 1) Fillable and (4, 1, 1) Grabbable would fit, as both can be rotated
        int[] fillableSizeList = new int[] { gridSize.x, gridSize.y, gridSize.z }.OrderByDescending(x => x).ToArray();
        int[] grabbableSizeList = grabbable.size.OrderByDescending(x => x).ToArray();
        for (int i = 0; i < 3; i++)
        {
            if (fillableSizeList[i] < grabbableSizeList[i])
            {
                Debug.Log("Fillable: (Check 1 failed) Grabbable cannot fit in Fillable (too big)");
                return;
            }
        }
        //Check rotation tolerance, if rotation is within tolerance
        if (!RotationHelper.IsValidRotation(transform, grabbableObject.transform, LevelManager.rotationTolerancePercentage))
        {
            Debug.Log("Fillable: (Check 2 failed) Rotation is not within tolerance");
            return;
        }
        // Check if position is within tolerance
            // Currently unimplemented. No position tolerance check needed, always snapping to the closest position.

        // Check if the grabbable can fit in the fillable at all with correct rotation
        

        /* 
            1. Remove rotation from fillable from grabbable
            2. Calculate grid offset from fillable to grabbable in fillable local space (local space is whats needed for accessing the matrix)
            3. Subtract grabbable size depending on the right rotation to get the real offset
        */
        // Get the rotation of grabbable depending on fillable. if fillable is rotated by 90 degrees on y axis, grabbable needs to be rotated by -90 degrees on y axis to fit correctly
        Vector3 fillablePosition = transform.position;
        Vector3 grabbablePosition = grabbableObject.transform.position;
        Vector3 positionDifference = grabbablePosition - fillablePosition; // Vector from fillable to grabbable center
        Quaternion rotationDifference = Quaternion.Inverse(transform.rotation) * grabbableObject.transform.rotation; // Just to get the correct rotation difference
        // Rotate the position difference according to the rotation difference so that it matches the fillable's local space (local space is whats needed for accessing the matrix)
        positionDifference = Quaternion.Inverse(transform.rotation) * positionDifference; // Rotate position difference to remove fillable rotation
     
        //Quaternion fillableRotationInverse = Quaternion.Inverse(transform.rotation); // grabbable rotation with fillable rotation removed. so if fillable is rotated by 90 degrees on y axis, grabbable rotation gets rotated by -90 degrees on y axis so its "neutral" to fillable
        //Vector3 rotatedPositionDifference = fillableRotationInverse * positionDifference; // Rotate the position difference into fillable local space
        Vector3 gridPositionOffset = positionDifference / voxelSize + 0.5f * (Vector3)gridSize; // Calculate grid offset in fillable local space

        // subtract the rotated grabbable size to get the correct grid offset 
        
        (int rotatedGrabbableSizeX, int rotatedGrabbableSizeY, int rotatedGrabbableSizeZ) = RotationHelper.RotateDimensionSize(grabbable.size[0], grabbable.size[1], grabbable.size[2],
            RotationHelper.worldAxes[RotationHelper.FindClosestAxis(rotationDifference * Vector3.up, RotationHelper.worldAxes)],
            RotationHelper.worldAxes[RotationHelper.FindClosestAxis(rotationDifference * Vector3.forward, RotationHelper.worldAxes)]);
        Vector3Int rotatedToWorldGrabbableGridSize = new Vector3Int(rotatedGrabbableSizeX, rotatedGrabbableSizeY, rotatedGrabbableSizeZ);
        Vector3 rotatedGrabbableOffset = 0.5f * (Vector3)rotatedToWorldGrabbableGridSize;
        Vector3Int gridOffset = Vector3Int.RoundToInt(gridPositionOffset - rotatedGrabbableOffset);
        if (gridOffset.x < 0 || gridOffset.y < 0 || gridOffset.z < 0)
        {
            Debug.Log("Fillable: (Check 3 failed) Grabbable is outside Fillable - GridOffset: " + gridOffset);
            return;
        }
        Debug.Log("Fillable: Calculated GridOffset: " + gridOffset + " from posOffset: "+ gridPositionOffset+ "with grabOffset" + rotatedGrabbableOffset+ "| Position Difference: " + positionDifference + " and rotationDiff: "+ rotationDifference.eulerAngles);

        Vector3 grabbableAxisClosestUp = RotationHelper.worldAxes[RotationHelper.FindClosestAxis(rotationDifference * Vector3.up, RotationHelper.worldAxes)];
        Vector3 grabbableAxisClosestForward = RotationHelper.worldAxes[RotationHelper.FindClosestAxis(rotationDifference * Vector3.forward, RotationHelper.worldAxes)];
        Vector3 grabbableAxisClosestRight = RotationHelper.worldAxes[RotationHelper.FindClosestAxis(rotationDifference * Vector3.right, RotationHelper.worldAxes)];

        // Check if grabbable is overlapping with another Grabbable
        int[][][] rotatedGrbVoxels = RotationHelper.RotateMatrix(grabbable.voxels, grabbableAxisClosestUp, grabbableAxisClosestRight, grabbableAxisClosestForward);
        for (int x = 0; x < rotatedGrbVoxels.Length; x++)
        {
            for (int y = 0; y < rotatedGrbVoxels[x].Length; y++)
            {
                for (int z = 0; z < rotatedGrbVoxels[x][y].Length; z++)
                {
                    if (rotatedGrbVoxels[x][y][z] == 1)
                    {
                        Vector3Int grbCurVoxelGridPos = new Vector3Int(x + gridOffset.x, y + gridOffset.y, z + gridOffset.z);
                        if (grbCurVoxelGridPos.x >= gridSize.x || grbCurVoxelGridPos.y >= gridSize.y || grbCurVoxelGridPos.z >= gridSize.z)
                        {
                            Debug.Log("Fillable: (Check 4 failed) Grabbable is outside Fillable" + "- Voxel Grid Position: " + grbCurVoxelGridPos);
                            return;
                        }
                        if (fillableGrid[grbCurVoxelGridPos.x][grbCurVoxelGridPos.y][grbCurVoxelGridPos.z] == 1)
                        {
                            Debug.Log("Fillable: (Check 5 failed) Grabbable is overlapping with another Grabbable");
                            AudioManager.instance.Play("Fillable_Overlap");
                            return;
                        }
                    }
                }
            }
        }

        // If all checks are passed, grabbable fits in Fillable
        Quaternion newGrabbableRotation = transform.rotation * Quaternion.LookRotation(grabbableAxisClosestForward, grabbableAxisClosestUp); 
        AddGrabbableToFillable(grabbableObject, gridOffset, rotatedGrbVoxels, rotatedToWorldGrabbableGridSize, newGrabbableRotation);
        }
    public void AddGrabbableToFillable(GameObject grabbableObject, Vector3Int gridOffset, int[][][] rotatedVoxels, Vector3 rotatedVoxelGridSize, Quaternion newRotation)
    {
        GrabbableManager grabbableInformation = grabbableObject.GetComponent<GrabbableManager>();
        StatisticManager.instance.levelStatistic.numberOfSnapsToFillables++;
        AudioManager.instance.Play("Fillable_Snap");
        grabbableInformation.insideFillable = new()
        {
            fillableObject = gameObject,
            rotatedVoxelMatrix = rotatedVoxels,
            gridOffset = gridOffset
        };
        currentGrabbableObjects.Add(grabbableInformation);
        grabbableInformation.SetMaterial(false, true);
        // Move it to the corresponding position
        grabbableObject.transform.rotation = newRotation;
        grabbableObject.transform.localPosition = transform.localPosition - 0.5f * voxelSize * (Vector3)gridSize + (Vector3)gridOffset * voxelSize + 0.5f * voxelSize * rotatedVoxelGridSize;

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
        Debug.Log("Fillable: " + grabbableObject.name + " Grid Offset: " + gridOffset);
        CheckIfFillableIsFilled();
    }
    public void RemoveGrabbableFromFillable(GameObject grabbableObject)
    {
        GrabbableManager grabbableInformation = grabbableObject.GetComponent<GrabbableManager>();
        currentGrabbableObjects.Remove(grabbableInformation);
        AudioManager.instance.Play("Fillable_Unsnap");
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
    public void DebugFillableGrid()
    {
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
    private void CheckIfFillableIsFilled()
    {
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
    public void LeftTriggerPressed(InputAction.CallbackContext context)
    {
        OnFillableMissingHighlight(context, true);
    }
    public void RightTriggerPressed(InputAction.CallbackContext context)
    {
        OnFillableMissingHighlight(context, false);
    }

    public void OnFillableMissingHighlight(InputAction.CallbackContext context, bool isLeft)
    {
        if (context.performed)
        {
            HighlightMissingVoxels();
        }
        else if (context.canceled && isCurrentlyHighlighted)
        {
            Debug.Log("Fillable: Highlight deactivated");
            AudioManager.instance.Play("Fillable_Unhighlight");
            // Deletes highlight
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            foreach (GrabbableManager grabbableInformation in currentGrabbableObjects)
            {
                grabbableInformation.SetMaterial(false, true);
            }
        }
    }
    public void HighlightMissingVoxels()
    {
        Debug.Log("Fillable: Highlight activated"); // Warning: Used in tutorial-logic
        isCurrentlyHighlighted = true;
        AudioManager.instance.Play("Fillable_Highlight");
        // Generate a mesh of the missing voxels with VoxelMeshGenerator and set each grabbable object to transparent
        voxelMeshGenerator.GenerateFillableMissingHighlight(transform, gridSize, voxelSize, fillableGrid);
        StatisticManager.instance.levelStatistic.numberOfFillableTransparency++;
        foreach (GrabbableManager grabbableInformation in currentGrabbableObjects)
        {
            grabbableInformation.SetMaterial(true, true);
        }

    }
}
