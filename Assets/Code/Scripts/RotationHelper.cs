using System;
using UnityEngine;


public class RotationHelper
{


    // Used to rotate the voxel matrix of a grabbable object
    //Rotate a xyz matrix by xRotation, yRotation, zRotation, where each rotation is either 0, 1, 2 or 3, standing for 0, 90, 180, or 270 degrees
    //Return the rotated matrix
    public static int[][][] RotateMatrix (int[][][] matrix, int xRotation, int yRotation, int zRotation){
        Debug.Log ("Rotating matrix by " + xRotation + " " + yRotation + " " + zRotation);
        (int newXSize, int newYSize, int newZSize) = RotateDimensionSize(matrix.Length, matrix[0].Length, matrix[0][0].Length, xRotation, yRotation, zRotation);
        int[][][] rotatedMatrix = new int[newXSize][][];
        // Create new rotated matrix-array
        for (int x = 0; x < newXSize; x++)
        {
            rotatedMatrix[x] = new int[newYSize][];
            for (int y = 0; y < newYSize; y++)
            {
                rotatedMatrix[x][y] = new int[newZSize];
            }
        }
        Debug.Log ("Previous Matrix Size: " + matrix.Length + " " + matrix[0].Length + " " + matrix[0][0].Length + " and new Matrix Size: " + newXSize + " " + newYSize + " " + newZSize);
        //fill new matrix-array
        for (int x = 0; x < matrix.Length; x++)
        {
            for (int y = 0; y < matrix[x].Length; y++)
            {
                for (int z = 0; z < matrix[x][y].Length; z++)
                {
                    (int newX, int newY, int newZ) = RotateIndices(x, y, z, xRotation, yRotation, zRotation, newXSize, newYSize, newZSize);
                    try{
                       rotatedMatrix[newX][newY][newZ] = matrix[x][y][z];
                    }
                    catch (IndexOutOfRangeException e){
                        //TODO: fix bug here
                        Debug.Log("matrix size: " + matrix.Length + " " + matrix[x].Length + " " + matrix[x][y].Length +" and rotatedMatrixSize: " + newXSize + " " + newYSize + " " + newZSize + " and old coords:"+x+" "+y+" "+z+" and new Coords: " + newX + " " + newY + " " + newZ);
                        Debug.Log("Index out of range: " + e);
                        
                    }
                }
            }
        }
        return rotatedMatrix; 
    }

