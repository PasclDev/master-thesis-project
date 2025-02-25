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
    public static bool IsValidRotation(Transform grabbable, float rotationTolerancePercentage)
    {
        // Get the object's rotation as a matrix
        Matrix4x4 matrix = Matrix4x4.Rotate(grabbable.rotation);
        
        // Extract the local axes
        Vector3 right = matrix.GetColumn(0);  // Local X
        Vector3 up = matrix.GetColumn(1);     // Local Y
        Vector3 forward = matrix.GetColumn(2); // Local Z

        // Check if all axes align to world axes (±1,0,0), (0,±1,0), (0,0,±1)
        return IsAxisAligned(right, rotationTolerancePercentage) && IsAxisAligned(up, rotationTolerancePercentage) && IsAxisAligned(forward, rotationTolerancePercentage);
    }

    static bool IsAxisAligned(Vector3 v, float rotationTolerancePercentage)
    {
        // percentage of tolerance, 0.1f equals 10% tolerance
        // Check if the vector is approximately (±1,0,0), (0,±1,0), or (0,0,±1)
        float tolerance = rotationTolerancePercentage;
        return (Mathf.Abs(Mathf.Abs(v.x) - 1) <= tolerance && Mathf.Abs(v.y) <= tolerance && Mathf.Abs(v.z) <= tolerance) ||
            (Mathf.Abs(Mathf.Abs(v.y) - 1) <= tolerance && Mathf.Abs(v.x) <= tolerance && Mathf.Abs(v.z) <= tolerance) ||
            (Mathf.Abs(Mathf.Abs(v.z) - 1) <= tolerance && Mathf.Abs(v.x) <= tolerance && Mathf.Abs(v.y) <= tolerance);
    }
}
