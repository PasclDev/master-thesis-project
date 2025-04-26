using UnityEngine;
using UnityEngine.InputSystem;

public class CreateLevelManager : MonoBehaviour
{
    public int gridSizeX = 3, gridSizeY = 3, gridSizeZ = 3;
    public float voxelSize = 0.08f;
    public int[][][] grid;
    private VoxelMeshGenerator voxelMeshGenerator;
    public InputActionReference leftTriggerAction;
    public InputActionReference rightTriggerAction;
    public InputActionReference leftGripAction;
    public InputActionReference rightGripAction;

    public Transform leftControllerTransform;
    public Transform rightControllerTransform;

    public int curObjIDLeft = 0;
    public int curObjIDRight = 0;

    private Vector3 gridSize;
    private Vector3 gridOrigin;
    private Vector3 gridEnd;
    void Start()
    {
        voxelMeshGenerator = GetComponent<VoxelMeshGenerator>();
        CreateGrid();
        SetFillableVisual();
        gridSize = new Vector3(gridSizeX * voxelSize, gridSizeY * voxelSize, gridSizeZ * voxelSize);
        gridOrigin = transform.position - (gridSize / 2);
        gridEnd = transform.position + (gridSize / 2);
    }

    void OnEnable()
    {
        leftTriggerAction.action.performed += LeftTriggerPressed;
        rightTriggerAction.action.performed += RightTriggerPressed;
        leftGripAction.action.performed += LeftGripPressed;
        leftGripAction.action.canceled += LeftGripPressed;
        rightGripAction.action.performed += RightGripPressed;
        rightGripAction.action.canceled += RightGripPressed;
    }
    void OnDisable()
    {
        leftTriggerAction.action.performed -= LeftTriggerPressed;
        rightTriggerAction.action.performed -= RightTriggerPressed;
        leftGripAction.action.performed -= LeftGripPressed;
        leftGripAction.action.canceled -= LeftGripPressed;
        rightGripAction.action.performed -= RightGripPressed;
        rightGripAction.action.canceled -= RightGripPressed;
    }
    void Update()
    {

        if (leftGripAction.action.triggered)
        {
            WhileGripPressed(true);
        }
        if (rightGripAction.action.triggered)
        {
            WhileGripPressed(false);
        }
    }
    void CreateGrid()
    {
        // Initialize the grid with the specified size
        grid = new int[gridSizeX][][];
        for (int x = 0; x < gridSizeX; x++)
        {
            grid[x] = new int[gridSizeY][];
            for (int y = 0; y < gridSizeY; y++)
            {
                grid[x][y] = new int[gridSizeZ];
            }
        }
        EmptyGrid(grid);
    }
    void EmptyGrid(int[][][] grid)
    {
        // Initialize the grid with zeros (empty cells)
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    grid[x][y][z] = 0;
                }
            }
        }
    }
    void SetFillableVisual()
    {
        // Create a fillable object using the VoxelMeshGenerator and assign it to the MeshFilter and MeshRenderer components of this GameObject
        GameObject fillable = voxelMeshGenerator.GenerateFillableObject(
            voxelSize,
            new Fillable
            {
                size = new int[] { gridSizeX, gridSizeY, gridSizeZ },
                position = new float[] { 0, 0, 0 }
            }
        );
        GetComponent<BoxCollider>().size = new Vector3(gridSizeX * voxelSize, gridSizeY * voxelSize, gridSizeZ * voxelSize);
        GetComponent<MeshRenderer>().material = fillable.GetComponent<MeshRenderer>().material;
        GetComponent<MeshFilter>().mesh = fillable.GetComponent<MeshFilter>().mesh;
        Destroy(fillable);
    }

    bool IsIDInUse(int id)
    {
        for (int i = 0; i < gridSizeX; i++)
        {
            for (int j = 0; j < gridSizeY; j++)
            {
                for (int k = 0; k < gridSizeZ; k++)
                {
                    if (grid[i][j][k] == id)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    (bool isInsideGrid, int x, int y, int z) GetVoxelPosition(Vector3 controllerPos)
    {
        if (controllerPos.x > gridOrigin.x && controllerPos.x < gridEnd.x &&
            controllerPos.y > gridOrigin.y && controllerPos.y < gridEnd.y &&
            controllerPos.z > gridOrigin.z && controllerPos.z < gridEnd.z)
        {
            // Calculate the voxel position in the grid
            return (
                true,
                Mathf.FloorToInt((controllerPos.x - gridOrigin.x) / voxelSize),
                Mathf.FloorToInt((controllerPos.y - gridOrigin.y) / voxelSize),
                Mathf.FloorToInt((controllerPos.z - gridOrigin.z) / voxelSize)
            );
        }
        return (false, -1, -1, -1);
    }
    void LeftTriggerPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Handle left trigger pressed event
            Debug.Log("Left Trigger: Pressed");
        }
    }
    void RightTriggerPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Handle right trigger pressed event
            Debug.Log("Right Trigger: Pressed");
        }
    }
    void LeftGripPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GripPressed(true);
        }
        else if (context.canceled)
        {
            curObjIDLeft = 0;
        }
    }
    void RightGripPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Right Grip: Pressed" + rightControllerTransform.position);
            GripPressed(false);
        }
        else if (context.canceled)
        {
            Debug.Log("Right Grip: Released" + rightControllerTransform.position);
            curObjIDRight = 0;
        }
    }
    void GripPressed(bool isLeft)
    {
        (bool isInsideGrid, int x, int y, int z) = GetVoxelPosition(isLeft ? leftControllerTransform.position : rightControllerTransform.position);
        if (!isInsideGrid)
        {
            Debug.Log("Grip Pressed: " + (isLeft ? "Left" : "Right") + " Grid: Not in grid" + " Position: " + (isLeft ? leftControllerTransform.position : rightControllerTransform.position));
            return;
        }
        if (grid[x][y][z] == 0)
        {
            // Check if the voxel is empty,  set curObjIDLeft to the next free ID
            for (int i = 1; i < gridSizeX * gridSizeY * gridSizeZ; i++)
            {
                if (!IsIDInUse(i))
                {
                    if (isLeft) curObjIDLeft = i;
                    else curObjIDRight = i;
                    grid[x][y][z] = i;
                    break;
                }
            }
        }
        else
        {
            // If the voxel is not empty, set curObjIDLeft to the ID of the voxel
            if (isLeft) curObjIDLeft = grid[x][y][z];
            else curObjIDRight = grid[x][y][z];
        }
        Debug.Log("Grip Pressed: " + (isLeft ? "Left" : "Right") + " ID: " + (isLeft ? curObjIDLeft : curObjIDRight));
    }
    void WhileGripPressed(bool isLeft)
    {
        (bool isInsideGrid, int x, int y, int z) = GetVoxelPosition(isLeft ? leftControllerTransform.position : rightControllerTransform.position);
        if (!isInsideGrid)
            return;
        if (grid[x][y][z] == 0)
        {
            // Check if the voxel is empty, if it is, set it to the current object ID
            grid[x][y][z] = isLeft ? curObjIDLeft : curObjIDRight;
            Debug.Log("While Grip Pressed: " + (isLeft ? "Left" : "Right") + " ID: " + (isLeft ? curObjIDLeft : curObjIDRight) + "Grid:" + grid[x][y][z]);
        }
    }

}