    //xRotation, yRotation, zRotation are either 0, 1, 2 or 3 and determine how often the matrix is rotated by 90 degrees
    //For example, if xRotation = 1, the matrix is rotated by 90 degrees around the x-axis
    // Rotate the size of a matrix int[][][], so if the matrix is (1,3,1) and is rotated once on the x-axis, the new size is (1,1,3)
    public static (int,int,int) RotateDimensionSize(int x, int y, int z, int xRotation, int yRotation, int zRotation){
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

    private static (int,int,int) RotateIndices(int x, int y, int z, int xRotation, int yRotation, int zRotation, int xLength, int yLength, int zLength)
    {
        int newX = x;
        int newY = y;
        int newZ = z;
        //Rotation around the x-axis, if i had an empty 3x3x3 grid with a 1 at (2,0,0), rotating it once would result with the 1 being at (2,2,0), once again at (2,2,2), and finally at (2,0,2)
        for (int ix = 0; ix < xRotation; ix++)
        {
            (xLength, yLength, zLength) = RotateDimensionSize(xLength, yLength, zLength, 1, 0, 0);
            (newY, newZ) = (zLength - 1 - newZ, newY);
            
        }
        //Rotation around the y-axis, if i had an empty 3x3x3 grid with a 1 at (2,0,0), rotating it once would result with the 1 being at (0,0,0), once again at (0,0,2), and finally at (2,0,2)
        for (int iy = 0; iy < yRotation; iy++)
        {
            (xLength, yLength, zLength) = RotateDimensionSize(xLength, yLength, zLength, 0, 1, 0);
            (newX, newZ) = (zLength - 1 - newZ, newX);
        }
        //Rotation around the z-axis, if i had an empty 3x3x3 grid with a 1 at (2,0,0), rotating it once would result with the 1 being at (2,2,0), once again at (0,2,0), and finally at (0,0,0)
        for (int iz = 0; iz < zRotation; iz++)
        {
            (xLength, yLength, zLength) = RotateDimensionSize(xLength, yLength, zLength, 0, 0, 1);
            (newX, newY) = (yLength - 1 - newY, newX);
        }
        return (newX, newY, newZ);
    }

    // Rotate a matrix of voxels by the orientation of the object
    public static int[][][] RotateMatrix(int[][][] matrix, Vector3 up, Vector3 right, Vector3 forward)
    {
        int sizeX = matrix.Length;
        int sizeY = matrix[0].Length;
        int sizeZ = matrix[0][0].Length;

        // Determine new dimensions based on axis swaps
        int newSizeX = Mathf.Abs(Vector3.Dot(Vector3.right, up)) > 0 ? sizeY : (Mathf.Abs(Vector3.Dot(Vector3.forward, up)) > 0 ? sizeZ : sizeX);
        int newSizeY = Mathf.Abs(Vector3.Dot(Vector3.up, up)) > 0 ? sizeY : (Mathf.Abs(Vector3.Dot(Vector3.forward, up)) > 0 ? sizeX : sizeZ);
        int newSizeZ = Mathf.Abs(Vector3.Dot(Vector3.forward, up)) > 0 ? sizeY : (Mathf.Abs(Vector3.Dot(Vector3.up, up)) > 0 ? sizeZ : sizeX);

        Debug.Log("Rotate Matrix, previous size: " + sizeX + " " + sizeY + " " + sizeZ + " and new size: " + newSizeX + " " + newSizeY + " " + newSizeZ+ " with up: " + up + " right: " + right + " forward: " + forward);
        
        // Create the new rotated matrix with the correct dimensions
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
                    Vector3 oldPos = new Vector3(x, y, z);
                    Vector3 newPos = RotateVoxelPosition(oldPos, up, right, forward, sizeX, sizeY, sizeZ);

                    rotatedMatrix[(int)newPos.x][(int)newPos.y][(int)newPos.z] = matrix[x][y][z];
                }
            }
        }

        return rotatedMatrix;
    }

    private static Vector3 RotateVoxelPosition(Vector3 pos, Vector3 up, Vector3 right, Vector3 forward, int xLength, int yLength, int zLength)
    {
        int newX = 0, newY = 0, newZ = 0;

        // Determine the new position based on orientation
        //Default forward ist -z. up is y, right is x
        (xLength, yLength, zLength) = RotateDimensionSize(xLength, yLength, zLength, up, right, forward);
        if (up == Vector3.up) //only rotation around y-axis
        {
            if(forward == Vector3.forward){ //up is y, right is x, forward is z
                (newX, newY, newZ) = ((int)pos.x, (int)pos.y, (int)pos.z);
            }
            else if(forward == Vector3.back){ //up = up, forward = back means a rotation of 180 degrees around the y-axis
                (newX, newY, newZ) = (xLength - 1 - (int)pos.x, (int)pos.y, zLength - 1 - (int)pos.z);
            }
            else if(forward == Vector3.right){ //up = up, forward = right means a rotation of 90 degrees around the y-axis
                (newX, newY, newZ) = (zLength - 1 - (int)pos.z, (int)pos.y, (int)pos.x);

            }
            else if(forward == Vector3.left){
                (newX, newY, newZ) = ((int)pos.z, (int)pos.y, xLength - 1 - (int)pos.x);
            }
        }
        else if (up == Vector3.down) // rotation of 180 degree on the x-axis
        {
            if(forward == Vector3.forward){ //up is -y, right is -x, forward is z
                (newX, newY, newZ) = (xLength - 1 - (int)pos.x, yLength - 1 - (int)pos.y, (int)pos.z);
            }
            else if(forward == Vector3.back){ //up = down, forward = back means a rotation of 180 degrees around the y-axis
                (newX, newY, newZ) = ((int)pos.x, yLength - 1 - (int)pos.y, zLength - 1 - (int)pos.z);
            }
            else if(forward == Vector3.right){ //up = down, forward = right means a rotation of 90 degrees around the y-axis
                (newX, newY, newZ) = (zLength - 1 - (int)pos.z, yLength - 1 - (int)pos.y, (int)pos.x);
            }
            else if(forward == Vector3.left){
                (newX, newY, newZ) = ((int)pos.z, yLength - 1 - (int)pos.y, xLength - 1 - (int)pos.x);
            }
        }
        else if (up == Vector3.right)
        {
            if(forward == Vector3.forward){ //up is x, right is -y, forward is z
                (newX, newY, newZ) = (yLength - 1 - (int)pos.y, (int)pos.x, (int)pos.z);
            }
            else if(forward == Vector3.back){ //up = right, forward = back means a rotation of 180 degrees around the y-axis
                (newX, newY, newZ) = (yLength - 1 - (int)pos.y, xLength - 1 - (int)pos.x, zLength - 1 - (int)pos.z);
                //TODO: check and do the rest
            }
            else if(forward == Vector3.up){
        }
            else if (up == Vector3.left)
            {
            
            }
            else if (up == Vector3.forward)
            {
                
            }
            else if (up == Vector3.back)
            {
                
            }
            }
        return new Vector3(newX, newY, newZ);
        
    }
    public static (int,int,int) RotateDimensionSize(int x, int y, int z, Vector3 up, Vector3 right, Vector3 forward)
    {
        int newX = 0, newY = 0, newZ = 0;

        // Determine the new dimensions based on orientation
        if (up == Vector3.up || up == Vector3.down)
        {
            newX = x;
            newY = y;
            newZ = z;
        }
        else if (up == Vector3.right || up == Vector3.left)
        {
            newX = y;
            newY = x;
            newZ = z;
        }
        else if (up == Vector3.forward || up == Vector3.back)
        {
            newX = x;
            newY = z;
            newZ = y;
        }

        // Handle forward axis flips
        if (forward == Vector3.left){
            newX = x;
            newY = y;
            newZ = z;
        }
        else if (forward == Vector3.up){
            newX = x;
            newY = y;
            newZ = z;
        }
        else if (forward == Vector3.back){
            newX = x;
            newY = y;
            newZ = z;
        }

        return (newX, newY, newZ);
    }
    public static (bool, Vector3, Vector3, Vector3) IsValidRotation(Transform grabbable, float rotationTolerancePercentage)
    {
        // Get the object's transform axes directly
        Vector3 right = grabbable.transform.right;     // Local X
        Vector3 up = grabbable.transform.up;           // Local Y
        Vector3 forward = grabbable.transform.forward; // Local Z

        // Define possible world-aligned axes
        Vector3[] validAxes = { 
            Vector3.right, Vector3.left, 
            Vector3.up, Vector3.down, 
            Vector3.forward, Vector3.back 
        };

        // Find the closest world-aligned vectors for each local axis
        Vector3 closestRight = FindClosestAxis(right, validAxes);
        Vector3 closestUp = FindClosestAxis(up, validAxes);
        Vector3 closestForward = FindClosestAxis(forward, validAxes);

        // Check if the object is aligned with world axes
        bool isValid = 
            IsAxisAligned(right, rotationTolerancePercentage) &&
            IsAxisAligned(up, rotationTolerancePercentage) &&
            IsAxisAligned(forward, rotationTolerancePercentage) &&
            closestRight != closestUp && closestUp != closestForward && closestRight != closestForward; // Ensure perpendicularity

        // Return validity and the full rotation axes
        return (isValid, closestUp, closestRight, closestForward);
    }

    // Checks if a vector is close to a valid axis (±1,0,0), (0,±1,0), (0,0,±1)
    static bool IsAxisAligned(Vector3 v, float rotationTolerancePercentage)
    {
        float tolerance = rotationTolerancePercentage;
        
        return (Mathf.Abs(Mathf.Abs(v.x) - 1) <= tolerance && Mathf.Abs(v.y) <= tolerance && Mathf.Abs(v.z) <= tolerance) ||
            (Mathf.Abs(Mathf.Abs(v.y) - 1) <= tolerance && Mathf.Abs(v.x) <= tolerance && Mathf.Abs(v.z) <= tolerance) ||
            (Mathf.Abs(Mathf.Abs(v.z) - 1) <= tolerance && Mathf.Abs(v.x) <= tolerance && Mathf.Abs(v.y) <= tolerance);
    }

    // Finds the closest world-aligned axis to a given vector
    static Vector3 FindClosestAxis(Vector3 axis, Vector3[] validAxes)
    {
        Vector3 closest = validAxes[0];
        float maxDot = -1f;

        foreach (var valid in validAxes)
        {
            float dot = Vector3.Dot(axis, valid);
            if (dot > maxDot) // Find best match
            {
                maxDot = dot;
                closest = valid;
            }
        }

        return closest;
    }
}