using System;
using UnityEngine;

public class RotationHelper
{
    public static Vector3[] worldAxes = {
            Vector3.right, Vector3.up, Vector3.forward,
            Vector3.left, Vector3.down, Vector3.back
    };
    // Rotate a matrix of voxels by the orientation of the object
    public static int[][][] RotateMatrix(int[][][] matrix, Vector3 up, Vector3 right, Vector3 forward)
    {
        int sizeX = matrix.Length;
        int sizeY = matrix[0].Length;
        int sizeZ = matrix[0][0].Length;

        // Determine new dimensions based on axis swaps
        (int newSizeX, int newSizeY, int newSizeZ) = RotateDimensionSize(sizeX, sizeY, sizeZ, up, forward);

        Debug.Log("RotationHelper: Rotate Matrix, previous size: " + sizeX + " " + sizeY + " " + sizeZ + " and new size: " + newSizeX + " " + newSizeY + " " + newSizeZ + " with up: " + up + " right: " + right + " forward: " + forward);

        // Create the new rotated int[][][] matrix with the correct dimensions
        int[][][] rotatedMatrix = new int[newSizeX][][];
        for (int x = 0; x < newSizeX; x++)
        {
            rotatedMatrix[x] = new int[newSizeY][];
            for (int y = 0; y < newSizeY; y++)
            {
                rotatedMatrix[x][y] = new int[newSizeZ];
            }
        }

        // Rotate each voxel correctly
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    (int xRotation, int yRotation, int zRotation) = AxisRotationAmount(up, right, forward);
                    (int newX, int newY, int newZ) = RotateIndices(x, y, z, xRotation, yRotation, zRotation, sizeX, sizeY, sizeZ);
                    rotatedMatrix[newX][newY][newZ] = matrix[x][y][z];
                }
            }
        }

        return rotatedMatrix;
    }
    // Rotates dimension size depending on the up and forward axis of the object (depending on the orientation of the object)
    public static (int, int, int) RotateDimensionSize(int x, int y, int z, Vector3 up, Vector3 forward)
    {
        if (up == Vector3.up || up == Vector3.down)
        {
            if (forward == Vector3.forward || forward == Vector3.back)
            {
                return (x, y, z);
            }
            else if (forward == Vector3.right || forward == Vector3.left)
            {
                return (z, y, x);
            }
        }
        else if (up == Vector3.right || up == Vector3.left)
        {
            if (forward == Vector3.forward || forward == Vector3.back)
            {
                return (y, x, z);
            }
            else if (forward == Vector3.up || forward == Vector3.down)
            {
                return (y, z, x);
            }
        }
        else if (up == Vector3.forward || up == Vector3.back)
        {
            if (forward == Vector3.up || forward == Vector3.down)
            {
                return (x, z, y);
            }
            else if (forward == Vector3.right || forward == Vector3.left)
            {
                return (z, x, y);
            }
        }
        return (x, y, z);
    }
    // Rotates the indices of a voxel in the matrix depending on the rotation around each axis
    private static (int, int, int) RotateIndices(int x, int y, int z, int xRotation, int yRotation, int zRotation, int xLength, int yLength, int zLength)
    {
        int newX = x;
        int newY = y;
        int newZ = z;

        //Rotation around the x-axis, if i had an empty 3x3x3 grid with a 1 at (2,0,0), rotating it once would result with the 1 being at (2,2,0), once again at (2,2,2), and finally at (2,0,2)
        for (int ix = 0; ix < xRotation; ix++)
        {
            (newY, newZ) = (zLength - 1 - newZ, newY); //(x,y,z) = (x,!z,y)
            (xLength, yLength, zLength) = RotateDimensionSize(xLength, yLength, zLength, 1, 0, 0);

        }
        //Rotation around the y-axis, if i had an empty 3x3x3 grid with a 1 at (2,0,0), rotating it once would result with the 1 being at (0,0,0), once again at (0,0,2), and finally at (2,0,2)
        for (int iy = 0; iy < yRotation; iy++)
        {
            (newX, newZ) = (newZ, xLength - 1 - newX); //(x,y,z) = (z,y,!x)
            (xLength, yLength, zLength) = RotateDimensionSize(xLength, yLength, zLength, 0, 1, 0);
        }
        //Rotation around the z-axis, if i had an empty 3x3x3 grid with a 1 at (2,0,0), rotating it once would result with the 1 being at (2,2,0), once again at (0,2,0), and finally at (0,0,0)
        for (int iz = 0; iz < zRotation; iz++)
        {
            (newX, newY) = (yLength - 1 - newY, newX); //(x,y,z) = (!y,x,z)
            (xLength, yLength, zLength) = RotateDimensionSize(xLength, yLength, zLength, 0, 0, 1);
        }
        return (newX, newY, newZ);
    }
    //xRotation, yRotation, zRotation are either 0, 1, 2 or 3 and determine how often the matrix is rotated by 90 degrees
    //For example, if xRotation = 1, the matrix is rotated by 90 degrees around the x-axis, different from transform.rotation degrees, as they get calculated differently!
    // Rotate the size of a matrix int[][][], so if the matrix is (1,3,1) and is rotated once on the x-axis, the new size is (1,1,3)
    public static (int, int, int) RotateDimensionSize(int x, int y, int z, int xRotation, int yRotation, int zRotation)
    {
        int newX = x;
        int newY = y;
        int newZ = z;
        if (xRotation % 2 == 1)
        {
            (newY, newZ) = (newZ, newY);
        }
        if (yRotation % 2 == 1)
        {
            (newX, newZ) = (newZ, newX);
        }
        if (zRotation % 2 == 1)
        {
            (newX, newY) = (newY, newX);
        }
        return (newX, newY, newZ);
    }

    //Return how many x, y and z rotations (1 unit = 90 degree) are needed to align the matrix with the object orientation
    public static (int, int, int) AxisRotationAmount(Vector3 up, Vector3 right, Vector3 forward)
    {
        if (up == Vector3.up)
        {
            if (forward == Vector3.forward) return (0, 0, 0);
            else if (forward == Vector3.back) return (0, 2, 0);
            else if (forward == Vector3.right) return (0, 1, 0);
            else if (forward == Vector3.left) return (0, 3, 0);
        }
        else if (up == Vector3.down)
        {
            if (forward == Vector3.forward) return (0, 0, 2);
            else if (forward == Vector3.back) return (2, 0, 0);
            else if (forward == Vector3.right) return (2, 3, 0);
            else if (forward == Vector3.left) return (0, 1, 2);
        }
        else if (up == Vector3.right)
        {
            if (forward == Vector3.forward) return (0, 0, 3);
            else if (forward == Vector3.back) return (2, 0, 1);
            else if (forward == Vector3.up) return (3, 3, 0);
            else if (forward == Vector3.down) return (1, 1, 0);
        }
        else if (up == Vector3.left)
        {
            if (forward == Vector3.forward) return (0, 0, 1);
            else if (forward == Vector3.back) return (2, 0, 3);
            else if (forward == Vector3.up) return (3, 1, 0);
            else if (forward == Vector3.down) return (1, 3, 0);
        }
        else if (up == Vector3.forward)
        {
            if (forward == Vector3.down) return (1, 0, 0);
            else if (forward == Vector3.up) return (3, 2, 0);
            else if (forward == Vector3.left) return (1, 0, 3);
            else if (forward == Vector3.right) return (1, 0, 1);
        }
        else if (up == Vector3.back)
        {
            if (forward == Vector3.down) return (1, 2, 0);
            else if (forward == Vector3.up) return (3, 0, 0);
            else if (forward == Vector3.left) return (3, 0, 1);
            else if (forward == Vector3.right) return (3, 0, 3);
        }
        return (0, 0, 0);
    }
    // Checks if the orientation of the object is within the rotationTolerancePercentage
    public static bool IsValidRotation(Transform fillable, Transform grabbable, float rotationTolerancePercentage)
    {
        // Get the object's transform axes directly
        Vector3 right = grabbable.right;     // Local X
        Vector3 up = grabbable.up;           // Local Y
        Vector3 forward = grabbable.forward; // Local Z

        // Check if the object is aligned with fillable axes
        return(
            IsAxisAligned(right, fillable, rotationTolerancePercentage) &&
            IsAxisAligned(up, fillable, rotationTolerancePercentage) &&
            IsAxisAligned(forward, fillable, rotationTolerancePercentage)
        );

    }
    public static (bool, Orientation, Orientation) IsValidRotation_old(Transform fillable, Transform grabbable, float rotationTolerancePercentage)
    {
        // Get the object's transform axes directly
        Vector3 right = grabbable.right;     // Local X
        Vector3 up = grabbable.up;           // Local Y
        Vector3 forward = grabbable.forward; // Local Z

        // Define possible world-aligned axes
        Vector3[] validFillableAxes = {
             fillable.right,  fillable.up,  fillable.forward, 
            -fillable.right, -fillable.up, -fillable.forward
        };
        

        // Find the closest fillable-aligned vectors for each local grabbable axis
        int closestRightId = FindClosestAxis(right, validFillableAxes);
        int closestUpId = FindClosestAxis(up, validFillableAxes);
        int closestForwardId = FindClosestAxis(forward, validFillableAxes);

        Vector3 closestRight = validFillableAxes[closestRightId];
        Vector3 closestUp = validFillableAxes[closestUpId];
        Vector3 closestForward = validFillableAxes[closestForwardId];


        Vector3 rotDimUp = worldAxes[closestUpId];
        Vector3 rotDimRight = worldAxes[closestRightId];
        Vector3 rotDimForward = worldAxes[closestForwardId];


        // Check if the object is aligned with fillable axes
        bool isValid =
            IsAxisAligned(right, fillable, rotationTolerancePercentage) &&
            IsAxisAligned(up, fillable, rotationTolerancePercentage) &&
            IsAxisAligned(forward, fillable, rotationTolerancePercentage) &&
            closestRight != closestUp && closestUp != closestForward && closestRight != closestForward; // Ensure perpendicularity


        // Return validity and the full rotation axes
        // rotDimForward and rotDimUp represent the rotation difference
        Debug.Log("RotationHelperA: IsValidRotation - closestUp: " + closestUp + ", closestRight: " + closestRight + ", closestForward: " + closestForward + ", rotDimUp: " + rotDimUp + ", rotDimRight: " + rotDimRight + ", rotDimForward: " + rotDimForward);
        // Return the validity, the axis orientation of the grabbable responding to fillable, and the orientation of world axes
        return (isValid, new Orientation(closestForward, closestUp, closestRight), new Orientation (rotDimForward, rotDimUp, rotDimRight));
    }

    // Checks if a vector is close to a valid axis 
    static bool IsAxisAligned(Vector3 v, Transform validTransforms, float rotationTolerancePercentage)
    {
        float tolerance = rotationTolerancePercentage;
        //only the first three axes are needed, as the others are just the negative versions
        return (Mathf.Abs(Mathf.Abs(v.x) - 1) <= tolerance && Mathf.Abs(v.y) <= tolerance && Mathf.Abs(v.z) <= tolerance) ||
            (Mathf.Abs(Mathf.Abs(v.y) - 1) <= tolerance && Mathf.Abs(v.x) <= tolerance && Mathf.Abs(v.z) <= tolerance) ||
            (Mathf.Abs(Mathf.Abs(v.z) - 1) <= tolerance && Mathf.Abs(v.x) <= tolerance && Mathf.Abs(v.y) <= tolerance);

        /*return 
        Mathf.Abs(Mathf.Abs(v.x) - Mathf.Abs(validTransforms.right.x)) <= tolerance && 
        Mathf.Abs(Mathf.Abs(v.y) - Mathf.Abs(validTransforms.right.y)) <= tolerance && 
        Mathf.Abs(Mathf.Abs(v.z) - Mathf.Abs(validTransforms.right.z)) <= tolerance ||
        Mathf.Abs(Mathf.Abs(v.x) - Mathf.Abs(validTransforms.up.x)) <= tolerance && 
        Mathf.Abs(Mathf.Abs(v.y) - Mathf.Abs(validTransforms.up.y)) <= tolerance && 
        Mathf.Abs(Mathf.Abs(v.z) - Mathf.Abs(validTransforms.up.z)) <= tolerance ||
        Mathf.Abs(Mathf.Abs(v.x) - Mathf.Abs(validTransforms.forward.x)) <= tolerance && 
        Mathf.Abs(Mathf.Abs(v.y) - Mathf.Abs(validTransforms.forward.y)) <= tolerance && 
        Mathf.Abs(Mathf.Abs(v.z) - Mathf.Abs(validTransforms.forward.z)) <= tolerance;*/
    }

    // Finds the closest world-aligned axis to a given vector
    public static int FindClosestAxis(Vector3 axis, Vector3[] validAxes)
    {
        int closest = 0;
        float maxDot = -1f;

        for (int i = 0; i < validAxes.Length; i++)
        {
            float dot = Vector3.Dot(axis, validAxes[i]);
            if (dot > maxDot) // Find best match
            {
                maxDot = dot;
                closest = i;
            }
        }
        return closest;
    }
    public static Quaternion OrientationToQuaternion(Orientation orientation)
    {
        return Quaternion.LookRotation(orientation.forward, orientation.up);
    }
    public static Vector3Int RotateGridOffset(Vector3Int gridOffset, Vector3Int gridSize,Vector3Int rotatedGrabbableGridSize, Vector3 rotateDimensionUp, Vector3 rotateDimensionForward, Vector3 rotateDimensionRight)
    {
        (int newX, int newY, int newZ) = (gridOffset.x, gridOffset.y, gridOffset.z);
        //TTODO: Needs to be the unrotated grabbable grid size!
        Vector3Int gridOffsetUpperBound = new Vector3Int(gridOffset.x + rotatedGrabbableGridSize[0], gridOffset.y + rotatedGrabbableGridSize[1], gridOffset.z + rotatedGrabbableGridSize[2]);
        (int newXUpper, int newYUpper, int newZUpper) = (gridOffsetUpperBound.x, gridOffsetUpperBound.y, gridOffsetUpperBound.z);
        (int xRotation, int yRotation, int zRotation) = AxisRotationAmount(rotateDimensionUp, rotateDimensionRight, rotateDimensionForward);
        
        // Apply rotations to the grid offset
        // in case of Y rotation and (0,0,0) with a grid of (3,3,3), the new grid offset would be (0,0,3), then (3,0,3) and finally (3,0,0)
        // in case of Y rotation and (0,0,1) with a grid of (3,3,3), the new grid offset would be (1,0,3), then (3,0,2) and finally (2,0,0)
        // we want to REVERSE this rotation! So we apply the negative rotation
        // in case of negative Y rotation and (0,0,0) with a grid of (3,3,3), the new grid offset would be (3,0,0), then (3,0,3) and finally (0,0,3)
        for (int ix = 0; ix < xRotation; ix++)
        {
            //normal: (newY, newZ) = (newZ, gridSize.x - newY);
            // reversed:
            (newY, newZ) = (gridSize.z - newZ, newY);
            (newYUpper, newZUpper) = (gridSize.z - newZUpper, newYUpper);
            (gridSize.x, gridSize.y, gridSize.z) = RotateDimensionSize(gridSize.x, gridSize.y, gridSize.z, 1, 0, 0);
            
        }
        for (int iy = 0; iy < yRotation; iy++)
        {
            //old: (newX, newZ) = (newZ, gridSize.x - newX);
            // reversed:
            (newX, newZ) = (gridSize.x - newZ, newX);
            (newXUpper, newZUpper) = (gridSize.x - newZUpper, newXUpper);
            (gridSize.x, gridSize.y, gridSize.z) = RotateDimensionSize(gridSize.x, gridSize.y, gridSize.z, 0, 1, 0);
        }
        for (int iz = 0; iz < zRotation; iz++)
        {
            //old: (newX, newY) = (gridSize.y - newY, newX);
            // reversed:
            (newX, newY) = (newY, gridSize.y - newX);
            (newXUpper, newYUpper) = (newYUpper, gridSize.y - newXUpper);
            (gridSize.x, gridSize.y, gridSize.z) = RotateDimensionSize(gridSize.x, gridSize.y, gridSize.z, 0, 0, 1);
        }
        // in case of 90 degrees Y rotation and (0,0,0) with a grid of (3,3,3), the new grid offset would be (0,0,3), but with a rotatedGrabbableGridSize of (2,2,2), the new grid offset has to be (0,0,1) to better represent the actual position in the rotated grid
        
        Debug.Log("OffsetRotation: AxisRotationAmount - rotUp = "+ rotateDimensionForward + "rotRight = " + rotateDimensionRight + "rotForward = " + rotateDimensionUp + " xRotation: " + xRotation + ", yRotation: " + yRotation + ", zRotation: " + zRotation+", original offset: " + gridOffset + ", rotOffset grid: "+ new Vector3Int(newX, newY, newZ)+", rotOffsetUpper: "+ new Vector3Int(newXUpper, newYUpper, newZUpper)+" rotated offsetMin: " + new Vector3Int(Math.Min(newX, newXUpper), Math.Min(newY, newYUpper), Math.Min(newZ, newZUpper)));
        // min value of grid offset or upper grid offset:

        return new Vector3Int(Math.Min(newX, newXUpper), Math.Min(newY, newYUpper), Math.Min(newZ, newZUpper));
    }
}