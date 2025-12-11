using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

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
    private Dictionary<int, Color> createdGrabbableColors = new Dictionary<int, Color>();


    void Start()
    {
        voxelMeshGenerator = GetComponent<VoxelMeshGenerator>();
        CreateGrid();
        SetFillableVisual();
        gridSize = new Vector3(gridSizeX * voxelSize, gridSizeY * voxelSize, gridSizeZ * voxelSize);
        gridOrigin = transform.position - (gridSize / 2);
        gridEnd = transform.position + (gridSize / 2);
        UpdateUIText();
        CheckForController();
    }

    void OnEnable()
    {
        leftTriggerAction.action.performed += LeftTriggerPressed;
        rightTriggerAction.action.performed += RightTriggerPressed;
        leftGripAction.action.performed += LeftGripPressed;
        leftGripAction.action.canceled += LeftGripPressed;
        rightGripAction.action.performed += RightGripPressed;
        rightGripAction.action.canceled += RightGripPressed;
        Debug.Log("CreateLevel: Actions Enabled");
    }
    void OnDisable()
    {
        leftTriggerAction.action.performed -= LeftTriggerPressed;
        rightTriggerAction.action.performed -= RightTriggerPressed;
        leftGripAction.action.performed -= LeftGripPressed;
        leftGripAction.action.canceled -= LeftGripPressed;
        rightGripAction.action.performed -= RightGripPressed;
        rightGripAction.action.canceled -= RightGripPressed;
        Debug.Log("CreateLevel: Actions Disabled");
    }
    void Update()
    {

        if (leftGripAction.action.IsPressed())
        {
            WhileGripPressed(true);
        }
        if (rightGripAction.action.IsPressed())
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
    void UpdateVoxelInGrid(int x, int y, int z, int id)
    {
        // Update the voxel in the Grid with the specified ID (Only if the ID is different from the current one)
        if (x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY && z >= 0 && z < gridSizeZ && grid[x][y][z] != id)
        {
            if (id == 0)
            {
                id = grid[x][y][z];
                grid[x][y][z] = 0;
            }
            else
            {
                grid[x][y][z] = id;
            }
            UpdateGridVisualization(id);
            UpdateUIText();
            Debug.Log("Voxel Updated: " + x + "," + y + "," + z + " | ID: " + id);
        }
    }
    void UpdateGridVisualization(int id)
    {
        if (!IsIDInUse(id))
        {
            createdGrabbableColors.Remove(id);
            Destroy(GameObject.Find("CreatedGrabbable_" + id));
        }
        else
        {
            //Check if gameobject exists
            GameObject createdGrabbable = GameObject.Find("CreatedGrabbable_" + id);
            if (createdGrabbable == null)
            {
                Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
                voxelMeshGenerator.GenerateCreatedGrabbableObject(
                     position: transform.position,
                     gridSize: new Vector3Int(gridSizeX, gridSizeY, gridSizeZ),
                     voxelSize: voxelSize,
                     voxels: grid,
                     color: color,
                     id: id
                 );
                createdGrabbableColors.Add(id, color);

            }
            else
            {
                voxelMeshGenerator.UpdateCreatedGrabbableObject(
                    createdGrabbable,
                    gridSize: new Vector3Int(gridSizeX, gridSizeY, gridSizeZ),
                    voxelSize: voxelSize,
                    voxels: grid,
                    id: id
                );
            }
        }
    }
    void UpdateUIText()
    {
        // Update the UI text with the current grid state

        int[] existingIDs = grid.SelectMany(layer => layer.SelectMany(row => row))
            .Distinct()
            .Where(id => id != 0)
            .OrderBy(id => id)
            .ToArray();
        Debug.Log("CreateLevelManager: Update UI Text | IDs: " + string.Join(", ", existingIDs) + "| Colors: " + string.Join(", ", createdGrabbableColors.Keys));
        string text = "Gitterbox-Größe: [" + gridSizeX + ", " + gridSizeY + ", " + gridSizeZ + "]\n";
        text += "Voxel-Größe: " + voxelSize + "\n";
        text += "Freie Felder: " + grid.Sum(layer => layer.Sum(row => row.Count(voxel => voxel == 0))) + "\n";
        text += "Farbformen: " + existingIDs.Length + "\n";
        foreach (int i in existingIDs)
        {
            int voxelAmount = grid.Sum(layer => layer.Sum(row => row.Count(voxel => voxel == i)));

            text += $"<color=#{ColorUtility.ToHtmlStringRGB(createdGrabbableColors[i])}>█</color>Farbform {i}: {voxelAmount} Voxel\n";
        }
        UIManager.instance.SetCurrentUIText(text);
    }

    public void ResetLevel()
    {
        //Delete all children
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        //Reset grid
        EmptyGrid(grid);
        UpdateUIText();
        createdGrabbableColors.Clear();
        Debug.Log("CreateLevelManager: Level Reset");
    }
    public void SaveLevel()
    {
        // Save the level as a json file with timestamp

        string levelFilePath;
        // Logging
        if (Application.platform == RuntimePlatform.Android)
        { //Path to Documents in Meta Quest 3
            levelFilePath = "/storage/emulated/0/Documents/Level_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".json";
        }
        else
        {
            levelFilePath = Application.persistentDataPath + "/Level_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".json";
        }
        System.IO.StreamWriter levelFile = new System.IO.StreamWriter(levelFilePath, true);
        string jsonString = @"
{
    ""levels"": [
        {
            ""voxelSize"": " + voxelSize + @",
            ""fillable"": {
                ""size"": [" + gridSizeX + @", " + gridSizeY + @", " + gridSizeZ + @"],
                ""position"": [0, 0, 0]
            },
            ""grabbables"": [" + ConvertCreatedGrabbablesToJson() + @"
            ]
        }
    ]
}";
        levelFile.WriteLine(jsonString);
        levelFile.Close();
        Debug.Log("CreateLevelManager: Level saved to " + levelFilePath);
    }
    private string ConvertCreatedGrabbablesToJson()
    {
        string grabbablesJson = @"";
        for (int i = 1; i < gridSizeX * gridSizeY * gridSizeZ; i++)
        {
            if (IsIDInUse(i))
            {
                if (i > 1) grabbablesJson += @",";
                (int[][][] grabbableGrid, int sizeX, int sizeY, int sizeZ) = IsolateGrabbableFromGrid(i);
                string rawVoxels = ConvertVoxelsToRawVoxels(grabbableGrid);
                string position = DetermineGrabbablePositionById(i);
                string color = $"#{ColorUtility.ToHtmlStringRGB(createdGrabbableColors[i])}";
                grabbablesJson += @"
            {
                ""size"": [" + sizeX + @", " + sizeY + @", " + sizeZ + @"],
                ""rawVoxels"": [" + rawVoxels + @"],
                ""position"": " + position + @",
                ""color"": """ + color + @"""
            }";
            }
        }
        return grabbablesJson;
    }
    private (int[][][] grid, int sizeX, int sizeY, int sizeZ) IsolateGrabbableFromGrid(int id)
    {
        int[][][] grabbableGrid = new int[gridSizeX][][];
        for (int x = 0; x < gridSizeX; x++)
        {
            grabbableGrid[x] = new int[gridSizeY][];
            for (int y = 0; y < gridSizeY; y++)
            {
                grabbableGrid[x][y] = new int[gridSizeZ];
                for (int z = 0; z < gridSizeZ; z++)
                {
                    if (grid[x][y][z] == id)
                    {
                        grabbableGrid[x][y][z] = 1;
                    }
                }
            }
        }
        // Before returning, shrink the grabbable grid to the smallest size that contains all voxels
        return ShrinkGrabbableGrid(grabbableGrid);
    }
    private (int[][][] grid, int sizeX, int sizeY, int sizeZ) ShrinkGrabbableGrid(int[][][] grabbableGrid)
    {
        // Shrink the grid to the smallest size that contains all voxels
        int sizeX = grabbableGrid.Length;
        int sizeY = grabbableGrid[0].Length;
        int sizeZ = grabbableGrid[0][0].Length;
        int minX = sizeX, minY = sizeY, minZ = sizeZ;
        int maxX = 0, maxY = 0, maxZ = 0;
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    if (grabbableGrid[x][y][z] == 1)
                    {
                        if (x < minX) minX = x;
                        if (y < minY) minY = y;
                        if (z < minZ) minZ = z;
                        if (x > maxX) maxX = x;
                        if (y > maxY) maxY = y;
                        if (z > maxZ) maxZ = z;
                    }
                }
            }
        }
        // Create a new grid with the reduced size
        int newSizeX = maxX - minX + 1;
        int newSizeY = maxY - minY + 1;
        int newSizeZ = maxZ - minZ + 1;
        int[][][] newGrabbableGrid = new int[newSizeX][][];
        for (int x = 0; x < newSizeX; x++)
        {
            newGrabbableGrid[x] = new int[newSizeY][];
            for (int y = 0; y < newSizeY; y++)
            {
                newGrabbableGrid[x][y] = new int[newSizeZ];
                for (int z = 0; z < newSizeZ; z++)
                {
                    newGrabbableGrid[x][y][z] = grabbableGrid[x + minX][y + minY][z + minZ];
                }
            }
        }
        return (newGrabbableGrid, newSizeX, newSizeY, newSizeZ);
    }
    private string ConvertVoxelsToRawVoxels(int[][][] grid)
    {
        int sizeX = grid.Length;
        int sizeY = grid[0].Length;
        int sizeZ = grid[0][0].Length;
        string rawVoxels = @"
                    ";
        // Convert the grid to a raw voxel format
        for (int z = 0; z < sizeZ; z++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    rawVoxels += grid[x][y][z];
                    if (!(x == sizeX - 1 && y == sizeY - 1 && z == sizeZ - 1))
                        rawVoxels += ", "; // Add a comma and space between voxels, except for the last one
                    if (x == sizeX - 1) rawVoxels += " ";
                }
                if (y == sizeY - 1) rawVoxels += @"
                    ";
            }
        }
        // Remove the last 4 characters (4 spaces)
        if (rawVoxels.Length >= 4)
            rawVoxels = rawVoxels.Substring(0, rawVoxels.Length - 4);
        return rawVoxels;
    }
    private string DetermineGrabbablePositionById(int id)
    {
        int x = 0, y = 0;

        if (id == 1)
            x = -1;
        else if (id == 2)
            x = 1;
        else
        {
            y = id / 3;
            x = (id % 3) - 1;
        }
        return $"[{x * (gridSizeX + 1)}, {y * (gridSizeY + 1)}, {0}]";
    }

    //***** Input Action Callbacks  *****//

    void LeftTriggerPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            TriggerPressed(true);
        }
    }
    void RightTriggerPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            TriggerPressed(false);
        }
    }
    void TriggerPressed(bool isLeft)
    {
        Debug.Log("CreateLevel: Trigger pressed "+isLeft);
        if (CheckForController()) return;
        // TODO: Wie beim Grip auch während des Drückens die Löschen
        (bool isInsideGrid, int x, int y, int z) = GetVoxelPosition(isLeft ? leftControllerTransform.position : rightControllerTransform.position);
        if (!isInsideGrid)
        {
            Debug.Log("Trigger Pressed: " + (isLeft ? "Left" : "Right") + " |  Grid: Not in grid" + " | Position: " + (isLeft ? leftControllerTransform.position : rightControllerTransform.position));
            return;
        }
        Debug.Log("Trigger Pressed: " + (isLeft ? "Left" : "Right") + " | ID: " + (isLeft ? curObjIDLeft : curObjIDRight) + " | Grid: " + x + "," + y + "," + z + " | Position: " + rightControllerTransform.position);
        UpdateVoxelInGrid(x, y, z, 0);
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
            GripPressed(false);
        }
        else if (context.canceled)
        {
            curObjIDRight = 0;
        }
    }
    void GripPressed(bool isLeft)
    {
        Debug.Log("CreateLevel: Grip pressed "+isLeft);
        if (CheckForController()) return;
        (bool isInsideGrid, int x, int y, int z) = GetVoxelPosition(isLeft ? leftControllerTransform.position : rightControllerTransform.position);
        if (!isInsideGrid)
        {
            Debug.Log("Grip Pressed: " + (isLeft ? "Left" : "Right") + " | Grid: Not in grid" + " | Position: " + (isLeft ? leftControllerTransform.position : rightControllerTransform.position));
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
                    UpdateVoxelInGrid(x, y, z, i);
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
        Debug.Log("Grip Pressed: " + (isLeft ? "Left" : "Right") + " | ID: " + (isLeft ? curObjIDLeft : curObjIDRight) + " | Grid: " + x + "," + y + "," + z + " | Position: " + (isLeft ? leftControllerTransform.position : rightControllerTransform.position));
    }
    void WhileGripPressed(bool isLeft)
    {
        Debug.Log("CreateLevel: While Grip pressed "+isLeft);
        if (CheckForController()) return;
        (bool isInsideGrid, int x, int y, int z) = GetVoxelPosition(isLeft ? leftControllerTransform.position : rightControllerTransform.position);
        if (!isInsideGrid)
            return;
        if (grid[x][y][z] == 0)
        {
            // Check if the voxel is empty, if it is, set it to the current object ID
            UpdateVoxelInGrid(x, y, z, isLeft ? curObjIDLeft : curObjIDRight);
            Debug.Log("While Grip Pressed: " + (isLeft ? "Left" : "Right") + " | ID: " + (isLeft ? curObjIDLeft : curObjIDRight) + " | Grid:" + grid[x][y][z]);
        }
    }
    private bool CheckForController()
    {
        
        if (leftControllerTransform == null) leftControllerTransform = GameObject.Find("XR Origin (XR Rig)").GetComponent<XRInputModalityManager>().leftController.transform;
        if (rightControllerTransform == null) rightControllerTransform = GameObject.Find("XR Origin (XR Rig)").GetComponent<XRInputModalityManager>().rightController.transform;
        return leftControllerTransform == null || rightControllerTransform == null;
    }

}
